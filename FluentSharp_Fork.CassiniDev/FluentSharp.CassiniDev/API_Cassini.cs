using CassiniDev;
using FluentSharp.CoreLib;

namespace FluentSharp.CassiniDev
{
    public class API_Cassini
    {
        public Server CassiniServer     { get; set; }
        public string PhysicalPath      { get; set; }        
        
        public API_Cassini() : this("CassiniSite".tempDir())
        {            
        }
        public API_Cassini(string physicalPath)
        {
            PhysicalPath = physicalPath;
            CassiniServer = new Server(physicalPath);            
        }
        public API_Cassini(string physicalPath, string virtualPath, int port)
        {
            PhysicalPath = physicalPath;
            CassiniServer = new Server(port, virtualPath, physicalPath);
        }        
    }
}
