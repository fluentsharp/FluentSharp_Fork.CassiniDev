using System;
using FluentSharp.CassiniDev;
using FluentSharp.CassiniDev.NUnit;
using FluentSharp.CoreLib;
using FluentSharp.NUnit;
using FluentSharp.Web35;
using NUnit.Framework;

namespace UnitTests.FluentSharp.CassiniDev.NUnit
{
    [TestFixture] 
    public class Test_NUnitTests_Cassini
    {        
        [Test] public void NUnitTests_Cassini_Ctor()
        {
            // Checks that the Ctor doesn't start the server
            var nUnitTests_Cassini = new NUnitTests_Cassini();
            nUnitTests_Cassini.apiCassini.assert_Null();
            nUnitTests_Cassini.webRoot   .assert_Folder_Not_Exists();
            nUnitTests_Cassini.port      .assert_Default();
        }
        [Test] public void start()         
        {
            // stop() is also tests here

            var nUnitTests_Cassini = new NUnitTests_Cassini();
            nUnitTests_Cassini.apiCassini.assert_Null();
            nUnitTests_Cassini.webRoot   .assert_Folder_Not_Exists();
            nUnitTests_Cassini.port      .assert_Default();

            nUnitTests_Cassini.start();
            
            nUnitTests_Cassini.port      .tcpClient().assert_Not_Null();

            nUnitTests_Cassini.stop();
            nUnitTests_Cassini.port      .tcpClient().assert_Null();  
            nUnitTests_Cassini.webRoot.assert_Folder_Not_Exists();
        }


        //Workflows
        [Test]
        public void Get_Html_From_Txt_and_Aspx_Files()
        {
            var nUnitTests_Cassini = new NUnitTests_Cassini();
            nUnitTests_Cassini.start();
            var apiCassini = nUnitTests_Cassini.apiCassini;
            var webRoot    = nUnitTests_Cassini.webRoot;

            webRoot.assert_Folder_Exists();

            Action<string,string,string> checkFileViaHttp = 
                (fileName,fileContents, expectedResponse) =>
                {
                    var filePath = webRoot.pathCombine(fileName);
                    Assert.IsFalse(filePath.fileExists());
                    if (fileContents.valid())
                    {
                        fileContents.saveAs(filePath);
                        Assert.IsTrue(filePath.fileExists());
                    }
                    var fileUrl = apiCassini.url() + fileName;
                    var html    = fileUrl.html();
                    Assert.AreEqual(expectedResponse, html);
                    filePath.file_Delete();
                    Assert.IsFalse(filePath.fileExists());                
                };
            
            checkFileViaHttp("test_File1.txt" , ""                          , "");            
            checkFileViaHttp("test_File2.txt" , "Some contents ..."         , "Some contents ...");                        
            checkFileViaHttp("test_File3.txt" , "Some contents changed"     , "Some contents changed");                        
            checkFileViaHttp("test_ASPX1.aspx",  "<%=\"Hello from ASPX\"%>" , "Hello from ASPX");
            checkFileViaHttp("test_ASPX2.aspx",  "<%=\"Hello Again\"%>"     , "Hello Again");

            nUnitTests_Cassini.stop();

            webRoot.assert_Folder_Not_Exists();
        }
    }
}