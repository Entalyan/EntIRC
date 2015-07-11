using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entalyan.EntIRC.IRCProtocol.Messages
{
    class NoticeMessage : IrcMessage
    {
        public NoticeMessage(string fullMessage)
            : base(fullMessage)
        {
        }
    }
}
