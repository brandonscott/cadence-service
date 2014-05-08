using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
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
using System.Windows.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PubNubMessaging.Core;
using CadenceService.Models;

namespace CadenceService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<string> userList; 
        private Pubnub pn;
        private static string channelName = "test";
        private bool disconnectShown = false;

        public MainWindow()
        {
            InitializeComponent();
            userList = new ObservableCollection<string>();
            UserListView.ItemsSource = userList;

            //var timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 1), DispatcherPriority.Normal,
                //(sender, args) => AddUsers()), Dispatcher);

            pn = new Pubnub("pub-c-18bc7bd1-2981-4cc4-9c4e-234d25519d36", "sub-c-5782df52-d147-11e3-93dd-02ee2ddab7fe");

            pn.Subscribe<string>(channelName, delegate(string o) { }, delegate(string o) { }, delegate(PubnubClientError error) { });
            pn.Presence<string>(channelName, OnUserPresence, OnPresenceConnect, OnPresenceError);

            ConnectionStatusLabel.Content = "Connected";
            ConnectionStatusLabel.Foreground = Brushes.White;
            ConnectButton.Click -= ConnectButton_OnClick;
            ConnectButton.Opacity = 0.3;
        }

        private void DisplayErrorMessage(PubnubClientError obj)
        {
            throw new NotImplementedException();
        }

        private void DisplayReturnMessage(string obj)
        {
            throw new NotImplementedException();
        }
        private void  AddUsers(string info)
        {
            string jString = info.Substring(1).Remove(info.IndexOf("},\""));
            PresenceList presence =  JsonConvert.DeserializeObject<PresenceList>(jString);

            Dispatcher.Invoke(delegate
            {
                userList.Clear();
                numUsersTextBlock.Text = presence.occupancy.ToString();

                foreach (Uuid uuid in presence.uuids)
                {
                    userList.Add(uuid.uuid);
                }
            });
        }

        private void OnPresenceError(PubnubClientError obj)
        {
            if (!disconnectShown)
            {
                disconnectShown = true;
                MessageBox.Show("Error. Service disconnected.");
                MessageBox.Show(obj.ToString());
            }
            
            Dispatcher.Invoke(delegate
            {
                ConnectionStatusLabel.Content = "Disconnected";
                ConnectionStatusLabel.Foreground = Brushes.Black;
                ConnectButton.Click += ConnectButton_OnClick;
                ConnectButton.Opacity = 1;
            });
        }

        private void OnPresenceConnect(string obj)
        {
            // Presuming that these are connect/disconnect notifications
            pn.HereNow<string>(channelName, true, true, AddUsers, DisplayErrorMessage);
        }

        private void OnUserPresence(string obj)
        {
            // Presuming that these are messages, not connect/disconnect notifications
            pn.HereNow<string>(channelName, true, true, AddUsers, DisplayErrorMessage);

            string jString = obj.Substring(1).Remove(obj.IndexOf("},\""));
            UserAction userAction = JsonConvert.DeserializeObject<UserAction>(jString);

            Dispatcher.Invoke(delegate
            {
                DebugListView.Items.Add(userAction);
            });

            UpdatePresence(userAction);
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            pn.Subscribe<string>(channelName, delegate(string o) { }, delegate(string o) { }, delegate(PubnubClientError error) { });
            pn.Presence<string>(channelName, OnUserPresence, OnPresenceConnect, OnPresenceError);

            ConnectionStatusLabel.Content = "Connected";
            ConnectionStatusLabel.Foreground = Brushes.White;
            ConnectButton.Click -= ConnectButton_OnClick;
            ConnectButton.Opacity = 0.3;
        }

        private void UpdatePresence(UserAction presence)
        {
            int status = 0;

            if (presence.action.Equals("join"))
            {
                status = 1;
            }

            WebClient wc = new WebClient();
            NetworkCredential nc = new NetworkCredential("brandon@brandonscott.co.uk", "Cadenc3!");
            wc.Credentials = nc;

            NameValueCollection nvc = new NameValueCollection()
            {
                { "status", status.ToString(CultureInfo.InvariantCulture) }
            };

            wc.UploadValues(string.Format("http://cadence-bu.cloudapp.net/servers/{0}/status", presence.uuid), "PUT", nvc);
        }
    }
}