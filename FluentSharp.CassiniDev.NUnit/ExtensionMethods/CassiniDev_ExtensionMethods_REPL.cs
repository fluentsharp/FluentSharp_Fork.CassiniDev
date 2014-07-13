using FluentSharp.CoreLib;
using FluentSharp.NUnit;
using FluentSharp.REPL;
using FluentSharp.REPL.Controls;
using FluentSharp.Watin;
using FluentSharp.WatiN.NUnit;
using FluentSharp.Web35;
using FluentSharp.WinForms;

namespace FluentSharp.CassiniDev.NUnit
{
    public static class CassiniDev_ExtensionMethods_REPL
    {
        public static ascx_Simple_Script_Editor script_Cassini(this API_Cassini api_Cassini, bool startHidden = false)
        {
            api_Cassini.url().assert_Not_Null()
                             .uri().GET();
            
            var extraCode = @"
//using FluentSharp.CassiniDev
//O2Ref:FluentSharp.CassiniDev.dll
";
            return api_Cassini.script_Me("cassini", startHidden)
                              .code_Append(extraCode);
        }

        public static ascx_Simple_Script_Editor script_IE(this API_Cassini api_Cassini)
        {
            api_Cassini.url().assert_Not_Null()
                             .uri().GET();
            
            var extraCode = @"
//using FluentSharp.CassiniDev
//O2Ref:FluentSharp.CassiniDev.dll
";
     

            var scriptEditor = "Cassini Dev Test".add_IE_PopupWindow()
                                                .open(api_Cassini.url())
                                                .script_IE();
            scriptEditor.add_InvocationParameter("cassini", api_Cassini)
                        .code_Append(extraCode);                              
            return scriptEditor;    
        }
        public static API_Cassini script_Cassini_WaitForClose(this API_Cassini api_Cassini)
        {
            api_Cassini.script_Cassini().waitForClose();
            return api_Cassini;
        }        
    }
}
