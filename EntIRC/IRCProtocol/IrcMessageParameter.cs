using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entalyan.EntIRC.IRCProtocol
{
    internal struct IrcMessageParameter
    {
        internal string Name { get; set; }
        internal int Order { get; set; }
        internal bool AllowMultiple { get; set; }
        internal bool Optional { get; set; }

    }
}
