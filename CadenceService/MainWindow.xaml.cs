// ---------------------------------------------------------------------------------------
//  <copyright file="MainWindow.xaml.cs" company="Cadence">
//      Copyright © 2013-2014 by Brandon Scott and Christopher Franklin.
// 
//      Permission is hereby granted, free of charge, to any person obtaining a copy of
//      this software and associated documentation files (the "Software"), to deal in
//      the Software without restriction, including without limitation the rights to
//      use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//      of the Software, and to permit persons to whom the Software is furnished to do
//      so, subject to the following conditions:
// 
//      The above copyright notice and this permission notice shall be included in all
//      copies or substantial portions of the Software.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//      IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//      FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//      AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
//      WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
//      CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
//  </copyright>
//  ---------------------------------------------------------------------------------------

#region

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Media;
using CadenceService.Models;
using Newtonsoft.Json;
using PubNubMessaging.Core;

#endregion

namespace CadenceService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string channelName = "test";
        private readonly Pubnub pn;
        private readonly ObservableCollection<string> userList;
        private bool disconnectShown;

        public MainWindow()
        {
            InitializeComponent();
            userList = new ObservableCollection<string>();
            UserListView.ItemsSource = userList;

            //var timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 1), DispatcherPriority.Normal,
            //(sender, args) => AddUsers()), Dispatcher);

            pn = new Pubnub("pub-c-18bc7bd1-2981-4cc4-9c4e-234d25519d36", "sub-c-5782df52-d147-11e3-93dd-02ee2ddab7fe");

            pn.Subscribe(channelName, delegate { }, delegate { }, delegate { });
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

        private void AddUsers(string info)
        {
            string jString = info.Substring(1).Remove(info.IndexOf("},\""));
            PresenceList presence = JsonConvert.DeserializeObject<PresenceList>(jString);

            Dispatcher.Invoke(delegate
            {
                userList.Clear();
                NumUsersTextBlock.Text = presence.occupancy.ToString();

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

            Dispatcher.Invoke(delegate { DebugListView.Items.Add(userAction); });

            UpdatePresence(userAction);
        }

        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            pn.Subscribe(channelName, delegate { }, delegate { }, delegate { });
            pn.Presence<string>(channelName, OnUserPresence, OnPresenceConnect, OnPresenceError);

            ConnectionStatusLabel.Content = "Connected";
            ConnectionStatusLabel.Foreground = Brushes.White;
            ConnectButton.Click -= ConnectButton_OnClick;
            ConnectButton.Opacity = 0.3;
        }

        private void UpdatePresence(UserAction presence)
        {
            try
            {
                var status = 0;

                if (presence.action.Equals("join"))
                {
                    status = 1;
                }

                var wc = new WebClient();
                var nc = new NetworkCredential("brandon@brandonscott.co.uk", "Cadenc3!");
                wc.Credentials = nc;

                var nvc = new NameValueCollection
                {
                    {"status", status.ToString(CultureInfo.InvariantCulture)}
                };

                wc.UploadValues(string.Format("http://cadence-bu.cloudapp.net/servers/{0}/status", presence.uuid), "PUT",
                    nvc);
            }
            catch (Exception)
            {
                UpdatePresence(presence);
            }
        }
    }
}