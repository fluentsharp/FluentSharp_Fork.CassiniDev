using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentSharp.CassiniDev;
using FluentSharp.CoreLib;
using FluentSharp.NUnit;
using NUnit.Framework;

namespace UnitTests.FluentSharp.CassiniDev
{    
    [TestFixture]
    public class Test_API_Cassini_ExtensionMethods
    {
         public API_Cassini apiCassini;
        [SetUp] public void setup()
        {
            apiCassini = new API_Cassini();            

            apiCassini.webRoot().assert_Folder_Empty();
        }
        [TearDown] public void tearDown()
        {               
            apiCassini.webRoot().assert_Folder_Deleted();
        }
        [Test] public void url()
        {                        
            //check API_Cassini.url()
            var url          = apiCassini.url();
            var expected_Url = "http://{0}:{1}/".format(apiCassini.ipAddress(), apiCassini.port());
            url.assert_Equal_To(expected_Url);

            //check API_Cassini.url(virtualPath)
            var file = "test.txt"; 
            apiCassini.url(file).assert_Equal_To(expected_Url + file);           
        }
    }
}
