using System;
using FluentSharp.CoreLib;
using FluentSharp.CoreLib.API;

namespace FluentSharp.CassiniDev
{
    public static class API_Cassini_ExtensionMethods_AppDomain
    {
        public static AppDomain appDomain(this API_Cassini apiCassini)
        {
            return apiCassini.host().notNull() ? apiCassini.host().AppDomain : null;
        }
        public static O2AppDomainFactory o2AppDomainFactory (this API_Cassini apiCassini)
        {
            var appDomain = apiCassini.appDomain();
            if (appDomain.notNull())
                return new O2AppDomainFactory(appDomain);
            return null;
        }

        public static API_Cassini appDomain_Load_FluentSharp_Assemblies(this API_Cassini apiCassini)
        {
            return apiCassini;
        }
    }
}