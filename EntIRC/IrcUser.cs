using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entalyan.EntIRC.IRCProtocol;
using Entalyan.EntIRC.IRCProtocol.Messages;

namespace Entalyan.EntIRC
{
    /// <summary>
    /// Represents a user for connecting to an IRC server.
    /// </summary>
    public class IrcUser : IEquatable<IrcUser>
    {
        #region Private Fields

        private string nickValue = string.Empty;
        private string userValue = string.Empty;
        private string passValue = string.Empty;
        private string realValue = string.Empty;
        private string ipValue = string.Empty;
        private int portValue;

        #endregion

        #region Properties

        public string Nickname { get { return nickValue; } }
        public string UserName { get { return userValue; } }
        public string Password { get { return passValue; } }
        public string RealName { get { return realValue; } }
        public string HostAddress { get { return ipValue; } }
        public int HostPort { get { return portValue; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new IrcUser object.
        /// </summary>
        /// <param name="nickName">The users nickname.</param>
        /// <param name="userName">The users username.</param>
        /// <param name="passWord">The users password.</param>
        /// <param name="realName">The users real name.</param>
        public IrcUser(string nickname, string userName, string password, string realName, string hostAddress, int hostPort)
        {
            this.nickValue = nickname;
            this.userValue = userName;
            this.passValue = password;
            this.realValue = realName;
            this.ipValue = hostAddress;
            this.portValue = hostPort;
        }

        #endregion

        #region Public Methods


        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            var convertedObject = obj as IrcMessage;
            if (convertedObject != null)
            {
                return this.Equals(convertedObject);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(IrcUser other)
        {
            if ((object)other != null)
            {
                return (this.Nickname == other.Nickname)
                   && (this.UserName == other.UserName)
                   && (this.Password == other.Password)
                   && (this.RealName == other.RealName);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Nickname.GetHashCode() ^ UserName.GetHashCode() ^ Password.GetHashCode() ^ RealName.GetHashCode();
        }

        public static bool operator ==(IrcUser left, IrcUser right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IrcUser left, IrcUser right)
        {
            return !(left.Equals(right));
        }

        #endregion
    }
}
