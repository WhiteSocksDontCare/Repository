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
using System.IO;

namespace ChatClient.ViewModels
{
    class ViewProfileViewModel  : BindableBase
    {

        Profile _profile;
        string _avatarPath;

        public ViewProfileViewModel()
        {
            //EditCommand = new DelegateCommand(EditProfile);
            ReturnToLobbyCommand = new DelegateCommand(ReturnToLobby);
            _profile = new Profile();
        }

        public Profile Profile
        {
            get { return _profile; }
            set
            {
                SetProperty(ref _profile, value);
                AvatarPath = _profile.AvatarUri;
            }
        }

        public string AvatarPath
        {
            get { return _avatarPath; }
            set
            {
                if (value != null)
                    _avatarPath = Path.GetFullPath(value);
                else
                    _avatarPath = "";

                OnPropertyChanged("AvatarPath");
            }
        }

        //public ICommand EditCommand { get; private set; }
        public ICommand ReturnToLobbyCommand { get; private set; }


        //public void EditProfile()
        //{
        //    Container.GetA<EditProfileViewModel>().Profile = this.Profile;
        //    Container.GetA<MainViewModel>().NavigateToView(Container.GetA<EditProfileViewModel>());
        //}

        public void ReturnToLobby()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LobbyViewModel>());
        }
    }
}
