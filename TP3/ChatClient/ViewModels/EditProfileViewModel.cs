using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatCommunication;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    class EditProfileViewModel : BindableBase
    {

        EditProfileViewModel()
        {
            SaveCommand = new DelegateCommand(SaveModification);
            CancelCommand = new DelegateCommand(CancelModification);
        }

        Profile _profile;
        public Profile Profile
        {
            get { return _profile; }
            set { SetProperty(ref _profile, value); }
        }


        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public void SaveModification()
        {
            //TODO: devrait recevoir un un update Lobby
            //Ou setter LobbyViewModel.Lobby.Profile = this.Profile
        }

        public void CancelModification()
        {
            //TODO: redemander un viewProfile pour 
        }
    }
}
