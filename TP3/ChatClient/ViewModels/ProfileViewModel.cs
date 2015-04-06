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

//Ceci est un profil dans une liste
namespace ChatClient.ViewModels
{
    class ProfileViewModel : BindableBase
    {
        private Profile _profile;

        public ProfileViewModel()
        {
            ViewProfileCommand = new DelegateCommand(ViewProfile);
            _profile = new Profile();
        }

        public Profile Profile
        {
            get { return _profile; }
            set { SetProperty(ref _profile, value); }
        }

        public ICommand ViewProfileCommand { get; private set; }


        public void ViewProfile()
        {
            //SendMessage

            //then
            Container.GetA<MainViewModel>().NavigateToView(Container.GetA<ViewProfileViewModel>());
        }
    }
}
