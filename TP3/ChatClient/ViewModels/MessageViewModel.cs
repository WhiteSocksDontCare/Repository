using ChatCommunication;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    class MessageViewModel : BindableBase
    {
        private Message _message;

        public MessageViewModel()
        {
            LikeCommand = new DelegateCommand(LikeMessage);
            DislikeCommand = new DelegateCommand(DislikeMessage);
            DeleteCommand = new DelegateCommand(DeleteMessage);
            _message = new Message();
        }

        public Message Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        public ICommand LikeCommand { get; private set; }
        public ICommand DislikeCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }


        public void LikeMessage()
        {

        }

        public void DislikeMessage()
        {

        }

        public void DeleteMessage()
        {

        }
    }
}
