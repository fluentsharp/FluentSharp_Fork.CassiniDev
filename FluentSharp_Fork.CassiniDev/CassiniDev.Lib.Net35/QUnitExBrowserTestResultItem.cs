﻿// /* **********************************************************************************
//  *
//  * Copyright (c) Sky Sanders. All rights reserved.
//  * 
//  * This source code is subject to terms and conditions of the Microsoft Public
//  * License (Ms-PL). A copy of the license can be found in the license.htm file
//  * included in this distribution.
//  *
//  * You must not remove this notice, or any other, from this software.
//  *
//  * **********************************************************************************/
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace CassiniDev
{


    ///<summary>
    ///</summary>
    ///<typeparam name="T"></typeparam>
       [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"),
     PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public abstract class CassiniDevBrowserTestFixture<T> where T : BrowserTestResultItem, new()
    {
        
        public string _url;
        public TimeSpan _timeOut = TimeSpan.FromMinutes(1);
        public Dictionary<string, BrowserTestResultItem> _results;
        public string _postKey = "log.axd";

        ///<summary>
        ///</summary>
        public abstract WebBrowser Browser { get; }
        ///<summary>
        ///</summary>
        public abstract string Path { get; }
        ///<summary>
        ///</summary>
        public abstract string Url { get; }

        ///<summary>
        ///</summary>
        public TimeSpan TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }


        ///<summary>
        ///</summary>
        public Dictionary<string, BrowserTestResultItem> Results
        {
            get { return _results; }
        }



        ///<summary>
        ///</summary>
        public string PostKey
        {
            get { return _postKey; }
            set { _postKey = value; }
        }


        
        ///<summary>
        ///</summary>
        public void RunTest()
        {
            //ContentLocator locator = new ContentLocator(@"RESTWebServices\RESTWebServices");

            var test = new CassiniDevBrowserTest(PostKey);
            test.StartServer(Path);
            _url = test.NormalizeUrl(Url);

            var testResults = new BrowserTestResults(test.RunTest(_url, Browser, TimeOut));
            var results = new T();
            results.Parse(testResults.Log);
            _results = results.Items;
            test.StopServer();
        }
    }

    /// <summary>
    ///   NOTE: there seems to be a 7k limit on data posted from the test so
    ///   be concious of the data you log
    /// </summary>
    [Serializable]
    public class QUnitExBrowserTestResultItem : BrowserTestResultItem
    {
        public static readonly Regex rx = new Regex(
            @"failures\s*=\s*(?<failures>\d+)\s*;\s*total\s*=\s*(?<total>\d+)",
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        ///<summary>
        ///</summary>
        ///<param name="log"></param>
        ///<exception cref="NotImplementedException"></exception>
        public override void Parse(string log)
        {
            // parse it line by line
            var lines = log.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
            Log.AddRange(lines);
            BrowserTestResultItem currentItem = this;
            BrowserTestResultItem lastItem = null;
            int index = 0;
            while (index < lines.Length - 1)
            {
                var line = lines[index];
                if (line.StartsWith("Module Started:") || line.StartsWith("  Test Started:"))
                {
                    lastItem = currentItem;
                    currentItem = new BrowserTestResultItem
                        {
                            Name = line.Substring("Module Started:".Length + 1)
                        };
                    if (lastItem == null)
                    {
                        throw new Exception("lst item is null?");
                    }
                    lastItem.Items.Add(currentItem.Name, currentItem);
                }
                else if (line.StartsWith("Module Done:") || line.StartsWith("  Test Done:"))
                {
                    SetCount(currentItem, line);
                    currentItem = lastItem;
                }
                else
                {
                    if (currentItem == null)
                    {
                        throw new Exception("log parse exception");
                    }
                    currentItem.Log.Add(line);
                }

                index++;
            }
            SetCount(this, lines[lines.Length - 1]);
        }

        public static void SetCount(BrowserTestResultItem item, string value)
        {
            int total, failures;
            ParseCount(value, out total, out failures);
            item.Total = total;
            item.Failures = failures;
            item.Success = failures == 0;
        }

        public static void ParseCount(string value, out int total, out int failures)
        {
            var match = rx.Match(value);
            total = Convert.ToInt32(match.Groups["total"].Value);
            failures = Convert.ToInt32(match.Groups["failures"].Value);
        }
    }
}
