using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ChatCommunication;
using Microsoft.Practices.Prism.Commands;
using MVVM.Container;
using System.Collections.ObjectModel;
using ChatClient.Utils;
using System.Windows.Forms;

namespace ChatClient.ViewModels
{
    class LobbyViewModel : BindableBase
    {
        private Lobby _lobby;
        private RoomViewModel _roomViewModel;        

        public LobbyViewModel()
        {
            DisconnectCommand = new DelegateCommand(Disconnect);
            EditProfileCommand = new DelegateCommand(EditProfile);
            ViewProfileCommand = new DelegateCommand(ViewProfile);
            CreateRoomCommand = new DelegateCommand(CreateRoom);

            this.JoinRoomCommand = new DelegateCommand<Room>(JoinRoom);
            this.ViewOtherProfileCommand = new DelegateCommand<Profile>(ViewOtherProfile);
            
            _lobby = new Lobby();
            _roomViewModel = new RoomViewModel();
        }

        public Lobby Lobby
        {
            get { return _lobby; }
            set { SetProperty(ref _lobby, value); }
        }

        public RoomViewModel RoomViewModel
        {
            get { return _roomViewModel; }
        }
 
        public bool IsInRoom
        {
            //True si l'user a pas -1 et si on a recu la bonne room dans le updateRoom!
            get 
            {
                Console.WriteLine("Lobby.ClientProfile.IDRoom : {0}", Lobby.ClientProfile.IDRoom);
                Console.WriteLine("RoomViewModel.Room.IDRoom : {0}", RoomViewModel.Room.IDRoom);                
                return Lobby.ClientProfile.IDRoom != -1 && Lobby.ClientProfile.IDRoom == RoomViewModel.Room.IDRoom; 
            }
        }

        public ICommand DisconnectCommand { get; private set; }
        public ICommand EditProfileCommand { get; private set; }
        public ICommand ViewProfileCommand { get; private set; }
        public ICommand CreateRoomCommand { get; private set; }
        public ICommand JoinRoomCommand { get; private set; }
        public ICommand ViewOtherProfileCommand { get; private set; }

        public void Disconnect()
        {
            Client.DisconnectClient();
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LoginViewModel>());
        }

        public void EditProfile()
        {
            Container.GetA<EditProfileViewModel>().Profile = Lobby.ClientProfile;
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<EditProfileViewModel>());
        }

        public void ViewProfile()
        {
            Client.ViewProfile(Lobby.ClientProfile.Pseudo);
        }

        public void ViewProfileCallback()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<ViewProfileViewModel>());
        }

        public void CreateRoom()
        {
            Container.GetA<CreateRoomViewModel>().Room = new Room();
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<CreateRoomViewModel>());
        }

        public void JoinRoom(Room room)
        {
            Client.JoinRoom(room.IDRoom);
        }

        public void ViewOtherProfile(Profile profile)
        {
            Client.ViewProfile(profile.Pseudo);
        }
    }
}   
