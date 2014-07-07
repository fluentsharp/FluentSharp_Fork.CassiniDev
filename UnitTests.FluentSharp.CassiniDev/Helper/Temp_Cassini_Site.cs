using System;
using CassiniDev;
using FluentSharp.CassiniDev;
using FluentSharp.CoreLib;
using FluentSharp.CoreLib.API;
using FluentSharp.Web35;
using NUnit.Framework;

namespace UnitTests.FluentSharp_AspNet_MVC
{
    public class Temp_Cassini_Site
    {
        public API_Cassini apiCassini;
        public Server      server;
        public String      webRoot;

        [SetUp]
        public void SetUp()
        {
            webRoot = "_temp_CassiniSite".tempDir();
            apiCassini = new API_Cassini(webRoot).start();
            server  = apiCassini.CassiniServer;            

            Assert.IsNotNull(apiCassini);
            Assert.IsNotNull(server);
            Assert.Less     (32767, apiCassini.port());
            Assert.IsNotNull(apiCassini.port().tcpClient());
            Assert.IsTrue   (webRoot.dirExists());
            Assert.AreEqual (webRoot, apiCassini.PhysicalPath);

            //var httpContent = HttpContext.Current;
            //
        }
        
        [TearDown]
        public void TearDown()
        {
            apiCassini.stop();            
            Assert.IsNull (apiCassini.port().tcpClient());
            Files.deleteFolder(webRoot, true);            
            //if(webRoot.dirExists())
            //    webRoot.startProcess();
            5.loop((i)=>
                {
                    if (webRoot.dirExists().isFalse())
                        return false;     
                    200.sleep();
                    return true;
                });            
            Assert.IsFalse(webRoot.dirExists());
        }
    }
}
