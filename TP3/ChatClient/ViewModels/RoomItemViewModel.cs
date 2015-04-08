using ChatCommunication;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    class RoomItemViewModel : BindableBase
    {
        private Room _roomItem;

        public RoomItemViewModel()
        {
            JoinCommand = new DelegateCommand(JoinRoom);
            _roomItem = new Room();
        }

        public Room RoomItem
        {
            get { return _roomItem; }
            set { SetProperty(ref _roomItem, value); }
        }

        public ICommand JoinCommand { get; private set; }

        public void JoinRoom()
        {
            MessageBox.Show("Join");
        }
    }
}
