using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Resources;

namespace MugenMvvm.Binding
{
    public static class MugenBindingService
    {
        #region Properties

        public static IGlobalValueConverter GlobalValueConverter => Service<IGlobalValueConverter>.Instance;

        public static IBindingManager BindingManager => Service<IBindingManager>.Instance;

        public static IMemberProvider MemberProvider => Service<IMemberProvider>.Instance;

        public static IObserverProvider ObserverProvider => Service<IObserverProvider>.Instance;

        public static IResourceResolver ResourceResolver => Service<IResourceResolver>.Instance;

        #endregion
    }
}