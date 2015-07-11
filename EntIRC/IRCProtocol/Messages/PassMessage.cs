using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entalyan.EntIRC.IRCProtocol.Messages
{
    class PassMessage : IrcMessage
    {

        public PassMessage(string fullMessage)
            : base(fullMessage)
        {
        }
    }
}
