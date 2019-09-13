using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm
{
    public static class MugenService
    {
        #region Properties

        public static IMugenApplication Application => Service<IMugenApplication>.Instance;

        public static ICommandMediatorProvider CommandMediatorProvider => Service<ICommandMediatorProvider>.Instance;

        public static IComponentCollectionProvider ComponentCollectionProvider => Service<IComponentCollectionProvider>.Instance;

        public static IAttachedDictionaryProvider AttachedDictionaryProvider => Service<IAttachedDictionaryProvider>.Instance;

        public static IReflectionDelegateProvider ReflectionDelegateProvider => Service<IReflectionDelegateProvider>.Instance;

        public static ITracer Tracer => Service<ITracer>.Instance;

        public static IWeakReferenceProvider WeakReferenceProvider => Service<IWeakReferenceProvider>.Instance;

        public static IMessenger Messenger => Service<IMessenger>.Instance;

        public static IMetadataContextProvider MetadataContextProvider => Service<IMetadataContextProvider>.Instance;

        public static INavigationDispatcher NavigationDispatcher => Service<INavigationDispatcher>.Instance;

        public static IPresenter Presenter => Service<IPresenter>.Instance;

        public static ISerializer Serializer => Service<ISerializer>.Instance;

        public static IThreadDispatcher ThreadDispatcher => Service<IThreadDispatcher>.Instance;

        public static IValidatorProvider ValidatorProvider => Service<IValidatorProvider>.Instance;

        public static IViewModelManager ViewModelManager => Service<IViewModelManager>.Instance;

        public static IViewManager ViewManager => Service<IViewManager>.Instance;

        public static IWrapperManager WrapperManager => Service<IWrapperManager>.Instance;

        #endregion
    }
}