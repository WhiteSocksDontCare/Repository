using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Commands;
using System.Windows.Input;
using ChatCommunication;
using MVVM.Container;

namespace ChatClient.ViewModels
{
    class ProfileViewModel  : BindableBase
    {

        Profile _profile;

        public ProfileViewModel()
        {
            EditCommand = new DelegateCommand(EditProfile);
            ReturnToLobbyCommand = new DelegateCommand(ReturnToLobby);
        }

        public Profile Profile
        {
            get { return _profile; }
            set { SetProperty(ref _profile, value); }
        }

        public ICommand EditCommand { get; private set; }
        public ICommand ReturnToLobbyCommand { get; private set; }


        public void EditProfile()
        {
            Container.GetA<EditProfileViewModel>().Profile = this.Profile;
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<EditProfileViewModel>());
        }

        public void ReturnToLobby()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LobbyViewModel>());
        }
    }
}
