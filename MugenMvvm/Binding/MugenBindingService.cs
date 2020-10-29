using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Resources;

namespace MugenMvvm.Bindings
{
    public static class MugenBindingService
    {
        #region Properties

        public static IGlobalValueConverter GlobalValueConverter => MugenService.Instance<IGlobalValueConverter>();

        public static IBindingManager BindingManager => MugenService.Instance<IBindingManager>();

        public static IMemberManager MemberManager => MugenService.Instance<IMemberManager>();

        public static IObservationManager ObservationManager => MugenService.Instance<IObservationManager>();

        public static IResourceResolver ResourceResolver => MugenService.Instance<IResourceResolver>();

        public static IExpressionParser Parser => MugenService.Instance<IExpressionParser>();

        public static IExpressionCompiler Compiler => MugenService.Instance<IExpressionCompiler>();

        #endregion
    }
}