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
            BrowseCommand = new DelegateCommand(BrowseImage);
            EditCommand = new DelegateCommand(EditProfile);
            LeaveCommand = new DelegateCommand(LeaveProfile);
        }

        public Profile Profile
        {
            get { return _profile; }
            set { SetProperty(ref _profile, value); }
        }


        public ICommand BrowseCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand LeaveCommand { get; private set; }

        public void BrowseImage()
        {
            //TODO: Browse an image.
        }

        public void EditProfile()
        {
            Container.GetA<EditProfileViewModel>().Profile = this.Profile;
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<EditProfileViewModel>());
        }

        public void LeaveProfile()
        {
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<LobbyViewModel>());
        }
    }
}
