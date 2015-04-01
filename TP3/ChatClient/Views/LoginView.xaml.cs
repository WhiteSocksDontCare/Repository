using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using ChatCommunication;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        User u;

        public MainWindow()
        {
            InitializeComponent();
            u = new User();
        }

        private string Encode_Pass(string pass)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("trYT0" + pass + "H4cKme");
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return System.Text.Encoding.ASCII.GetString(data);
        }

        private void BTN_Connect_Click(object sender, RoutedEventArgs e)
        {
            u.Pseudo = TB_Username_Login.Text;
            u.Password = Encode_Pass(PB_Password_Login.Password);

            if (!Client.StartClient(u))
            {

            }
        }
    }
}
