using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Entalyan.EntIRC;
using Entalyan.EntIRC.Events;
using Entalyan.EntIRC.IRCProtocol.Messages;
using System.Net;
using System.Collections.Concurrent;

namespace Entalyan.EntIRCGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Some values to use while the settings system has not been implemented
        private const string NICKNAME = "";
        private const string USERNAME = "";
        private const string PASSWORD = "";
        private const string REALNAME = "Custom IRC Client Tester";
        private const string SERVER_IP = "";
        private const int SERVER_PORT = 0;

        private IrcClient client;

        public MainWindow()
        {
            InitializeComponent();

            //Set up the required variables
            client = new IrcClient();

            //Connect event handlers
            client.MessageReceived += HandleIncomingMessage;



        }

        #region UI Event Handlers

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: This needs a better solution. User needs to be maintained in the IrcClient, not the UI.
            var user = new IrcUser(NICKNAME, USERNAME, PASSWORD, REALNAME, SERVER_IP, SERVER_PORT);

            try
            {
                await client.ConnectAsync(user);
            }
            catch (Exception ex)
            {
                //DEBUG: For debugging purposes, I just want to catch all exceptions to be able to inspect them.
                txtMainOutput.AppendText(ex.Message);
                txtMainOutput.AppendText(ex.InnerException.Message);

            }
        }

        /// <summary>
        /// When the send button is clicked, send the text in the input box to the IRC server, on the channel that
        /// is currently selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await client.SendMessageAsync(txtMainInput.Text);
            }
            catch (Exception ex)
            {
                //For debugging purposes, I just want to catch all exceptions to be able to inspect them.
                txtMainOutput.AppendText(ex.Message);
                txtMainOutput.AppendText(ex.InnerException.Message);

            }
            finally
            {
                txtMainInput.Text = string.Empty;
            }

        }

        #endregion

        #region Backend Event Handlers

        /// <summary>
        /// Writes an incoming message to the appropriate control or buffer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleIncomingMessage(object sender, MessageEventArgs e)
        {
            txtMainOutput.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        txtMainOutput.AppendText(e.Message.RawString);
                        txtMainOutput.AppendText(Environment.NewLine);
                        txtMainOutput.ScrollToEnd();
                    }
                )
            );

        }

        #endregion
    }
}
