﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeSearchMBudzisz.Exceptions
{
    internal class AppConfigException : Exception
    {
        public AppConfigException(string message): base(message) { }
    }
}
