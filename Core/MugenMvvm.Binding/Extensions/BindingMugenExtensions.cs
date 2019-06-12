using MugenMvvm.Binding.Infrastructure.Observers;
using MugenMvvm.Binding.Interfaces.Observers;

namespace MugenMvvm.Binding.Extensions
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static WeakBindingEventListener ToWeak(this IBindingEventListener listener)
        {
            return new WeakBindingEventListener(listener);
        }

        #endregion
    }
}