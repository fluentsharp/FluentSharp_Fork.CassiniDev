using FluentSharp.CoreLib;

namespace FluentSharp.CassiniDev
{
    public static class API_Cassini_ExtensionMethods_Files
    {
        public static string webRoot(this API_Cassini apiCassini)
        {
            return (apiCassini.notNull()) ? apiCassini.PhysicalPath : null;
        }

        public static string mapPath(this API_Cassini apiCassini, string virtualPath)
        {
            return apiCassini.webRoot()
                .mapPath(virtualPath);            
        }
        /// <summary>
        /// Creates a file inside the current web root
        /// 
        /// returns the path to the file created
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <param name="fileVirtualPath"></param>
        /// <param name="fileContents"></param>
        /// <returns></returns>
        public static string create_File(this API_Cassini apiCassini, string fileVirtualPath, string fileContents)
        {            
            if (fileContents.valid())
            { 
                var filePath = apiCassini.mapPath(fileVirtualPath);
                if (filePath.valid() && filePath.file_Not_Exists())
                    return fileContents.saveAs(filePath);
            }
            return null;   
        }
        /// <summary>
        /// Returns the url to the file provided
        /// 
        /// For example c:\path\to\server\a\file.aspx should return http://server:port:/a/file.aspx
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public static string url_From_File(this API_Cassini apiCassini, string fileToMap)
        {
            var webRoot = apiCassini.webRoot();
            if (fileToMap.contains(webRoot))            
                return apiCassini.url(fileToMap.remove(webRoot));
            return null;
        }
        /// <summary>
        /// Returns the file path to the url provided (urlToMap)
        /// 
        /// For example http://server:port:/a/file.aspx should return c:\path\to\server\a\file.aspx
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public static string file_From_Url(this API_Cassini apiCassini, string urlToMap)
        {
            var url = apiCassini.url();
            if (urlToMap.contains(url))            
                return apiCassini.mapPath(urlToMap.remove(url));
            return null;
        }
    }
}