using FluentSharp.CassiniDev;
using FluentSharp.CoreLib;
using FluentSharp.REPL;
using FluentSharp.NUnit;
using FluentSharp.Watin;
using FluentSharp.WatiN.NUnit;
using FluentSharp.Web35;
using FluentSharp.WinForms;
using NUnit.Framework;

namespace UnitTests.FluentSharp.CassiniDev.NUnit
{
    [TestFixture]
    public class Test_CassiniDev_ExtensionMethods_REPL : NUnitTests
    {
        //workflows
        [Test][Ignore] public void Open_Cassini_On_Root_With_REPL()
        {
            var api_Cassini = new API_Cassini();
            //api_Cassini.PhysicalPath.startProcess();
            api_Cassini.start();
            

        }
    }
}
