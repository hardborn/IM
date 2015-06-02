using Nova.NovaWeb.McGo.Windows;
using System.Windows.Interactivity;

namespace Nova.NovaWeb.McGo.Windows.Behaviours
{
    public class WindowsSettingBehaviour : Behavior<NebulaWindow>
    {
        protected override void OnAttached()
        {
            WindowSettings.SetSave(AssociatedObject, AssociatedObject.SaveWindowPosition);
        }
    }
}