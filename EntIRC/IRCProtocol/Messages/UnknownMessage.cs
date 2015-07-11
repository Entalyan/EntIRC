using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entalyan.EntIRC.IRCProtocol.Messages
{
    class UnknownMessage : IrcMessage
    {
        /// <summary>
        /// This Message is used when no appropriate Message could be found. 
        /// </summary>
        /// <param name="fullMessage"></param>
        public UnknownMessage(string fullMessage)
            : base(fullMessage)
        {
            this.RawString = "UNIDENTIFIED MESSAGE: " + fullMessage;
        }
    }
}
