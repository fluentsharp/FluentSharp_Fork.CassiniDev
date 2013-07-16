using FluentSharp.WinForms;
using NUnit.Framework;
using FluentSharp.CoreLib;
using FluentSharp.CassiniDev;

namespace UnitTests.FluentSharp_AspNet_MVC
{
    [TestFixture][Ignore("Temporarily")]
    public class Test_API_Cassini
    {
        [Test(Description = "Starts the Cassini webserver")]
        public void start()
        {
            var cassini = new API_Cassini();
            var port = cassini.port();
            var url = cassini.url();
            var port_BeforeStart = port.tcpClient();            

            cassini.start();
            var port_AfterStart = port.tcpClient();
            var html = url.get_Html();            

            cassini.stop();
            var port_AfterStop  = port.tcpClient();

            Assert.Less     (0,port);
            Assert.IsNull   (port_BeforeStart);
            Assert.IsNotNull(port_AfterStart);
            Assert.IsNull   (port_AfterStop);
            Assert.IsNotNull(url);
            Assert.IsTrue   (html.valid());
            Assert.IsTrue   (html.contains("Directory Listing"));

            //test null data
            Assert.IsNull((null as API_Cassini).start());        
    
            //delete site folder
            cassini.PhysicalPath.delete_Folder();
        }

        [Test(Description = "Stops the Cassini webserver")]
        public void stop()
        {
            //start();
            Assert.IsNull((null as API_Cassini).stop());   
        }

        [Test]
        public void API_Cassini_Ctors()
        {
            //() ctor
            var cassini_Default = new API_Cassini();
            Assert.IsNotNull(cassini_Default);
            Assert.IsTrue   (cassini_Default.CassiniServer.PhysicalPath.dirExists());
            Assert.Less     (20000, cassini_Default.CassiniServer.Port);
            Assert.AreEqual ("/", cassini_Default.CassiniServer.VirtualPath);

            //(string physicalPath) ctor
            var physicalPath = @"C:\a_path\".add_RandomLetters(10) + "\\";
            var cassini_PhysicalPath = new API_Cassini(physicalPath);
            Assert.IsNotNull(cassini_PhysicalPath);
            Assert.IsFalse  (cassini_PhysicalPath.CassiniServer.PhysicalPath.dirExists());
            Assert.AreEqual (cassini_PhysicalPath.CassiniServer.PhysicalPath, physicalPath);
            Assert.Less     (20000, cassini_PhysicalPath.CassiniServer.Port);
            Assert.AreEqual ("/", cassini_PhysicalPath.CassiniServer.VirtualPath);

            //(string physicalPath, string virtualPath, int port) ctor
            var port = 12345;
            var virtualPath = "/".add_RandomLetters(10);
            var cassini_AllValues = new API_Cassini(physicalPath, virtualPath, port);
            Assert.IsNotNull(cassini_AllValues);
            Assert.IsFalse  (cassini_AllValues.CassiniServer.PhysicalPath.dirExists());
            Assert.AreEqual (cassini_AllValues.CassiniServer.PhysicalPath, physicalPath);
            Assert.AreEqual (cassini_AllValues.CassiniServer.Port, port);
            Assert.AreEqual (cassini_AllValues.CassiniServer.VirtualPath, virtualPath);

            //delete site folder
            cassini_Default.PhysicalPath.delete_Folder();
            cassini_PhysicalPath.PhysicalPath.delete_Folder();
        }


        [Test]
        public void Browser_Website()
        {
            var cassini = new API_Cassini();
            cassini.start();
            var browser = "FluentSharp - Test side".popupWindow(700,600).add_WebBrowser();
            
            browser.open(cassini.url());

            browser.closeForm_InNSeconds(0);
            browser.waitForClose();

            //delete site folder
            cassini.PhysicalPath.delete_Folder();

        }



    }
}
