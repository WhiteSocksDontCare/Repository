using System.Collections.Generic;
using MVVM.Container;
using ChatClient.ViewModels;

namespace PMVVM.Commands
{
    public class NavigateToViewCommand : WpfCommand
    {
        private readonly object _viewToNavigate;

        public NavigateToViewCommand(object viewToNavigate) : base("Navigate")
        {
            _viewToNavigate = viewToNavigate;
        }

        protected override void RunCommand(object parameter)
        {
            Container.GetA<MainViewModel>().NavigateToView(_viewToNavigate);
        }

        protected override IEnumerable<string> GetPreconditions(object parameter)
        {
            yield break;
        }
    }
}