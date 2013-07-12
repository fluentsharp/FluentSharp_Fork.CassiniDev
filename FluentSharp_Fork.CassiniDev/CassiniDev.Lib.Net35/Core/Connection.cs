//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using CassiniDev.ServerLog;
using FluentSharp.CoreLib;

#endregion

namespace CassiniDev
{
    /// <summary>
    /// 
    /// </summary>
    public class Connection : MarshalByRefObject
    {
        
        public int HttpForbidden = 403;

        // ReSharper disable InconsistentNaming
        public int HttpOK = 200;
        // ReSharper restore InconsistentNaming

        public MemoryStream _responseContent;

        public Server _server;
        public LogInfo _requestLog;
        public LogInfo _responseLog;


        public Socket _socket;

        public Connection(Server server, Socket socket)
        {
            
            Id = Guid.NewGuid();
            _responseContent = new MemoryStream();
            _server = server;
            _socket = socket;
            InitializeLogInfo();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Connected
        {
            get { return _socket.Connected; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        // ReSharper disable InconsistentNaming
        public string LocalIP
        // ReSharper restore InconsistentNaming
        {
            get
            {
                IPEndPoint ep = (IPEndPoint)_socket.LocalEndPoint;
                return (ep != null && ep.Address != null) ? ep.Address.ToString() : "127.0.0.1";
            }
        }


        // ReSharper disable InconsistentNaming
        /// <summary>
        /// 
        /// </summary>
        public string RemoteIP
        // ReSharper restore InconsistentNaming
        {
            get
            {
                IPEndPoint ep = (IPEndPoint)_socket.RemoteEndPoint;
                return (ep != null && ep.Address != null) ? ep.Address.ToString() : "127.0.0.1";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public LogInfo RequestLog
        {
            get { return _requestLog; }
        }

        /// <summary>
        /// 
        /// </summary>
        public LogInfo ResponseLog
        {
            get { return _responseLog; }
        }

        /// <summary>
        /// #TODO: check to see if unqualified calls to Close() are the reason for the request latency
        /// </summary>
        public void Close()
        {
            FinalizeLogInfo();

            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }
            finally
            {
                _socket = null;
            }
        }

        /// <summary>
        /// </summary>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathTranslated"></param>
        /// <param name="url"></param>
        public void LogRequest(string pathTranslated, string url)
        {
            _requestLog.PathTranslated = pathTranslated;

            _requestLog.Url = url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public void LogRequestBody(byte[] content)
        {
            _requestLog.Body = content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headers"></param>
        public void LogRequestHeaders(string headers)
        {
            _requestLog.Headers = headers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxBytes"></param>
        /// <returns></returns>
        public byte[] ReadRequestBytes(int maxBytes)
        {
            try
            {
                if (WaitForRequestBytes() == 0)
                {
                    return null;
                }

                int numBytes = _socket.Available;

                if (numBytes > maxBytes)
                {
                    numBytes = maxBytes;
                }

                int numReceived = 0;

                byte[] buffer = new byte[numBytes];

                if (numBytes > 0)
                {
                    numReceived = _socket.Receive(buffer, 0, numBytes, SocketFlags.None);
                }

                if (numReceived < numBytes)
                {
                    byte[] tempBuffer = new byte[numReceived];

                    if (numReceived > 0)
                    {
                        Buffer.BlockCopy(buffer, 0, tempBuffer, 0, numReceived);
                    }

                    buffer = tempBuffer;
                }

                return buffer;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int WaitForRequestBytes()
        {
            int availBytes = 0;

            try
            {
                if (_socket.Available == 0)
                {
                    _socket.Poll(100000, SelectMode.SelectRead);

                    if (_socket.Available == 0 && _socket.Connected)
                    {
                        _socket.Poll(30000000, SelectMode.SelectRead);
                    }
                }

                availBytes = _socket.Available;
            }            
            catch(Exception ex)            
            {
                ex.log();
            }

            return availBytes;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Write100Continue()
        {
            WriteEntireResponseFromString(100, null, null, true);
        }

        public void Write200Continue()
        {
            WriteEntireResponseFromString(200, null, string.Empty, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void WriteBody(byte[] data, int offset, int length)
        {
            try
            {
                _responseContent.Write(data, 0, data.Length);
                _socket.Send(data, offset, length, SocketFlags.None);
            }
            catch (SocketException)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="keepAlive"></param>
        public void WriteEntireResponseFromFile(String fileName, bool keepAlive)
        {
            if (!File.Exists(fileName))
            {
                WriteErrorAndClose(404);
                return;
            }

            // Deny the request if the contentType cannot be recognized.

            string contentType = _server.GetContentType(fileName);

            
            if (contentType == null)
            {
                //#TODO:
                // no need to deny unknown content types in cassinidev
                // contentType = "text/plain";
                WriteErrorAndClose(HttpForbidden);
                return;
            }

            string contentTypeHeader = "Content-Type: " + contentType + "\r\n";

            bool completed = false;
            FileStream fs = null;

            try
            {
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                int len = (int)fs.Length;
                byte[] fileBytes = new byte[len];
                int bytesRead = fs.Read(fileBytes, 0, len);

                String headers = MakeResponseHeaders(HttpOK, contentTypeHeader, bytesRead, keepAlive);
                _responseLog.Headers = headers;
                _responseLog.StatusCode = HttpOK;
                _socket.Send(Encoding.UTF8.GetBytes(headers));

                _socket.Send(fileBytes, 0, bytesRead, SocketFlags.None);

                completed = true;
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (!keepAlive || !completed)
                {
                    Close();
                }

                if (fs != null)
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="extraHeaders"></param>
        /// <param name="body"></param>
        /// <param name="keepAlive"></param>
        public void WriteEntireResponseFromString(int statusCode, String extraHeaders, String body, bool keepAlive)
        {
            try
            {
                int bodyLength = (body != null) ? Encoding.UTF8.GetByteCount(body) : 0;
                string headers = MakeResponseHeaders(statusCode, extraHeaders, bodyLength, keepAlive);

                _responseLog.Headers = headers;
                _responseLog.StatusCode = statusCode;
                _socket.Send(Encoding.UTF8.GetBytes(headers + body));
            }
            catch (SocketException)
            {
            }
            finally
            {
                if (!keepAlive)
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        public void WriteErrorAndClose(int statusCode, string message)
        {
            WriteEntireResponseFromString(statusCode, null, GetErrorResponseBody(statusCode, message), false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCode"></param>
        public void WriteErrorAndClose(int statusCode)
        {
            WriteErrorAndClose(statusCode, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="extraHeaders"></param>
        public void WriteErrorWithExtraHeadersAndKeepAlive(int statusCode, string extraHeaders)
        {
            WriteEntireResponseFromString(statusCode, extraHeaders, GetErrorResponseBody(statusCode, null), true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="extraHeaders"></param>
        public void WriteHeaders(int statusCode, String extraHeaders)
        {
            string headers = MakeResponseHeaders(statusCode, extraHeaders, -1, false);

            _responseLog.Headers = headers;
            _responseLog.StatusCode = statusCode;

            try
            {
                _socket.Send(Encoding.UTF8.GetBytes(headers));
            }
            catch (SocketException)
            {
            }
        }


        public void FinalizeLogInfo()
        {
            try
            {
                _responseLog.Body = _responseContent.ToArray();
                _responseContent.Dispose();
                _responseLog.Created = DateTime.Now;
                _responseLog.Url = _requestLog.Url;
                _responseLog.PathTranslated = _requestLog.PathTranslated;
                _responseLog.Identity = _requestLog.Identity;
                _responseLog.PhysicalPath = _requestLog.PhysicalPath;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // log error to text
            }
        }

        public string GetErrorResponseBody(int statusCode, string message)
        {
            string body = Messages.FormatErrorMessageBody(statusCode, _server.VirtualPath);

            if (!string.IsNullOrEmpty(message))
            {
                body += "\r\n<!--\r\n" + message + "\r\n-->";
            }

            return body;
        }

        public void InitializeLogInfo()
        {
            _requestLog = new LogInfo
                {
                    Created = DateTime.Now,
                    ConversationId = Id,
                    RowType = 1,
                    Identity = _server.GetProcessUser(),
                    PhysicalPath = _server.PhysicalPath
                };

            _responseLog = new LogInfo
                {
                    ConversationId = Id,
                    RowType = 2
                };
        }

        public static string MakeResponseHeaders(int statusCode, string moreHeaders, int contentLength, bool keepAlive)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("HTTP/1.1 " + statusCode + " " + HttpWorkerRequest.GetStatusDescription(statusCode) + "\r\n");
            sb.Append("Server: Cassini/" + Messages.VersionString + "\r\n");
            sb.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");

            if (contentLength >= 0)
            {
                sb.Append("Content-Length: " + contentLength + "\r\n");
            }

            if (moreHeaders != null)
            {
                sb.Append(moreHeaders);
            }

            if (!keepAlive)
            {
                sb.Append("Connection: Close\r\n");
            }

            sb.Append("\r\n");

            return sb.ToString();
        }
    }
}