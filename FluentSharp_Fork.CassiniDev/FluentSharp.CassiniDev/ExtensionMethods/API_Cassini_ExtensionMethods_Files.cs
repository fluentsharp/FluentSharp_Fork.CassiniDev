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
        /// Creates a random ASPX file on the web root (with no dynamic content, just pure text)
        /// 
        /// returns <code>apiCassini.create_Random_File("aspx");</code>
        /// </summary>
        /// <param name="apiCassini"></param>        
        /// <returns></returns>
        public static string create_Random_File_Aspx(this API_Cassini apiCassini)
        {
            return apiCassini.create_Random_File("aspx");
        }
        /// <summary>
        /// Creates a random HTML file on the web root
        /// 
        /// returns <code>apiCassini.create_Random_File("html");</code>
        /// </summary>
        /// <param name="apiCassini"></param>        
        /// <returns></returns>
        public static string create_Random_File_Html(this API_Cassini apiCassini)
        {
            return apiCassini.create_Random_File("html");
        }
        /// <summary>
        /// Creates a random Text file on the web root
        /// 
        /// returns <code>apiCassini.create_Random_File("txt");</code>
        /// </summary>
        /// <param name="apiCassini"></param>        
        /// <returns></returns>
        public static string create_Random_File_Txt(this API_Cassini apiCassini)
        {
            return apiCassini.create_Random_File("txt");
        }
        /// <summary>
        /// Creates a random file (one with a unique name and contents) in the web root of apiCassini
        /// 
        /// The file name is created by <code>var fileName     = "randomFile_{0}.{1}".format(6.randomLetters(), extension);</code>
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string create_Random_File(this API_Cassini apiCassini, string extension)
        {
            var fileName     = "randomFile_{0}.{1}".format(6.randomLetters(), extension);
            var fileContents = "This is a random file created by API_Cassini.create_Random_File extension method".line().add_5_RandomLetters();
            return apiCassini.create_File(fileName, fileContents);
        }
        /// <summary>
        /// Returns the url to the file provided
        /// </summary>
        /// <param name="fileToMap"></param>
        /// <param name="apiCassini"></param>
        /// <returns></returns>
        public static string url_From_File(this string fileToMap,  API_Cassini apiCassini)
        {
            return apiCassini.url_From_File(fileToMap);
        }
        /// <summary>
        /// Returns the url to the file provided
        /// 
        /// For example c:\path\to\server\a\file.aspx should return http://server:port:/a/file.aspx
        /// </summary>
        /// <param name="apiCassini"></param>
        /// <param name="fileToMap"></param>
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
        /// <param name="urlToMap"></param>
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