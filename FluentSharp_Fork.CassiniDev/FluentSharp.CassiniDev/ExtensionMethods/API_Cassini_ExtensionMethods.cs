using CassiniDev;
using FluentSharp.CoreLib;

namespace FluentSharp.CassiniDev
{
    public static class API_Cassini_ExtensionMethods
    {
        public static Server server(this API_Cassini apiCassini)
        {
            return (apiCassini.notNull())
                ? apiCassini.CassiniServer
                : null;
        }

        public static int    port(this API_Cassini apiCassini)
        {
            return apiCassini.notNull() 
                ? apiCassini.CassiniServer.Port 
                : -1;
        }
         public static string ipAddress(this API_Cassini apiCassini)
        {
            if (apiCassini.server().notNull())
                return apiCassini.server().IPAddress.str();
            return null;
        }

        public static string url(this API_Cassini apiCassini)
        {
            if (apiCassini.notNull())
            {
                var server = apiCassini.server();
                return "http://{0}:{1}/".format(server.IPAddress.str(), server.Port);
            }
            return null;
        }
        /// <summary>
        /// Resources a virtual path into a full url
        /// 
        /// For example if <code>virtualPath="test.txt"</code>, the return value will be <code>http://server:port/test.txt</code>
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static string url(this API_Cassini apiCassini, string virtualPath)
        {
            return apiCassini.url()
                             .uri()
                             .append(virtualPath).str();
        }

        public static API_Cassini start(this API_Cassini apiCassini)
        {
            if (apiCassini.notNull())
                apiCassini.CassiniServer.Start();
            return apiCassini;
        }
        public static API_Cassini stop(this API_Cassini apiCassini)
        {
            if (apiCassini.notNull())
                apiCassini.CassiniServer.ShutDown();
            return apiCassini;
        }
    }
}