using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace FluentSharp.CoreLib
{
    public static class Extra_ExtensionMethod_TcpClient
    {
        public static byte[] read_Response_Bytes(this TcpClient tcpClient)
        {
            if(tcpClient.isNotNull())
            {
                try
                {                  
                    var memoryStream = new MemoryStream();
                    var bytes = new byte[1024];
                    var bytesRead = tcpClient.stream().Read(bytes, 0, bytes.size());
                    while(bytesRead > 0)
                    {
                        memoryStream.Write(bytes,0,bytesRead);    
                        bytesRead = tcpClient.stream().Read(bytes, 0, bytes.size());
                    }                    
                    return memoryStream.ToArray();
                }
                catch(Exception ex)
                {
                    ex.log("[TcpClient][readResponseBytes]");   
                }
            }
            return new byte[0];
        }

        public static string read_Response(this TcpClient tcpClient)
        {
            return tcpClient.read_Response_Bytes().ascii();
        }
        
    }
}
