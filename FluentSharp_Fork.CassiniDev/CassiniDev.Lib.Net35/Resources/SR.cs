//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.htm file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

#endregion

namespace CassiniDev
{
    public sealed class SR
    {
        
        public const string ErrInvalidIPMode="SR.ErrInvalidIPMode";
        public const string ErrInvalidIPAddress = "ErrInvalidIPAddress";
        public const string ErrInvalidPortMode = "ErrInvalidPortMode";
        public const string ErrPortIsInUse = "ErrPortIsInUse";
        public const string ErrNoAvailablePortFound = "ErrNoAvailablePortFound";
        public const string ErrPortRangeEndMustBeEqualOrGreaterThanPortRangeSta =
            "ErrPortRangeEndMustBeEqualOrGreaterThanPortRangeSta";
        public const string ErrInvalidPortRangeValue = "ErrInvalidPortRangeValue";
        public const string ErrInvalidHostname = "ErrInvalidHostname";

        public const string ErrFailedToStartCassiniDevServerOnPortError =
            "ErrFailedToStartCassiniDevServerOnPortError";
        public const string ErrApplicationPathIsNull = "ErrApplicationPathIsNull";
        public const string ErrPortOutOfRange = "ErrPortOutOfRange";

        public const string WebdevAspNetVersion = "WebdevAspNetVersion";

        public const string WebdevDirListing = "WebdevDirListing";

        public const string WebdevDirNotExist = "WebdevDirNotExist";

        public const string WebdevErrorListeningPort = "WebdevErrorListeningPort";

        public const string WebdevHttpError = "WebdevHttpError";

        public const string WebdevInMemoryLogging = "WebdevInMemoryLogging";

        public const string WebdevInvalidPort = "WebdevInvalidPort";

        public const string WebdevLogViewerNameWithPort = "WebdevLogViewerNameWithPort";

        public const string WebdevName = "WebdevName";

        public const string WebdevNameWithPort = "WebdevNameWithPort";

        public const string WebdevOpenInBrowser = "WebdevOpenInBrowser";

        public const string WebdevRunAspNetLocally = "WebdevRunAspNetLocally";

        public const string WebdevServerError = "WebdevServerError";

        public const string WebdevShowDetail = "WebdevShowDetail";

        public const string WebdevStop = "WebdevStop";

        public const string WebdevUsagestr1 = "WebdevUsagestr1";

        public const string WebdevUsagestr2 = "WebdevUsagestr2";

        public const string WebdevUsagestr3 = "WebdevUsagestr3";

        public const string WebdevUsagestr4 = "WebdevUsagestr4";

        public const string WebdevUsagestr5 = "WebdevUsagestr5";

        public const string WebdevUsagestr6 = "WebdevUsagestr6";

        public const string WebdevUsagestr7 = "WebdevUsagestr7";

        public const string WebdevVersionInfo = "WebdevVersionInfo";

        public const string WebdevVwdName = "WebdevVwdName";

        public static SR _loader;

        public readonly ResourceManager _resources;
        public const string WebdevStart = "WebdevStart";

        public SR()
        {
            Type t = GetType();
            Assembly thisAssembly = t.Assembly;
            string stringResourcesName = t.Namespace + ".Resources.CassiniDev";
            _resources = new ResourceManager(stringResourcesName, thisAssembly);
        }

        public static CultureInfo Culture
        {
            get { return null; }
        }

        public static ResourceManager Resources
        {
            get { return GetLoader()._resources; }
        }

        public static string GetString(string name)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader._resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader._resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static SR GetLoader()
        {
            if (_loader == null)
            {
                SR sr = new SR();
                Interlocked.CompareExchange(ref _loader, sr, null);
            }
            return _loader;
        }

    }
}