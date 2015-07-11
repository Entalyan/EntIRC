using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace Entalyan.EntIRC.IRCProtocol.Messages
{
    /// <summary>
    /// Represents a message that is used to communicate with an IRC server. 
    /// </summary>
    public class IrcMessage
    {

        #region Properties


        /// <summary>
        /// The optional prefix in an IRC message.
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// An IRC command.
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// All parameters that are passed to the server.
        /// </summary>
        public List<string> Parameters { get; set; }

        public string RawString { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new IrcMessage, containing the commands and messages that go 
        /// to and from the IRC server.
        /// </summary>
        public IrcMessage(string fullMessage)
        {
            this.Parameters = new List<string>();
            this.RawString = fullMessage;

            ParseFullMessage();
        }


#endregion

#region Private Methods
        /// <summary>
        /// Parses a raw message string into a usable format.
        /// </summary>
        /// <param name="fullMessage">A string in raw IRC format.</param>
        /// <returns>A structured object containing the information that was in the message before.</returns>
        /// <remarks>The way this string is parsed, is by identifying an element at the front of the array,
        /// moving that to its correct position, then removing it from the original string. This way we know
        /// we always only need to process the first entry, and we have a way of telling when we're done.
        /// </remarks>
        private void ParseFullMessage()
        {
            /* IRC RawString Syntax:
             * 
             * <message>  ::= [':' <prefix> <SPACE> ] <command> <params> <crlf>
             * <prefix>   ::= <servername> | <nick> [ '!' <user> ] [ '@' <host> ]
             * <command>  ::= <letter> { <letter> } | <number> <number> <number>
             * <SPACE>    ::= ' ' { ' ' }
             * <params>   ::= <SPACE> [ ':' <trailing> | <middle> <params> ]
             * <middle>   ::= <Any *non-empty* sequence of octets not including SPACE
             *                or NUL or CR or LF, the first of which may not be ':'>
             * <trailing> ::= <Any, possibly *empty*, sequence of octets not including
             *                NUL or CR or LF>
             * <crlf>     ::= CR LF
             * SPACE = %x20 ; Whitespace. 
             * crlf = %x0D %x0A ; Carriage return/linefeed. 
             */
            
            //Make a working copy of the full message for parsing.
            var raw = this.RawString;

            //If first character of the first word is a colon, this message has a prefix.
            if (raw.Substring(0, 1) == ":")
            {
                this.Prefix = raw.Substring(1, raw.IndexOf(' ') - 1);
                raw = raw.Substring(raw.IndexOf(' ') + 1);
            }

            this.Command = raw.Substring(0, raw.IndexOf(' '));
            raw = raw.Substring(raw.IndexOf(' ') + 1);

            //The message now only has parameters left. Normal parameters are split by SPACE, and a last parameter
            //that is prefixed by COLON that can contain spaces. First the remaining raw message is split into two
            //parts, before and after the COLON (if present). Everything before the colon will be split at SPACE
            //and treated as seperate parameters. Everything after the COLON will be treated as a single parameter,
            //if it exists at all.
            var remainder = raw.Split(new[] { ":" }, 2, StringSplitOptions.RemoveEmptyEntries);

            this.Parameters.AddRange(remainder[0].Split(' ')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                );

            if (remainder.Length == 2)
            {
                this.Parameters.Add(remainder[1]);
            }

        }
#endregion

    }
}
