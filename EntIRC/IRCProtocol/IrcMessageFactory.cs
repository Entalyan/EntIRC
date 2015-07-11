using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entalyan.EntIRC.IRCProtocol.Messages;

namespace Entalyan.EntIRC.IRCProtocol
{
    public enum MessageTypes
    {
        ADMIN = 0,
        CONNECT,
        ERROR,
        INFO,
        INVITE,
        JOIN,
        KICK,
        KILL,
        LINKS,
        LIST,
        LUSERS,
        MODE,
        MOTD,
        NAMES,
        NICK,
        NOTICE,
        OPER,
        PART,
        PASS,
        PING,
        PONG,
        PRIVMSG,
        QUIT,
        SERVICE,
        SERVLIST,
        SQUERY,
        SQUIT,
        STATS,
        TIME,
        TOPIC,
        TRACE,
        USER,
        VERSION,
        WHO,
        WHOIS,
        WHOWAS,
        INTERNALERROR = 99
    }


    public sealed class IrcMessageFactory
    {
        private static Dictionary<MessageTypes, Func<string, IrcMessage>> messageMapper;

        static IrcMessageFactory()
        {
            /* I am no longer sure a Dictionary is very useful. A switch statement would do
             * just fine as well. Also the whole concept of parsing a Command from the raw
             * received string, then using that to find a match in an enumerator seems
             * useless, a direct string comparison would do just as well.
             */
            messageMapper = new Dictionary<MessageTypes, Func<string, IrcMessage>>()
            {
                //{ MessageTypes.ADMIN, () => {  } },
                //{ MessageTypes.CONNECT, () => { } },
                //{ MessageTypes.ERROR, () => { } },
                //{ MessageTypes.INFO, () => { } },
                //{ MessageTypes.INVITE , () => { } },
                //{ MessageTypes.JOIN, () => { } },
                //{ MessageTypes.KICK, () => { } },
                //{ MessageTypes.KILL, () => { } },
                //{ MessageTypes.LINKS, () => { } },
                //{ MessageTypes.LIST, () => { } },
                //{ MessageTypes.LUSERS, () => { } },
                //{ MessageTypes.MODE, () => { } },
                //{ MessageTypes.MOTD, () => { } },
                //{ MessageTypes.NAMES, () => { } },
                //{ MessageTypes.NICK, () => { } },
                { MessageTypes.NOTICE, (msg) => { return new PassMessage(msg); } },
                //{ MessageTypes.OPER, () => { } },
                //{ MessageTypes.PART, () => { } },
                { MessageTypes.PASS, (msg) => { return new PassMessage(msg); } },
                //{ MessageTypes.PING, () => { } },
                //{ MessageTypes.PONG, () => { } },
                { MessageTypes.PRIVMSG, (msg) => { return new PrivateMessage(msg); } },
                //{ MessageTypes.QUIT, () => { } },
                //{ MessageTypes.SERVICE, () => { } },
                //{ MessageTypes.SERVLIST, () => { } },
                //{ MessageTypes.SQUERY, () => { } },
                //{ MessageTypes.SQUIT, () => { } },
                //{ MessageTypes.STATS, () => { } },
                //{ MessageTypes.TIME, () => { } },
                //{ MessageTypes.TOPIC, () => { } },
                //{ MessageTypes.TRACE, () => { } },
                //{ MessageTypes.USER, () => { } },
                //{ MessageTypes.VERSION, () => { } },
                //{ MessageTypes.WHO, () => { } },
                //{ MessageTypes.WHOIS, () => { } },
                //{ MessageTypes.WHOWAS, () => { } }
                { MessageTypes.INTERNALERROR, (msg) => { return new UnknownMessage(msg); } }
            };
        }

        public static IrcMessage ParseIrcMessageFromRaw(string fullMessage)
        {
            //Detect the type of message that was received
            var msgType = IrcMessageFactory.IdentifyMessageType(fullMessage);

            //Select the matching Message from the dictionary.
            var ircMsg = (from messageType in messageMapper
                          where messageType.Key == msgType
                          select messageType.Value(fullMessage)).FirstOrDefault();
                        ;
            
            //If no matching Message was found the Message must not have been implemented yet.
            var result = (ircMsg != default(IrcMessage)) ? ircMsg : new NotImplementedMessage(fullMessage);

            //Return the IrcMessage that was selected from the dictionary. 
            return result;

        }

        #region Private Methods

        /// <summary>
        /// Identifies a message according to the IRC specification. 
        /// </summary>
        /// <param name="rawMessage"></param>
        /// <returns>The MessageType of the input raw message if this matched the IRC specification, otherwise
        /// returns MessageTypes.INTERNALERROR.</returns>
        /// <remarks>This does not mean the MessageType was actually implemented!</remarks>
        private static MessageTypes IdentifyMessageType(string rawMessage)
        {
            //Check if command is in first or second position.
            var messageElements = rawMessage.Split(' ');
            string command = messageElements[0][0] == ':' ? messageElements[1] : messageElements[0];

            //Check if command matches a defined MessageType, otherwise return an internal error. 
            return Enum.IsDefined(typeof(MessageTypes), command) ? (MessageTypes)Enum.Parse(typeof(MessageTypes), command) : MessageTypes.INTERNALERROR;
        }


        #endregion



    }
}
