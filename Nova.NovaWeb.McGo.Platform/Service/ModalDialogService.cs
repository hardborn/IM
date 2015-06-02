using Nova.NovaWeb.McGo.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.Service
{
    public class ModalDialogService : IModalDialogService
    {
        public void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel, Action<TDialogViewModel> onDialogClose)
        {
            view.DataContext = viewModel;
            view.Owner = App.Current.MainWindow;
            view.ShowInTaskbar = false;
            if (onDialogClose != null)
            {
                view.Closed += (sender, e) => onDialogClose(viewModel);
            }
            view.ShowDialog();
        }

        public void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel)
        {
            this.ShowDialog(view, viewModel, null);
        }
    }
}
