using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entalyan.EntIRC.IRCProtocol.Messages
{
    class NotImplementedMessage : IrcMessage
    {
        /// <summary>
        /// This Message is used when a message is present in the specification,
        /// but does not have an implementation in the IrcMessageFactory. 
        /// </summary>
        /// <param name="fullMessage"></param>
        public NotImplementedMessage(string fullMessage)
            : base(fullMessage)
        {
            this.RawString = "NOT IMPLEMENTED MESSAGE: " + fullMessage;
        }
    }
}
