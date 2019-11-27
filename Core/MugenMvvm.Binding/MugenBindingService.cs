using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Resources;

namespace MugenMvvm.Binding
{
    public static class MugenBindingService
    {
        #region Properties

        public static IGlobalValueConverter GlobalValueConverter => MugenService.Instance<IGlobalValueConverter>();

        public static IBindingManager BindingManager => MugenService.Instance<IBindingManager>();

        public static IMemberProvider MemberProvider => MugenService.Instance<IMemberProvider>();

        public static IObserverProvider ObserverProvider => MugenService.Instance<IObserverProvider>();

        public static IResourceResolver ResourceResolver => MugenService.Instance<IResourceResolver>();

        public static IExpressionParser Parser => MugenService.Instance<IExpressionParser>();

        public static IExpressionCompiler Compiler => MugenService.Instance<IExpressionCompiler>();

        #endregion
    }
}