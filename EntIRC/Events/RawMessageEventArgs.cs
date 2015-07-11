using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entalyan.EntIRC.Events
{
    /// <summary>
    /// EventArgs-class used when a new raw message is received is received.
    /// </summary>
    public class RawMessageReceivedEventArgs : EventArgs
    {
        private byte[] rawMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawMessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="rawMessage">The raw message that triggered the event.</param>
        public RawMessageReceivedEventArgs(byte[] message)
        {
            this.rawMessage = message;
        }

        /// <summary>
        /// Gets the rawMessage.
        /// </summary>
        public byte[] Message
        {
            get { return rawMessage; }
            set { rawMessage = value; }
        }
    }

}
