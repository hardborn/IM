using System.Windows;
using System.Windows.Interactivity;

namespace Nova.NovaWeb.McGo.Windows.Behaviours
{
    public class StylizedBehaviorCollection : FreezableCollection<Behavior>
    {
        protected override Freezable CreateInstanceCore()
        {
            return new StylizedBehaviorCollection();
        }
    }
}