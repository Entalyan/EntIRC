using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
        //private Stream networkStream;
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

        /// <summary>
        /// The user this client is logged in as.
        /// </summary>
        public IrcUser User { get; private set; }

        /// <summary>
        /// Determines if SSL will be used to connect.
        /// </summary>
        public bool UseSSL { get; private set; }

        /// <summary>
        /// Determines if invalid SSL certificates are ignored.
        /// </summary>
        public bool IgnoreInvalidSSL { get; private set; }

        /// <summary>
        /// The hostname of the server this client connects to.
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// The port number of the server this client connects to.
        /// </summary>
        public int HostPort { get; private set; }

        /// <summary>
        /// The low level TCP stream for this client.
        /// </summary>
        public Stream NetworkStream { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IrcClient"/> class.
        /// </summary>
        public IrcClient(IrcUser user, string hostName, int hostPort, bool useSSL = false, bool ignoreInvalidSSL = false)
        {
            Connected = false;
            networkClient = new TcpClient(); //TODO: Replace with Socket
            User = user;
            UseSSL = useSSL;
            IgnoreInvalidSSL = ignoreInvalidSSL;
            HostName = hostName;
            HostPort = hostPort;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method will connect to the IRC server using the connection
        /// information specified for this user. When connected the listener
        /// loop will start in order for messages to pass in from the server
        /// to the client.
        /// </summary>
        public async Task ConnectAsync()
        {

            //This method will handle connecting to the IRC server, and starting
            //and maintaining the listener/writer. 
            await networkClient.ConnectAsync(HostName, HostPort)
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

                //Create the appropriate Stream depending on connection parameters.
                try
                {

                    if (UseSSL)
                    {
                        if (IgnoreInvalidSSL)
                        {
                            NetworkStream = new SslStream(networkClient.GetStream(), false, (sender, certificate, chain, policyErrors) => true);
                        }
                        else
                        {
                            NetworkStream = new SslStream(networkClient.GetStream());
                        }

                        //Authenticate the certificate.
                        ((SslStream)NetworkStream).AuthenticateAsClient(HostName);

                    }
                    else
                    {
                        NetworkStream = networkClient.GetStream();
                    }

                    //Set up the input and output streams
                    writeBuffer = new StreamWriter(NetworkStream, IrcClient.Encoding) { AutoFlush = true };
                    readBuffer = new StreamReader(NetworkStream, IrcClient.Encoding);
                }
                catch (AuthenticationException authEx)
                {
                    throw new InvalidOperationException("Validating the SSL certificate has failed.", authEx);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Connection could not be established.", ex);
                }

                //Now that connection has been initialized, start the login sequence.
                await SendMessageAsync(string.Format("PASS {0}", User.Password));
                await SendMessageAsync(string.Format("NICK {0}", User.Nickname));
                await SendMessageAsync(string.Format("USER {0} {1} {2} :{3}", User.UserName, "hostname", "servername", User.RealName));

                //ZNC doesn't accept the password the first time around, but does the second time
                await SendMessageAsync(string.Format("PASS {0}", User.Password));

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
