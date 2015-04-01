using ChatClient.ViewModels;
using MVVM.Container;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var container = new MicrokernelContainer();

            //Interface entre les braquette ?
            container.RegisterA<LobbyViewModel>(typeof(LobbyViewModel));
            container.RegisterA<LoginViewModel>(typeof(LoginViewModel));
            container.RegisterA<ProfileViewModel>(typeof(ProfileViewModel));
            container.RegisterA<RoomViewModel>(typeof(RoomViewModel));

            Container.InitializeContainerWith(container);
        }
    }
}
