using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
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
        private ObservableCollection<UserObject> debugList; 
        private Pubnub pn;

        public MainWindow()
        {
            InitializeComponent();

            //var timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 1), DispatcherPriority.Normal,
                //(sender, args) => AddUsers()), Dispatcher);

            pn = new Pubnub("pub-c-18bc7bd1-2981-4cc4-9c4e-234d25519d36", "sub-c-5782df52-d147-11e3-93dd-02ee2ddab7fe");

            pn.Subscribe<string>("test", delegate(string o) { }, delegate(string o) { }, delegate(PubnubClientError error) { });
            pn.Presence<string>("test", OnUserPresence, OnPresenceConnect, OnPresenceError);

            pn.HereNow<string>("test", true, true, AddUsers, DisplayErrorMessage);
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
            //PresenceList list = (PresenceList)JObject.Parse(info);
            PresenceList values =  JsonConvert.DeserializeObject<PresenceList>(info);
            MessageBox.Show(info);
            //UserListView.Items.Add(new UserObject()
            //{
            //    Time = 1,
            //    ConnType = "CPU",
            //    Guid = info
            //});
        }

        private void OnPresenceError(PubnubClientError obj)
        {
            MessageBox.Show("presenceerror");
        }

        private void OnPresenceConnect(string obj)
        {
            MessageBox.Show("presenceconnect");
        }

        private void OnUserPresence(string obj)
        {
            MessageBox.Show("userpresence");
        }
    }
}
