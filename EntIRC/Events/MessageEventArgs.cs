using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entalyan.EntIRC.IRCProtocol.Messages;

namespace Entalyan.EntIRC.Events
{
    /// <summary>
    /// EventArgs-class used when a new message is received.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Retrieves all data contained in the message.
        /// </summary>
        public IrcMessage Message
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        public MessageEventArgs(IrcMessage message)
        {
                this.Message = message;
        }

    }
}
