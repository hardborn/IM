using Nova.NovaWeb.McGo.Platform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.Service
{
    public interface IModalDialogService
    {
        void ShowDialog<TViewModel>(IModalWindow view, TViewModel viewModel, Action<TViewModel> onDialogClose);

        void ShowDialog<TDialogViewModel>(IModalWindow view, TDialogViewModel viewModel);
    }
}
