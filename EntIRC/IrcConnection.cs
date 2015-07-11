using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Entalyan.EntIRC.IRCProtocol;
using Entalyan.EntIRC.Events;

namespace Entalyan.EntIRC
{
    public class IrcConnection
    {
        #region Constants

        internal const int BUFFER_SIZE = 512;
        internal const byte CR = 10;
        internal const byte LF = 13;
        internal const int TERMINATOR_LENGTH = 2;

        #endregion

        #region Private Fields

        private TcpClient networkClient = null;
        //private MemoryStream readBuffer;

        //private IPEndPoint endPoint;
        //private IrcUser user;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether there was a connection to a
        /// remote host during the last I/O operation.
        /// </summary>
        public bool IsConnected
        {
            get { return networkClient.Connected; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of an IrcConnection.
        /// </summary>
        public IrcConnection()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Connects to the specified IRC server.
        /// </summary>
        public async Task ConnectAsync(IPAddress host, int port)
        {
            //Initialization
            if (networkClient == null)
                networkClient = new TcpClient();

            
                //ConnectAsync to the server
                await networkClient.ConnectAsync(host, port).ContinueWith(t =>
                    {
                        //Handle unsuccesful connection
                        if (t.IsFaulted)
                        {
                            //Propagate the original exception
                            throw t.Exception;
                        }
                        else
                        {
                            networkStream = networkClient.GetStream();
                        }

                    }
            );
            }

        

        /// <summary>
        /// Reads incoming data, and puts it in the buffer for processing.
        /// </summary>
        public async Task ReadAsync()
        {
            var readBuffer = new MemoryStream(BUFFER_SIZE);

            //Set up buffers to store incoming data
            byte[] inputBuffer = new byte[BUFFER_SIZE];
            int bytesRead = 0;

            //Read packet from stream
            bytesRead = await networkStream.ReadAsync(inputBuffer, 0, inputBuffer.Length);

            //Add read data to the read buffer
            await readBuffer.WriteAsync(inputBuffer, 0, bytesRead);

            //Determine if there are terminators in the buffer, because that would signify
            //at least 1 full record. 
            var readBufferArray = readBuffer.ToArray();
            var terminatorStrings = readBufferArray
                .Select((v, i) => new { Value = v, Index = i })
                .Where(a => a.Index < readBufferArray.Length - 1
                    && a.Value == LF
                    && readBufferArray[a.Index + 1] == CR).ToList();

            //Find all full messages in the readBuffer, and process them.
            readBuffer.Position = 0;
            foreach (var termString in terminatorStrings)
            {
                var messageLength = (int)termString.Index - (int)readBuffer.Position;
                var messageContents = new byte[messageLength];
                await readBuffer.ReadAsync(messageContents, 0, messageLength);

                OnRawMessageReceived(new RawMessageReceivedEventArgs(messageContents));
                
                //Increase position by terminator string width.
                readBuffer.Position += 2;

            }
            //All full messages have been processed, so copy whatever is still in the buffer to the
            //start of a new reset buffer.
            var finalPosition = (int)readBuffer.Position;
            var bufferLength = (int)readBuffer.Length;
            var bytesLeft = bufferLength - finalPosition;

            //Create a new readBuffer that can hold the current remaining data and a full extra read.
            readBuffer = new MemoryStream((bytesLeft > 0) ? BUFFER_SIZE + bytesLeft : BUFFER_SIZE);

            if (bytesLeft > 0)
            {
                await readBuffer.WriteAsync(readBufferArray, finalPosition, bytesLeft);
            }


        }

        /// <summary>
        /// Writes an IRC message to the output stream.
        /// </summary>
        internal async Task WriteAsync(IrcMessage message)
        {
            var raw = message.GetRawMessage();
            await networkStream.WriteAsync(raw, 0, raw.Length);
        }

        #endregion

        #region Private Methods

        private IrcMessage ParseMessage(byte[] rawMessage)
        {
            var ircMsg = new IrcMessage();
            var raw = IrcClient.Encoding.GetString(rawMessage);

            //If first character of the first word is a colon, this message has a prefix.
            if (raw.Substring(0, 1) == ":")
            {
                ircMsg.Prefix = raw.Substring(1, raw.IndexOf(' ') - 1);
                raw = raw.Substring(raw.IndexOf(' ') + 1);
            }

            ircMsg.Command = raw.Substring(0, raw.IndexOf(' '));
            raw = raw.Substring(raw.IndexOf(' ') + 1);

            //The message now only has parameters left. Normal parameters are split by SPACE, and a last parameter
            //that is prefixed by COLON that can contain spaces. First the remaining raw message is split into two
            //parts, before and after the COLON (if present). Everything before the colon will be split at SPACE
            //and treated as seperate parameters. Everything after the COLON will be treated as a single parameter,
            //if it exists at all.
            var remainder = raw.Split(new[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);

            ircMsg.Parameters.AddRange(remainder[0].Split(' ')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                );

            if (remainder.Length == 2)
            {
                ircMsg.Parameters.Add(remainder[1]);
            }

            return ircMsg;
        }

        #endregion

        #region Events

       

        #endregion

        #region Event Handlers

        public virtual void OnRawMessageReceived(RawMessageReceivedEventArgs e)
        {
            if (RawMessageReceived != null)
                RawMessageReceived(this, e);

        }

        #endregion

    }
}
