using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Entalyan.EntIRC.Events;
using Entalyan.EntIRC.IRCProtocol;


namespace Entalyan.EntIRC
{
    /// <summary>
    /// This class provides access to an IRC server. It provides methods
    /// to manage a connection, send and receive data, and exposes received
    /// data for consumption.
    /// </summary>
    public class IrcClient
    {
        #region Constants

        private const string ENCODING = "UTF-8";

        #endregion

        #region Private Fields

        private TcpClient networkClient;
        private StreamWriter writeBuffer;
        private StreamReader readBuffer;
        private bool isConnected;


        #endregion

        #region Properties

        /// <summary>
        /// Gets the encoding used on all messages.
        /// </summary>
        internal static Encoding Encoding { get { return Encoding.GetEncoding(ENCODING); } }

        /// <summary>
        /// Gets a value indicating if the IRC client is currently connected to a server.
        /// </summary>
        public bool Connected
        {
            get
            {
                //TODO: Include more sophisticated code for determining connectivity, instead
                //      of basing it purely on the socket connection.
                return isConnected;
            }
            private set
            {
                isConnected = value;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcClient"/> class.
        /// </summary>
        public IrcClient()
        {
            networkClient = new TcpClient();
            isConnected = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method will connect to the IRC server using the connection
        /// information specified for this user. When connected the listener
        /// loop will start in order for messages to pass in from the server
        /// to the client.
        /// </summary>
        /// <param name="user">The IrcUser that will log in to the server.</param>
        /// <returns></returns>
        public async Task ConnectAsync(IrcUser user)
        {
            
            //This method will handle connecting to the IRC server, and starting
            //and maintaining the listener/writer. 
            await networkClient.ConnectAsync(user.HostAddress, user.HostPort)
                .ContinueWith(async (connect) =>
            {
                //Determine if connection was succesfully established.
                switch (connect.Status)
                {
                    case TaskStatus.RanToCompletion:
                        //Success
                        this.Connected = true;
                        break;
                    default:
                        //In all other cases assume connection has failed.
                        throw new InvalidOperationException("Connection to IRC server has failed.", connect.Exception.Flatten());
                }

                //Set up the input and output streams
                var networkStream = networkClient.GetStream();
                writeBuffer = new StreamWriter(networkStream, IrcClient.Encoding) { AutoFlush = true };
                readBuffer = new StreamReader(networkStream, IrcClient.Encoding);

                //Now that connection has been initialized, start the login sequence.
                await SendMessageAsync(string.Format("PASS {0}", user.Password));
                await SendMessageAsync(string.Format("NICK {0}", user.Nickname));
                await SendMessageAsync(string.Format("USER {0} {1} {2} :{3}", user.UserName, "hostname", "servername", user.RealName));

                //ZNC doesn't accept the password the first time around, but does the second time
                await SendMessageAsync(string.Format("PASS {0}", user.Password));
                
            }, TaskContinuationOptions.OnlyOnRanToCompletion
            ).ContinueWith((setup) =>
            {
                //Start the listener on a seperate thread.
                Task.Run(async () =>
                {
                    while (this.Connected)
                    {
                        var rawMessage = await readBuffer.ReadLineAsync();
                        var parsedMessage = IrcMessageFactory.ParseIrcMessageFromRaw(rawMessage);


                        OnMessageReceived(new MessageEventArgs(parsedMessage));
                    }
                }
                );

            }, TaskContinuationOptions.LongRunning
            );

        }

        /// <summary>
        /// Sends an IrcMessage to the connected IRC server.
        /// </summary>
        /// <param name="message">The message that will be sent to the server.</param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message)
        {
            if (this.Connected)
            {
                    await writeBuffer.WriteLineAsync(message);
            }
            else
            {
                throw new InvalidOperationException("Cannot send message when not connected to server.");
            }
        }

        #endregion

        #region Private Methods


        #endregion

        #region Events

        /// <summary>
        /// This event is raised whenever a message is received from the IRC server.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region Event Handlers

        /// <summary>
        /// When a raw message is received by the connection this event is raised.
        /// </summary>
        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = MessageReceived;

            if (handler != null)
                handler(this, e);
        }

        #endregion

    }


}
