using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entalyan.EntIRC.IRCProtocol.Messages
{
    class PrivateMessage : IrcMessage
    {
        public PrivateMessage(string fullMessage)
            : base(fullMessage)
        {
        }
    }
}
