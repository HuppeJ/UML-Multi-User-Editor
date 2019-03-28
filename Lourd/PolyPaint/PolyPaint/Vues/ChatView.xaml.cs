﻿using System;
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
using PolyPaint.Modeles;
using PolyPaint.Utilitaires;
using PolyPaint.VueModeles;

namespace PolyPaint.Vues
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
        }

        private void CreateRoomPopup(object sender, EventArgs e)
        {
            popUpCreateRoomVue.Initialize();
            popUpCreateRoomVue.RoomNameTextBox.Text = "";
            popUpCreateRoom.IsOpen = true;
            IsEnabled = false;
        }

        private void JoinRoomPopup(object sender, EventArgs e)
        {
            popUpJoinRoomVue.Initialize();
            popUpJoinRoomVue.joinableRooms = ((Button)sender).Tag as AsyncObservableCollection<Room>;
            popUpJoinRoom.IsOpen = true;
            IsEnabled = false;
        }

        public void ClosePopup()
        {
            popUpCreateRoom.IsOpen = false;
            popUpJoinRoom.IsOpen = false;
            IsEnabled = true;
        }
    }
}
