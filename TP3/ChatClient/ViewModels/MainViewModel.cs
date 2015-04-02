using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatClient.ViewModels
{
    class MainViewModel : BindableBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return this._currentView; }
            set { SetProperty(ref this._currentView, value); }
        }

        public void NavigateToView(object viewToNavigate)
        {
            CurrentView = viewToNavigate;
        }
    }
}
