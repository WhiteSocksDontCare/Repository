using ChatCommunication;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using MVVM.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    class CreateRoomViewModel : BindableBase
    {
        Room _room;

        public CreateRoomViewModel()
        {
            CancelCommand = new DelegateCommand(CancelCreation);
            SaveCommand = new DelegateCommand(CreateRoom);
            _room = new Room();
        }

        public Room Room
        {
            get { return _room; }
            set { SetProperty(ref _room, value); }
        }

        public ICommand CancelCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public void CancelCreation()
        {
            Container.GetA<MainViewModel>().NavigateToView((Container.GetA<LobbyViewModel>()));
        }

        public void CreateRoom()
        {
            Client.CreateRoom(Room);
            Container.GetA<MainViewModel>().NavigateToView((Container.GetA<LobbyViewModel>()));
        }
    }
}
