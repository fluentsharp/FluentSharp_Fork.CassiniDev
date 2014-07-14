using CassiniDev;
using FluentSharp.CoreLib;

namespace FluentSharp.CassiniDev
{
    public static class API_Cassini_ExtensionMethods
    {     
        /// <summary>
        /// Returns the IP Address of the current cassini server
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static string        ipAddress(this API_Cassini apiCassini)                      
        {
            if (apiCassini.server().notNull())
                return apiCassini.server().IPAddress.str();
            return null;
        }
 
        /// <summary>
        /// Returns the CassiniServer Host object (by calling <code>CassiniServer.GetHost()</code>  )
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static Host          host     (this API_Cassini apiCassini)                      
        {
            return apiCassini.server().notNull() 
                        ? apiCassini.server().GetHost() 
                        : null;
        }

        /// <summary>
        /// Returns the port of the current cassini server
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static int           port     (this API_Cassini apiCassini)                      
        {
            return apiCassini.notNull() 
                        ? apiCassini.CassiniServer.Port 
                        : -1;
        }

        /// <summary>
        /// Returns a  reference to the CassiniServer object
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static Server        server   (this API_Cassini apiCassini)                      
        {
            return (apiCassini.notNull())
                ? apiCassini.CassiniServer
                : null;
        }

        /// <summary>
        /// Starts the CassiniServer (cy calling its Start Method)
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static API_Cassini   start    (this API_Cassini apiCassini)                      
        {
            if (apiCassini.notNull())
                apiCassini.CassiniServer.Start();
            return apiCassini;
        }

        /// <summary>
        /// Stops the CassiniServer (by calling its ShutDown method)
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static API_Cassini   stop     (this API_Cassini apiCassini)                      
        {
            if (apiCassini.notNull())
                apiCassini.CassiniServer.ShutDown();
            return apiCassini;
        }

        /// <summary>
        /// Returns the root URL of the current server
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static string        url      (this API_Cassini apiCassini)                      
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
        public static string        url      (this API_Cassini apiCassini, string virtualPath)  
        {
            return apiCassini.url()
                             .uri()
                             .append(virtualPath).str();
        }
    }
}