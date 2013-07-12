﻿using System;
using System.IO;

namespace CassiniDev
{
    /// <summary>
    ///   Walks up from the current execution directory looking for directoryName.
    ///   This means that we can spin up a server on an arbitrary directory that is a child
    ///   of any of the current directory's ancestors
    /// </summary>
    public class ContentLocator : IContentLocator
    {
        
        public readonly string _directoryName;

        ///<summary>
        ///</summary>
        ///<param name="directoryName"></param>
        public ContentLocator(string directoryName)
        {
            _directoryName = directoryName;
        }

        #region IContentLocator Members

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string LocateContent()
        {
            var path = Environment.CurrentDirectory;

            while (!Directory.Exists(Path.Combine(path + "", _directoryName)))
            {
                path = Path.GetDirectoryName(path);
            }

            if (Directory.Exists(Path.Combine(path + "", _directoryName)))
            {
                path = Path.Combine(path + "", _directoryName);
            }
            else
            {
                throw new Exception("could not find content");
            }

            return path;
        }

        #endregion
    }
}
