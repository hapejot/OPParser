﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Hapejot.OPParser
{
    public class ParseException : Exception
    {
        public ParseException(string msg) : base(msg)
        {
        }
    }
}
