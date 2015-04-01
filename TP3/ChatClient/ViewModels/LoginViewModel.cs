using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatCommunication;

namespace ChatClient.ViewModels
{
    class LoginViewModel : BindableBase 
    {
        private User user;

        public LoginViewModel()
        {
            this.LoginCommand = new DelegateCommand<object>(Login);
            this.SubscribeCommand = new DelegateCommand<object>(Subscribe);
            this.user = new User();
        }

        public ICommand LoginCommand { get; private set; }
        public ICommand SubscribeCommand { get; private set; }

        private void Login(object password)
        {
            user.Password = Encode_Pass((string)password);
            if(Client.StartClient(user))
            {
                //move to lobby
            }
        }

        private void Subscribe(object password)
        {
            user.Password = Encode_Pass((string)password);
            if (Client.StartClient(user))
            {
                //move to profil creation
            }

        }
        private string Encode_Pass(string pass)
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes("trYT0" + pass + "H4cKme");
            data = new System.Security.Cryptography.SHA256Managed().ComputeHash(data);
            return System.Text.Encoding.ASCII.GetString(data);
        }

    }
}
