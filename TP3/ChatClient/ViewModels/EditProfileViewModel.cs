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

        public EditProfileViewModel()
        {
            BrowseCommand = new DelegateCommand<object>(BrowseImage);
            CancelCommand = new DelegateCommand(CancelModification);
            SaveCommand = new DelegateCommand(SaveModification);
            _profile = new Profile();
        }

        public Profile Profile
        {
            get { return _profile; }
            set { SetProperty(ref _profile, value); }
        }

        public ICommand CancelCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand BrowseCommand { get; private set; }

        public void BrowseImage(object imageControl)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp; *.png)|*.jpg; *.jpeg; *.gif; *.bmp; *.png";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(fileDialog.FileName);
                image.EndInit();
                ((Image)imageControl).Source = image;
                Profile.AvatarUri = fileDialog.FileName;
            }
        }

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
