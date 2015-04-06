using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatCommunication;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;
using MVVM.Container;

namespace ChatClient.ViewModels
{
    class EditProfileViewModel : BindableBase
    {
        Profile _profile;

        public EditProfileViewModel()
        {
            BrowseCommand = new DelegateCommand(BrowseImage);
            CancelCommand = new DelegateCommand(CancelModification);
            SaveCommand = new DelegateCommand(SaveModification);
            ReturnToLobbyCommand = new DelegateCommand(ReturnToLobby);
        }

        public Profile Profile
        {
            get { return _profile; }
            set { SetProperty(ref _profile, value); }
        }

        public ICommand CancelCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand ReturnToLobbyCommand { get; private set; }
        public ICommand BrowseCommand { get; private set; }

        public void BrowseImage()
        {
            //TODO: Browse an image.
        }

        public void SaveModification()
        {
            bool succes = true;
            if (succes)
            {
                Container.GetA<ProfileViewModel>().Profile = this.Profile;
                Container.GetA<LobbyViewModel>().Lobby.ClientProfile = this.Profile;
                Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LobbyViewModel>());
            }
        }

        public void CancelModification()
        {
            bool succes = true;
            if (succes)
            {
                Container.GetA<MainViewModel>().NavigateToView(Container.GetA<ProfileViewModel>());
            }
        }

        public void ReturnToLobby()
        {
            Container.GetA<MainViewModel>().NavigateToView((Container.GetA<LobbyViewModel>()));
        }
    }
}
