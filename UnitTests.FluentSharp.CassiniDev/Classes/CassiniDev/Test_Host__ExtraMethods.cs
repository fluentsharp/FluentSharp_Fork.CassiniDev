using CassiniDev;
using FluentSharp.CassiniDev;
using FluentSharp.CassiniDev.NUnit;
using FluentSharp.CoreLib;
using FluentSharp.NUnit;
using FluentSharp.Web35;
using NUnit.Framework;

namespace UnitTests.FluentSharp_AspNet_MVC
{
    [TestFixture]
    public class Test_Host__ExtraMethods : NUnitTests_Cassini
    {
        [Test] public void Render_Request()
        {
            var fileInRoot = apiCassini.create_Random_File_Aspx().assert_File_Exists()
                                                                 .assert_File_Extension_Is(".aspx");
            var fileName   = fileInRoot.fileName();
            var url        = apiCassini.url_From_File(fileInRoot).assert_Equal(apiCassini.url(fileName));


            var server           = new Server(apiCassini.webRoot());
            var host             = server.GetHost();            
            
            url .GET()                   .assert_Valid().assert_Equals(fileInRoot.fileContents());  // check that we can get the file via a normal GET Request
            host.Render_Request(fileName).assert_Valid().assert_Equals(fileInRoot.fileContents());  // check that direct rendering of request produces the same value

            // check 404
            apiCassini.host().Render_Request("a.html").assert_Contains("The resource cannot be found.");
            apiCassini.host().Render_Request("a.img" ).assert_Contains("The resource cannot be found.");
            
            // check that HTML returns an empty value
            var htmlInRoot = apiCassini.create_Random_File_Html().assert_File_Exists()
                                                                 .assert_File_Extension_Is(".html")
                                                                 .assert_File_Extension_Is_Not(".aspx");

            host.Render_Request(      htmlInRoot.fileName()).assert_Is("");
            host.Render_Request("a" + htmlInRoot.fileName()).assert_Contains("The resource cannot be found.");;
        }

        //Workflows
        [Test] public void Render_Request__ASPX_CSharp()
        {
            var aspxPage = apiCassini.create_Random_File_Aspx();
            var message  = "Hello ASPX".add_5_RandomLetters();
            var aspxCode = @"<%@ Page Language=""C#""%><%= ""{0}"" %>".format(message);

            aspxPage.write_To_File(aspxCode);

            var host = apiCassini.host();

          //  host.Render_Request(aspxPage.fileName()).assert_Contains(message);

            //@"<%@ Page Language=""C#""%><%= HttpContext.Current.Request.Urlasd %>".format(message).saveAs(aspxPage);

            //host.Render_Request(aspxPage.fileName()).assert_Is("123");
        }
    }
    
}