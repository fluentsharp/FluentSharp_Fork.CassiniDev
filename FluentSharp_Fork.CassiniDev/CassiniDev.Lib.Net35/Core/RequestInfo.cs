﻿using System;
using System.Collections.Generic;

namespace CassiniDev
{

    [Serializable]
    public class RequestInfoArgs : EventArgs
    {
        public RequestInfoArgs ()
        {
            Continue = true;
            
        }


        
        
        public string Protocol { get; set; }
        public string Verb { get;  set; }
        public string Url { get;  set; }
        public byte[] Body { get;  set; }
        public string QueryString { get; set; }
        public string ProcessUser { get; set; }


        public int ResponseStatus { get; set; }
        public bool Continue { get; set; }
        public string Response { get;  set; }
        public string ExtraHeaders { get; set; }
    }
}