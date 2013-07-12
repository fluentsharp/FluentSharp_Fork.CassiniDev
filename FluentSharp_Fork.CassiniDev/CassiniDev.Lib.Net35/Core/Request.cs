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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Hosting;
using FluentSharp.CoreLib;
using Microsoft.Win32.SafeHandles;
using System.Security.Principal;
//

#endregion

namespace CassiniDev
{
    public class Request : SimpleWorkerRequest
    {



        #region Constants
        public const int MaxChunkLength = 64 * 1024;

        public const int MaxHeaderBytes = 32 * 1024;

        public static readonly char[] BadPathChars = new[] { '%', '>', '<', ':', '\\' };

        public static readonly string[] DefaultFileNames = new[] { "default.aspx", "default.htm", "default.html" };

        public static readonly char[] IntToHex = new[]
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
            };

        public static readonly string[] RestrictedDirs = new[]
            {
                "/bin",
                "/app_browsers",
                "/app_code",
                "/app_data",
                "/app_localresources",
                "/app_globalresources",
                "/app_webreferences"
            };

        #endregion

        #region exposable members
        public string _allRawHeaders;

        public byte[] _body;

        public int _bodyLength;
        public int _contentLength;

        // security permission to Assert remoting calls to _connection
        public int _endHeadersOffset;

        public string _filePath;

        public byte[] _headerBytes;

        public List<ByteString> _headerByteStrings;
        public bool _headersSent;

        // parsed request data

        public bool _isClientScriptPath;

        public string[] _knownRequestHeaders;

        public string _path;

        public string _pathInfo;

        public string _pathTranslated;

        public string _protocol;

        public string _queryString;
        public byte[] _queryStringBytes;
        public List<byte[]> _responseBodyBytes;

        public int _responseStatus;

        public bool _specialCaseStaticFileHeaders;

        public int _startHeadersOffset;

        public string[][] _unknownRequestHeaders;

        public string _url;

        public string _verb;

        #endregion

        public readonly IStackWalk _connectionPermission = new PermissionSet(PermissionState.Unrestricted);
        public readonly Host _host;
        public readonly Server _server;
        public Connection _connection;
        public StringBuilder _responseHeadersBuilder;
        public NtlmAuth _auth;



        public Request(Server server, Host host, Connection connection)
            : base(String.Empty, String.Empty, null)
        {
            _connectionPermission = new PermissionSet(PermissionState.Unrestricted);
            _server = server;
            _host = host;
            _connection = connection;
        }

        public override void CloseConnection()
        {
            _connectionPermission.Assert();
            _connection.Close();
        }

        public override void EndOfRequest()
        {
            Connection conn = _connection;
            if (conn != null)
            {
                _connection = null;
                _server.OnRequestEnd(conn, GetProcessUser());
            }

            if (_auth != null)
            {
                _auth.Dispose(); //
                _auth = null;
            }

        }

        public override void FlushResponse(bool finalFlush)
        {
            if (_responseStatus == 404 && !_headersSent && finalFlush && _verb == "GET")
            {
                // attempt directory listing
                if (ProcessDirectoryListingRequest())
                {
                    return;
                }
            }

            _connectionPermission.Assert();

            if (!_headersSent)
            {
                _connection.WriteHeaders(_responseStatus, _responseHeadersBuilder.ToString());

                _headersSent = true;
            }
            foreach (byte[] bytes in _responseBodyBytes)
            {
                _connection.WriteBody(bytes, 0, bytes.Length);
            }

            _responseBodyBytes = new List<byte[]>();

            if (finalFlush)
            {
                _connection.Close();
            }
        }

        public override string GetAppPath()
        {
            return _host.VirtualPath;
        }

        public override string GetAppPathTranslated()
        {
            return _host.PhysicalPath;
        }

        public override string GetFilePath()
        {
            return _filePath;
        }

        public override string GetFilePathTranslated()
        {
            return _pathTranslated;
        }

        public override string GetHttpVerbName()
        {
            return _verb;
        }

        public override string GetHttpVersion()
        {
            return _protocol;
        }

        public override string GetKnownRequestHeader(int index)
        {
            return _knownRequestHeaders[index];
        }

        public override string GetLocalAddress()
        {
            _connectionPermission.Assert();
            return _connection.LocalIP;
        }

        public override int GetLocalPort()
        {
            return _host.Port;
        }

        public override string GetPathInfo()
        {
            return _pathInfo;
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return _body;
        }

        public override string GetQueryString()
        {
            return _queryString;
        }

        public override byte[] GetQueryStringRawBytes()
        {
            return _queryStringBytes;
        }

        public override string GetRawUrl()
        {
            return _url;
        }

        public override string GetRemoteAddress()
        {
            _connectionPermission.Assert();
            return _connection.RemoteIP;
        }

        public override int GetRemotePort()
        {
            return 0;
        }

        public override string GetServerName()
        {
            string localAddress = GetLocalAddress();
            if (localAddress.Equals("127.0.0.1"))
            {
                return "localhost";
            }
            return localAddress;
        }

        public override string GetServerVariable(string name)
        {
            string processUser = string.Empty;
            string str2 = name;
            if (str2 == null)
            {
                return processUser;
            }
            if (str2 != "ALL_RAW")
            {
                if (str2 != "SERVER_PROTOCOL")
                {
                    if (str2 == "LOGON_USER")
                    {
                        if (GetUserToken() != IntPtr.Zero)
                        {
                            processUser = _host.GetProcessUser();
                        }
                        return processUser;
                    }
                    if ((str2 == "AUTH_TYPE") && (GetUserToken() != IntPtr.Zero))
                    {
                        processUser = "NTLM";
                    }
                    return processUser;
                }
            }
            else
            {
                return _allRawHeaders;
            }
            return _protocol;
        }

        public override string GetUnknownRequestHeader(string name)
        {
            int n = _unknownRequestHeaders.Length;

            for (int i = 0; i < n; i++)
            {
                if (string.Compare(name, _unknownRequestHeaders[i][0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return _unknownRequestHeaders[i][1];
                }
            }

            return null;
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            return _unknownRequestHeaders;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////
        // Implementation of HttpWorkerRequest

        public override string GetUriPath()
        {
            return _path;
        }

        public Connection GetConnection()
        {
            return _connection;
        }


        public override IntPtr GetUserToken()
        {
            if (_auth == null)
                return _host.GetProcessToken();
            else
                return _auth.Token;
        }

        public string GetProcessUser()
        {
            if (_auth == null)
                return _host.GetProcessUser();
            else
            {
                using (WindowsIdentity identity = new WindowsIdentity(_auth.Token))
                    return identity.Name;
            }
        }


        public override bool HeadersSent()
        {
            return _headersSent;
        }

        public override bool IsClientConnected()
        {
            _connectionPermission.Assert();
            return _connection.Connected;
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return (_contentLength == _bodyLength);
        }

        public override string MapPath(string path)
        {
            string mappedPath;
            bool isClientScriptPath;

            if (string.IsNullOrEmpty(path) || path.Equals("/"))
            {
                // asking for the site root
                mappedPath = _host.VirtualPath == "/" ? _host.PhysicalPath : Environment.SystemDirectory;
            }
            else if (_host.IsVirtualPathAppPath(path))
            {
                // application path
                mappedPath = _host.PhysicalPath;
            }
            else if (_host.IsVirtualPathInApp(path, out isClientScriptPath))
            {
                if (isClientScriptPath)
                {
                    mappedPath = _host.PhysicalClientScriptPath +
                                 path.Substring(_host.NormalizedClientScriptPath.Length);
                }
                else
                {
                    // inside app but not the app path itself
                    mappedPath = _host.PhysicalPath + path.Substring(_host.NormalizedVirtualPath.Length);
                }
            }
            else
            {
                // outside of app -- make relative to app path
                if (path.StartsWith("/", StringComparison.Ordinal))
                {
                    mappedPath = _host.PhysicalPath + path.Substring(1);
                }
                else
                {
                    mappedPath = _host.PhysicalPath + path;
                }
            }

            mappedPath = mappedPath.Replace('/', '\\');

            if (mappedPath.EndsWith("\\", StringComparison.Ordinal) &&
                !mappedPath.EndsWith(":\\", StringComparison.Ordinal))
            {
                mappedPath = mappedPath.Substring(0, mappedPath.Length - 1);
            }

            return mappedPath;
        }




        [AspNetHostingPermission(SecurityAction.Assert, Level = AspNetHostingPermissionLevel.Medium)]
        public void Process()
        {
            // read the request
            if (!TryParseRequest())
            {
                return;
            }


            // #TODO: hook for InterceptRequest - this is a total hack #HACK i say 

            var args = new RequestInfoArgs
                           {
                               Body = _body,
                               Url = _url,
                               Verb = _verb,
                               ProcessUser = GetProcessUser(),
                               Protocol = _protocol,
                               QueryString = _queryString
                           };

            _server.OnProcessRequest(ref args);


            string extraHeaders = args.ExtraHeaders;
            if (!string.IsNullOrEmpty(extraHeaders) && !extraHeaders.EndsWith("\r\n"))
            {
                extraHeaders = extraHeaders + "\r\n";
            }

            if (!args.Continue)
            {
                //#TODO: keepalive needs to be determined
                _connection.WriteEntireResponseFromString(200, extraHeaders, args.Response, false);
                return;
            }



            if (!string.IsNullOrEmpty(extraHeaders))
            {
                _responseHeadersBuilder.Append(extraHeaders);
            }


            // 100 response to POST););
            if (_verb == "POST" && _contentLength > 0 && _bodyLength < _contentLength)
            {
                _connection.Write100Continue();
            }
            if (!_host.RequireAuthentication || TryNtlmAuthenticate())
            {
                // special case for client script
                if (_isClientScriptPath)
                {
                    _connection.WriteEntireResponseFromFile(
                        _host.PhysicalClientScriptPath + _path.Substring(_host.NormalizedClientScriptPath.Length), false);
                    return;
                }

                // deny access to code, bin, etc.
                if (IsRequestForRestrictedDirectory())
                {
                    _connection.WriteErrorAndClose(403);
                    return;
                }

                // special case for a request to a directory (ensure / at the end and process default documents)
                if (ProcessDirectoryRequest())
                {
                    return;
                }


                PrepareResponse();

                // Hand the processing over to HttpRuntime
                HttpRuntime.ProcessRequest(this);
            }
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            int bytesRead = 0;

            _connectionPermission.Assert();
            byte[] bytes = _connection.ReadRequestBytes(size);

            if (bytes != null && bytes.Length > 0)
            {
                bytesRead = bytes.Length;
                Buffer.BlockCopy(bytes, 0, buffer, 0, bytesRead);
            }

            return bytesRead;
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            if (!_headersSent)
            {
                _responseHeadersBuilder.Append("Content-Length: ");
                _responseHeadersBuilder.Append(contentLength.ToString(CultureInfo.InvariantCulture));
                _responseHeadersBuilder.Append("\r\n");
            }
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            if (_headersSent)
            {
                return;
            }

            switch (index)
            {
                case HeaderServer:
                case HeaderDate:
                case HeaderConnection:
                    // ignore these
                    return;
                case HeaderAcceptRanges:
                    // FIX: #14359
                    if (value != "bytes")
                    {
                        // use this header to detect when we're processing a static file
                        break;
                    }
                    _specialCaseStaticFileHeaders = true;
                    return;

                case HeaderExpires:
                case HeaderLastModified:
                    // FIX: #14359
                    if (!_specialCaseStaticFileHeaders)
                    {
                        // NOTE: Ignore these for static files. These are generated
                        //       by the StaticFileHandler, but they shouldn't be.
                        break;
                    }
                    return;


                // FIX: #12506
                case HeaderContentType:

                    string contentType = null;

                    if (value == "application/octet-stream")
                    {
                        // application/octet-stream is default for unknown so lets
                        // take a shot at determining the type.
                        // don't do this for other content-types as you are going to
                        // end up sending text/plain for endpoints that are handled by
                        // asp.net such as .aspx, .asmx, .axd, etc etc
                        contentType = _server.GetContentType(_pathTranslated);
                    }
                    value = contentType ?? value;
                    break;
            }

            _responseHeadersBuilder.Append(GetKnownResponseHeaderName(index));
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        public void SetResponseHeader(string name, string value)
        {
            _responseHeadersBuilder.Append(name);
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            if (length == 0)
            {
                return;
            }

            FileStream f = null;
            try
            {
                f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                SendResponseFromFileStream(f, offset, length);
            }
            finally
            {
                if (f != null)
                {
                    f.Close();
                }
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (length == 0)
            {
                return;
            }

            using (var sfh = new SafeFileHandle(handle, false))
            {
                using (var f = new FileStream(sfh, FileAccess.Read))
                {
                    SendResponseFromFileStream(f, offset, length);
                }
            }
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (length > 0)
            {
                var bytes = new byte[length];

                Buffer.BlockCopy(data, 0, bytes, 0, length);
                _responseBodyBytes.Add(bytes);
            }
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            _responseStatus = statusCode;
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (_headersSent)
                return;

            _responseHeadersBuilder.Append(name);
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        public bool IsBadPath()
        {
            
            if (_path == null)                           // Fixes bug with Cassini where it can be brought down with a simple bad first request
                return true;
            if (_path.IndexOfAny(BadPathChars) >= 0)
            {
                Trace.TraceError("Path contains bad characters {1} : {0}", _path, BadPathChars);
                return true;
            }

            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(_path, "..", CompareOptions.Ordinal) >= 0)
            {
                Trace.TraceError("Path contains relative modifier '..' : {0}", _path);
                return true;
            }

            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(_path, "//", CompareOptions.Ordinal) >= 0)
            {
                Trace.TraceError("Path contains empty segment '//' : {0}", _path);
                return true;
            }

            return false;
        }

        public bool IsRequestForRestrictedDirectory()
        {
            String p = CultureInfo.InvariantCulture.TextInfo.ToLower(_path);

            if (_host.VirtualPath != "/")
            {
                p = p.Substring(_host.VirtualPath.Length);
            }

            foreach (String dir in RestrictedDirs)
            {
                if (p.StartsWith(dir, StringComparison.Ordinal))
                {
                    if (p.Length == dir.Length || p[dir.Length] == '/')
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ParseHeaders()
        {
            _knownRequestHeaders = new string[RequestHeaderMaximum];

            // construct unknown headers as array list of name1,value1,...
            var headers = new List<string>();

            for (int i = 1; i < _headerByteStrings.Count; i++)
            {
                string s = _headerByteStrings[i].GetString();

                int c = s.IndexOf(':');

                if (c >= 0)
                {
                    string name = s.Substring(0, c).Trim();
                    string value = s.Substring(c + 1).Trim();

                    // remember
                    int knownIndex = GetKnownRequestHeaderIndex(name);
                    if (knownIndex >= 0)
                    {
                        _knownRequestHeaders[knownIndex] = value;
                    }
                    else
                    {
                        headers.Add(name);
                        headers.Add(value);
                    }
                }
            }

            // copy to array unknown headers

            int n = headers.Count / 2;
            _unknownRequestHeaders = new string[n][];
            int j = 0;

            for (int i = 0; i < n; i++)
            {
                _unknownRequestHeaders[i] = new string[2];
                _unknownRequestHeaders[i][0] = headers[j++];
                _unknownRequestHeaders[i][1] = headers[j++];
            }

            // remember all raw headers as one string

            if (_headerByteStrings.Count > 1)
            {
                _allRawHeaders = Encoding.UTF8.GetString(_headerBytes, _startHeadersOffset,
                                                         _endHeadersOffset - _startHeadersOffset);
            }
            else
            {
                _allRawHeaders = String.Empty;
            }
        }

        public void ParsePostedContent()
        {
            _contentLength = 0;
            _bodyLength = 0;

            string contentLengthValue = _knownRequestHeaders[HeaderContentLength];
            if (contentLengthValue != null)
            {
                try
                {
                    _contentLength = Int32.Parse(contentLengthValue, CultureInfo.InvariantCulture);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }

            if (_headerBytes.Length > _endHeadersOffset)
            {
                _bodyLength = _headerBytes.Length - _endHeadersOffset;

                if (_bodyLength > _contentLength)
                {
                    _bodyLength = _contentLength; // don't read more than the content-length
                }

                if (_bodyLength > 0)
                {
                    _body = new byte[_bodyLength];
                    Buffer.BlockCopy(_headerBytes, _endHeadersOffset, _body, 0, _bodyLength);
                    _connection.LogRequestBody(_body);
                }
            }
        }

        public void ParseRequestLine()
        {
            ByteString requestLine = _headerByteStrings[0];
            ByteString[] elems = requestLine.Split(' ');

            if (elems == null || elems.Length < 2 || elems.Length > 3)
            {
                _connection.WriteErrorAndClose(400);
                return;
            }

            _verb = elems[0].GetString();

            ByteString urlBytes = elems[1];
            _url = urlBytes.GetString();

            _protocol = elems.Length == 3 ? elems[2].GetString() : "HTTP/1.0";

            // query string

            int iqs = urlBytes.IndexOf('?');
            _queryStringBytes = iqs > 0 ? urlBytes.Substring(iqs + 1).GetBytes() : new byte[0];

            iqs = _url.IndexOf('?');
            if (iqs > 0)
            {
                _path = _url.Substring(0, iqs);
                _queryString = _url.Substring(iqs + 1);
            }
            else
            {
                _path = _url;
                _queryStringBytes = new byte[0];
            }

            // url-decode path

            if (_path.IndexOf('%') >= 0)
            {
                _path = HttpUtility.UrlDecode(_path, Encoding.UTF8);

                iqs = _url.IndexOf('?');
                if (iqs >= 0)
                {
                    _url = _path + _url.Substring(iqs);
                }
                else
                {
                    _url = _path;
                }
            }

            // path info

            int lastDot = _path.LastIndexOf('.');
            int lastSlh = _path.LastIndexOf('/');

            if (lastDot >= 0 && lastSlh >= 0 && lastDot < lastSlh)
            {
                int ipi = _path.IndexOf('/', lastDot);
                _filePath = _path.Substring(0, ipi);
                _pathInfo = _path.Substring(ipi);
            }
            else
            {
                _filePath = _path;
                _pathInfo = String.Empty;
            }

            _pathTranslated = MapPath(_filePath);

            _connection.LogRequest(_pathTranslated, _url);
        }

        public void PrepareResponse()
        {
            _headersSent = false;
            _responseStatus = 200;
            _responseHeadersBuilder = new StringBuilder();
            _responseBodyBytes = new List<byte[]>();



        }

        public bool ProcessDirectoryListingRequest()
        {
            if (_verb != "GET")
            {
                return false;
            }

            String dirPathTranslated = _pathTranslated;

            if (_pathInfo.Length > 0)
            {
                // directory path can never have pathInfo
                dirPathTranslated = MapPath(_path);
            }

            if (!Directory.Exists(dirPathTranslated))
            {
                return false;
            }

            // get all files and subdirs
            FileSystemInfo[] infos = null;
            try
            {
                infos = (new DirectoryInfo(dirPathTranslated)).GetFileSystemInfos();
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }

            // determine if parent is appropriate
            string parentPath = null;

            if (_path.Length > 1)
            {
                int i = _path.LastIndexOf('/', _path.Length - 2);

                parentPath = (i > 0) ? _path.Substring(0, i) : "/";
                if (!_host.IsVirtualPathInApp(parentPath))
                {
                    parentPath = null;
                }
            }

            _connection.WriteEntireResponseFromString(200, "Content-type: text/html; charset=utf-8\r\n",
                                                      Messages.FormatDirectoryListing(_path, parentPath, infos),
                                                      false);
            return true;
        }

        public bool ProcessDirectoryRequest()
        {
            String dirPathTranslated = _pathTranslated;

            if (_pathInfo.Length > 0)
            {
                // directory path can never have pathInfo
                dirPathTranslated = MapPath(_path);
            }

            if (!Directory.Exists(dirPathTranslated))
            {
                return false;
            }

            // have to redirect /foo to /foo/ to allow relative links to work
            if (!_path.EndsWith("/", StringComparison.Ordinal))
            {
                string newPath = _path + "/";
                string location = "Location: " + UrlEncodeRedirect(newPath) + "\r\n";
                string body = "<html><head><title>Object moved</title></head><body>\r\n" +
                              "<h2>Object moved to <a href='" + newPath + "'>here</a>.</h2>\r\n" +
                              "</body></html>\r\n";

                _connection.WriteEntireResponseFromString(302, location, body, false);
                return true;
            }

            // check for the default file
            foreach (string filename in DefaultFileNames)
            {
                string defaultFilePath = dirPathTranslated + "\\" + filename;

                if (File.Exists(defaultFilePath))
                {
                    // pretend the request is for the default file path
                    _path += filename;
                    _filePath = _path;
                    _url = (_queryString != null) ? (_path + "?" + _queryString) : _path;
                    _pathTranslated = defaultFilePath;
                    return false; // go through normal processing
                }
            }

            return false; // go through normal processing
        }

        public void ReadAllHeaders()
        {
            _headerBytes = null;

            do
            {
                if (!TryReadAllHeaders())
                {
                    // something bad happened
                    break;
                }
            } while (_endHeadersOffset < 0); // found \r\n\r\n

            // 
            // fixed: Item # 13290
            if (_headerByteStrings != null && _headerByteStrings.Count > 0)
            {
                _connection.LogRequestHeaders(string.Join(Environment.NewLine, _headerByteStrings.Select(b => b.GetString()).ToArray()));
            }

        }

        public void Reset()
        {
            _headerBytes = null;
            _startHeadersOffset = 0;
            _endHeadersOffset = 0;
            _headerByteStrings = null;

            _isClientScriptPath = false;

            _verb = null;
            _url = null;
            _protocol = null;

            _path = null;
            _filePath = null;
            _pathInfo = null;
            _pathTranslated = null;
            _queryString = null;
            _queryStringBytes = null;

            _contentLength = 0;
            _bodyLength = 0;
            _body = null;

            _allRawHeaders = null;
            _unknownRequestHeaders = null;
            _knownRequestHeaders = null;
            _specialCaseStaticFileHeaders = false;
        }

        public void SendResponseFromFileStream(Stream f, long offset, long length)
        {
            long fileSize = f.Length;

            if (length == -1)
            {
                length = fileSize - offset;
            }

            if (length == 0 || offset < 0 || length > fileSize - offset)
            {
                return;
            }

            if (offset > 0)
            {
                f.Seek(offset, SeekOrigin.Begin);
            }

            if (length <= MaxChunkLength)
            {
                var fileBytes = new byte[(int)length];
                int bytesRead = f.Read(fileBytes, 0, (int)length);
                SendResponseFromMemory(fileBytes, bytesRead);
            }
            else
            {
                var chunk = new byte[MaxChunkLength];
                var bytesRemaining = (int)length;

                while (bytesRemaining > 0)
                {
                    int bytesToRead = (bytesRemaining < MaxChunkLength) ? bytesRemaining : MaxChunkLength;
                    int bytesRead = f.Read(chunk, 0, bytesToRead);

                    SendResponseFromMemory(chunk, bytesRead);
                    bytesRemaining -= bytesRead;

                    // flush to release keep memory
                    if ((bytesRemaining > 0) && (bytesRead > 0))
                    {
                        FlushResponse(false);
                    }
                }
            }
        }

        public void SkipAllPostedContent()
        {
            if ((_contentLength > 0) && (_bodyLength < _contentLength))
            {
                byte[] buffer;
                for (int i = _contentLength - _bodyLength; i > 0; i -= buffer.Length)
                {
                    buffer = _connection.ReadRequestBytes(i);
                    if ((buffer == null) || (buffer.Length == 0))
                    {
                        return;
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true),
         SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
        public bool TryNtlmAuthenticate()
        {
            NtlmAuth auth = null;
            try
            {
                auth = new NtlmAuth();

                do
                {
                    string blobString = null;
                    string extraHeaders = _knownRequestHeaders[0x18];
                    if ((extraHeaders != null) && extraHeaders.StartsWith("NTLM ", StringComparison.Ordinal))
                    {
                        blobString = extraHeaders.Substring(5);
                    }
                    if (blobString != null)
                    {
                        if (!auth.Authenticate(blobString))
                        {
                            _connection.WriteErrorAndClose(0x193);
                            return false;
                        }
                        if (auth.Completed)
                        {
                            goto Label_009A;
                        }
                        extraHeaders = "WWW-Authenticate: NTLM " + auth.Blob + "\r\n";
                    }
                    else
                    {
                        extraHeaders = "WWW-Authenticate: NTLM\r\n";
                    }
                    SkipAllPostedContent();
                    _connection.WriteErrorWithExtraHeadersAndKeepAlive(0x191, extraHeaders);
                } while (TryParseRequest());
                return false;
            Label_009A:
                if (_host.GetProcessSid() != auth.SID)
                {
                    _auth = auth;
                    auth = null;

                    //_connection.WriteErrorAndClose(0x193);
                    //return false;
                }

            }
            catch
            {
                try
                {
                    _connection.WriteErrorAndClose(500);
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                }
                return false;
            }
            finally
            {
                if (auth != null)
                    auth.Dispose();
            }
            return true;
        }


        /// <summary>
        /// TODO: defer response until request is written
        /// </summary>
        /// <returns></returns>
        public bool TryParseRequest()
        {
            Reset();

            ReadAllHeaders();

            if (_headerBytes == null || _endHeadersOffset < 0 ||
                _headerByteStrings == null || _headerByteStrings.Count == 0)
            {
                _connection.WriteErrorAndClose(400);
                return false;
            }

            ParseRequestLine();

            // Check for bad path
            if (IsBadPath())
            {
                _connection.WriteErrorAndClose(400);
                return false;
            }

            // Check if the path is not well formed or is not for the current app
            if (!_host.IsVirtualPathInApp(_path, out _isClientScriptPath))
            {
                _connection.WriteErrorAndClose(404);
                return false;
            }

            ParseHeaders();

            ParsePostedContent();

            return true;
        }

        public bool TryReadAllHeaders()
        {
            // read the first packet (up to 32K)
            byte[] headerBytes = _connection.ReadRequestBytes(MaxHeaderBytes);

            if (headerBytes == null || headerBytes.Length == 0)
                return false;

            if (_headerBytes != null)
            {
                // previous partial read
                int len = headerBytes.Length + _headerBytes.Length;
                if (len > MaxHeaderBytes)
                    return false;

                var bytes = new byte[len];
                Buffer.BlockCopy(_headerBytes, 0, bytes, 0, _headerBytes.Length);
                Buffer.BlockCopy(headerBytes, 0, bytes, _headerBytes.Length, headerBytes.Length);
                _headerBytes = bytes;
            }
            else
            {
                _headerBytes = headerBytes;
            }

            // start parsing
            _startHeadersOffset = -1;
            _endHeadersOffset = -1;
            _headerByteStrings = new List<ByteString>();

            // find the end of headers
            var parser = new ByteParser(_headerBytes);

            for (; ; )
            {
                ByteString line = parser.ReadLine();

                if (line == null)
                {
                    break;
                }

                if (_startHeadersOffset < 0)
                {
                    _startHeadersOffset = parser.CurrentOffset;
                }

                if (line.IsEmpty)
                {
                    _endHeadersOffset = parser.CurrentOffset;
                    break;
                }

                _headerByteStrings.Add(line);
            }

            return true;
        }

        public static string UrlEncodeRedirect(string path)
        {
            // this method mimics the logic in HttpResponse.Redirect (which relies on public methods)

            // count non-ascii characters
            byte[] bytes = Encoding.UTF8.GetBytes(path);
            int count = bytes.Length;
            int countNonAscii = 0;
            for (int i = 0; i < count; i++)
            {
                if ((bytes[i] & 0x80) != 0)
                {
                    countNonAscii++;
                }
            }

            // encode all non-ascii characters using UTF-8 %XX
            if (countNonAscii > 0)
            {
                // expand not 'safe' characters into %XX, spaces to +s
                var expandedBytes = new byte[count + countNonAscii * 2];
                int pos = 0;
                for (int i = 0; i < count; i++)
                {
                    byte b = bytes[i];

                    if ((b & 0x80) == 0)
                    {
                        expandedBytes[pos++] = b;
                    }
                    else
                    {
                        expandedBytes[pos++] = (byte)'%';
                        expandedBytes[pos++] = (byte)IntToHex[(b >> 4) & 0xf];
                        expandedBytes[pos++] = (byte)IntToHex[b & 0xf];
                    }
                }

                path = Encoding.ASCII.GetString(expandedBytes);
            }

            // encode spaces into %20
            if (path.IndexOf(' ') >= 0)
            {
                path = path.Replace(" ", "%20");
            }

            return path;
        }

        #region Nested type: ByteParser

        public class ByteParser
        {
            public readonly byte[] _bytes;

            public int _pos;

            public ByteParser(byte[] bytes)
            {
                _bytes = bytes;
                _pos = 0;
            }

            public int CurrentOffset
            {
                get { return _pos; }
            }

            public ByteString ReadLine()
            {
                ByteString line = null;

                for (int i = _pos; i < _bytes.Length; i++)
                {
                    if (_bytes[i] == (byte)'\n')
                    {
                        int len = i - _pos;
                        if (len > 0 && _bytes[i - 1] == (byte)'\r')
                        {
                            len--;
                        }

                        line = new ByteString(_bytes, _pos, len);
                        _pos = i + 1;
                        return line;
                    }
                }

                if (_pos < _bytes.Length)
                {
                    line = new ByteString(_bytes, _pos, _bytes.Length - _pos);
                }

                _pos = _bytes.Length;
                return line;
            }
        }

        #endregion

        #region Nested type: ByteString

        public class ByteString
        {
            public readonly byte[] _bytes;

            public readonly int _length;

            public readonly int _offset;

            public ByteString(byte[] bytes, int offset, int length)
            {
                _bytes = bytes;
                _offset = offset;
                _length = length;
            }

            public byte[] Bytes
            {
                get { return _bytes; }
            }

            public bool IsEmpty
            {
                get { return (_bytes == null || _length == 0); }
            }

            public byte this[int index]
            {
                get { return _bytes[_offset + index]; }
            }

            public int Length
            {
                get { return _length; }
            }

            public int Offset
            {
                get { return _offset; }
            }

            public byte[] GetBytes()
            {
                var bytes = new byte[_length];
                if (_length > 0) Buffer.BlockCopy(_bytes, _offset, bytes, 0, _length);
                return bytes;
            }

            public string GetString(Encoding enc)
            {
                if (IsEmpty) return string.Empty;
                return enc.GetString(_bytes, _offset, _length);
            }

            public string GetString()
            {
                return GetString(Encoding.UTF8);
            }

            public int IndexOf(char ch)
            {
                return IndexOf(ch, 0);
            }

            public int IndexOf(char ch, int offset)
            {
                for (int i = offset; i < _length; i++)
                {
                    if (this[i] == (byte)ch) return i;
                }
                return -1;
            }

            public ByteString[] Split(char sep)
            {
                var list = new List<ByteString>();

                int pos = 0;
                while (pos < _length)
                {
                    int i = IndexOf(sep, pos);
                    if (i < 0)
                    {
                        break;
                    }

                    list.Add(Substring(pos, i - pos));
                    pos = i + 1;

                    while (this[pos] == (byte)sep && pos < _length)
                    {
                        pos++;
                    }
                }

                if (pos < _length)
                    list.Add(Substring(pos));

                return list.ToArray();
            }

            public ByteString Substring(int offset, int len)
            {
                return new ByteString(_bytes, _offset + offset, len);
            }

            public ByteString Substring(int offset)
            {
                return Substring(offset, _length - offset);
            }
        }

        #endregion
    }
}