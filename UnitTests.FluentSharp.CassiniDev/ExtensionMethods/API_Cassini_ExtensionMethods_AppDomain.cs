using FluentSharp.CassiniDev;
using FluentSharp.CassiniDev.NUnit;
using FluentSharp.CoreLib;
using FluentSharp.NUnit;
using FluentSharp.REPL;
using FluentSharp.Web35;
using NUnit.Framework;

namespace UnitTests_FluentSharp_Fork.CassiniDev.ExtensionMethods
{
    [TestFixture]
    public class Test_API_Cassini_ExtensionMethods_AppDomain : NUnitTests_Cassini
    {
        [Test] public void appDomain()
        {
            var appDomain = apiCassini.appDomain();

            appDomain.assert_Not_Null()
                     .assert_Is_Equal_To(apiCassini.server().HostAppDomain);

            appDomain.assemblies().assert_Bigger_Than(30)
                                  .assert_Contains("System.Web", "System.Web.Extensions")
                                  .assert_Contains("FluentSharp.CassiniDev");            
        } 
        [Test] public void o2AppDomainFactory()
        {
            apiCassini.o2AppDomainFactory().assert_Not_Null()
                                           .assert_Are_Equal(o2AppDomainFactory=> o2AppDomainFactory.AppDomain, apiCassini.appDomain());                                           
        }

        [Test] public void appDomain_Load_FluentSharp_Assemblies()
        {
            var appDomain = apiCassini.appDomain()                        .assert_Not_Null();
            
            if (appDomain.assemblies().empty())
                "Skypping since appDomain.assemblies() is returning an Empty list".assert_Ignore();

            appDomain.isAssemblyLoaded("System.Web"                      ).assert_True ();
            appDomain.isAssemblyLoaded("FluentSharp.CoreLib"             ).assert_False();

            appDomain.load            ("FluentSharp.CoreLib".assembly_Location(),true).assert_True ();
            appDomain.isAssemblyLoaded("FluentSharp.CoreLib"                         ).assert_True ();            
                        
            appDomain.load("FluentSharp.REPL"              .assembly_Location() ,true).assert_True();
            appDomain.load("FluentSharp.WinForms"          .assembly_Location() ,true).assert_True();
            appDomain.load("FluentSharp.SharpDevelopEditor".assembly_Location() ,true).assert_True();            
            
            appDomain.isAssemblyLoaded("FluentSharp.REPL"                ).assert_True ();    
            appDomain.isAssemblyLoaded("FluentSharp.WinForms"            ).assert_True ();    
            appDomain.isAssemblyLoaded("FluentSharp.SharpDevelopEditor"  ).assert_True ();    
            
            var aspxFile = apiCassini.create_Random_File_Aspx();
            apiCassini.url_From_File(aspxFile).GET().assert_Is(aspxFile.fileContents());            

            
        }

        [Test] public void appDomain_Check_That_GetAssembiles_Method_Doesnt_Work_After_Web_Request()
        {
            var appDomain = apiCassini.appDomain().assert_Not_Null();

            appDomain.assemblies().assert_Bigger_Than(20);

            var aspxFile = apiCassini.create_Random_File_Aspx();
            apiCassini.url_From_File(aspxFile).GET().assert_Is(aspxFile.fileContents());
            
            // after the first request the GetAssemblies doesn't work since the current domain does
            // not have access to the temp asp.net assemblies created

            appDomain.assemblies().assert_Empty();              
        }
        
        [Test] public void appDomain_Get_O2_Proxy_After_Web_Request()
        {
            var appDomain = apiCassini.appDomain().assert_Not_Null();
            
            apiCassini.create_Random_File_Aspx()                                // create a temp file and get it via a GET request (this used to cause probs due to the fact that GetAssemblies doesn't work after the first GET request)
                      .url_From_File(apiCassini).assert_Is_Uri()
                                                .GET().assert_Contains("This is a random file created by");

            appDomain.copy_To_Bin_Folder("FluentSharp.CoreLib".assembly())      // copy the FluentSharp.CoreLib to the bin folder
                     .assert_File_Exists();            
            
            var o2Proxy   = appDomain.o2Proxy();                                // create the O2Proxy object (and return the currenly loaded assemblies)
            o2Proxy.assert_Not_Null()
                   .assemblies().assert_Not_Empty()
                                .assert_Contains("FluentSharp.CoreLib");
            //o2Proxy.script_Me_WaitForClose();


            //Invoke a couple static methods to confirm that we are in the web app domain
            var binDirectory             = o2Proxy.staticInvocation("System.Web","System.Web.HttpRuntime","get_BinDirectory"            ,new object[] {});            
            var appDomainAppId           = o2Proxy.staticInvocation("System.Web","System.Web.HttpRuntime","get_AppDomainAppId"          ,new object[] {});            
            var appDomainAppPath         = o2Proxy.staticInvocation("System.Web","System.Web.HttpRuntime","get_AppDomainAppPath"        ,new object[] {});            
            var appDomainAppVirtualPath  = o2Proxy.staticInvocation("System.Web","System.Web.HttpRuntime","get_AppDomainAppVirtualPath" ,new object[] {});
            
            binDirectory           .assert_Equal_To(appDomain.binFolder().append("\\"));
            appDomainAppId         .assert_Equal_To(appDomain.GetData(".appId"));
            appDomainAppPath       .assert_Equal_To(appDomain.rootFolder());
            appDomainAppVirtualPath.assert_Equal_To("/");
            
            
            //appDomainPath.assert_Not_Default();
                
                
            //appDomain.script_Me_WaitForClose();
        }
    }
}
