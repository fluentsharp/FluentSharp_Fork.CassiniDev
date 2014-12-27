using FluentSharp.CassiniDev;
using FluentSharp.CassiniDev.NUnit;
using FluentSharp.CoreLib;
using FluentSharp.REPL;
using FluentSharp.NUnit;
using FluentSharp.Watin;
using FluentSharp.WinForms;
using NUnit.Framework;

namespace UnitTests.FluentSharp.CassiniDev.NUnit
{
    [TestFixture]
    public class Test_CassiniDev_ExtensionMethods_REPL : NUnitTests
    {      
        public API_Cassini api_Cassini;
        
        [SetUp]
        public void setup()
        {
            api_Cassini  = new API_Cassini();     
            api_Cassini.start();
        }
        [TearDown]
        public void tearDown()
        {
            api_Cassini.stop();
        }

        [Test] public void script_Cassini()
        {
            var scriptEditor     = api_Cassini .script_Cassini(false)  .assert_Not_Null();
            var parentForm       = scriptEditor.parentForm()          .assert_Not_Null();
            var invocationParams = scriptEditor.invocationParameters().assert_Not_Null();
            
            invocationParams.assert_Size_Is(1)
                            .first()
                                .assert_Are_Equal((item)=>item.Key  , "cassini")
                                .assert_Are_Equal((item)=>item.Value, api_Cassini);

            scriptEditor.invocationParameter<API_Cassini>().assert_Is(api_Cassini);

            scriptEditor.executionResult().assert_Null();                           // check that there is no result
            
            scriptEditor.waitFor_ExecutionComplete();                               // wait for compile and execution to complete
            scriptEditor.executionResult().assert_Not_Null();                       // check that there is a result
            scriptEditor.executionResult<API_Cassini>().assert_Is(api_Cassini);     // and it is of type API_Cassini
            
            //check compilation
            var csharpCompiler = scriptEditor.csharpCompiler();

            csharpCompiler.compiledAssembly()    .assert_Not_Null();
            csharpCompiler.referencedAssemblies().assert_Size_Is(15)
                                                 .assert_Contains("FluentSharp.CassiniDev".assembly_Location());            
          //  parentForm.waitForClose();
            parentForm.close();
        }
        [Test] public void open_IE()
        {            
            var assembly = "Interop.SHDocVw".assembly();                            // for some reason we need to do this in order for the dll to be found by the UnitTest
            assembly.assert_Not_Null();
            var ie = "Cassini Dev Test".add_IE_PopupWindow();
            ie.assert_Not_Null();
            ie.parentForm().close();
            ie.parentForm_WaitForClose();
        }
        [Test]public void script_IE()
        {                                      
            var scriptEditor     = api_Cassini .script_IE()         .assert_Not_Null();
            var parentForm       = scriptEditor.parentForm()        .assert_Not_Null();
            var invocationParams = scriptEditor.InvocationParameters.assert_Not_Null();
                        
            invocationParams.assert_Size_Is(2);

            scriptEditor.onExecute = (result)=>
                {
                    scriptEditor.LastExecutionResult.assert_Not_Null();
                                                    //.assert_Are_Equal(()=> scriptEdiapi_Cassini.ie);                                        
                    scriptEditor.parentForm().closeForm();
                    parentForm.closeForm();
                };
            scriptEditor.onCompileExecuteOnce();
            
            //scriptEditor.waitForClose();

        }
    }
}
