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
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace ChatClient.ViewModels
{
    class EditProfileViewModel : BindableBase
    {
        Profile _profile;
        private int _avatarIndex;
        private string _avatarPath;

        public EditProfileViewModel()
        {
            CancelCommand = new DelegateCommand(CancelModification);
            SaveCommand = new DelegateCommand(SaveModification);
            Profile = new Profile();
        }

        public Profile Profile
        {
            get { return _profile; }
            set 
            { 
                SetProperty(ref _profile, value);

                if (_profile.AvatarUri == null)
                    AvatarIndex = 0;
                else
                {
                    int temp;

                    if (!Int32.TryParse(_profile.AvatarUri.Split(new char[] { '/', '.' })[1], out temp))
                        AvatarIndex = 0;
                    else
                        AvatarIndex = temp - 1;
                }
            }
        }

        public int AvatarIndex
        {
            get { return _avatarIndex; }
            set
            {
                _avatarIndex = value;
                Profile.AvatarUri = AvatarPath = "Avatars/" + (_avatarIndex + 1).ToString() + ".jpg";
            }
        }

        public string AvatarPath
        {
            get { return _avatarPath; }
            set 
            { 
                _avatarPath = Path.GetFullPath(value);
                OnPropertyChanged("AvatarPath");
            }
        }
        
        public ICommand CancelCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public void SaveModification()
        {
            Client.UpdateProfile(Profile);
        }
        public void EditProfileCallback(bool result)
        {
            if (result)
            {
                Container.GetA<LobbyViewModel>().Lobby.ClientProfile = this.Profile;
                Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LobbyViewModel>());
            }
        }

        public void CancelModification()
        {
            Container.GetA<MainViewModel>().NavigateToView((Container.GetA<LobbyViewModel>()));
        }
    }
}
