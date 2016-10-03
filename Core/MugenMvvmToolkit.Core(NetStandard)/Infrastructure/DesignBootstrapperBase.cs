using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class DesignBootstrapperBase
    {
        #region Fields

        private readonly Dictionary<string, IViewModel> _viewModelMapping;

        #endregion

        #region Constructors

        protected DesignBootstrapperBase()
        {
            if (ServiceProvider.IsDesignMode)
            {
                _viewModelMapping = new Dictionary<string, IViewModel>();
                InitializeInternal();
            }
        }

        #endregion

        #region Methods

        protected virtual T GetOrAddViewModel<T>(Func<IViewModelProvider, T> getViewModel, [CallerMemberName] string property = "")
            where T : IViewModel
        {
            if (!ServiceProvider.IsDesignMode)
                return default(T);
            IViewModel value;
            if (!_viewModelMapping.TryGetValue(property, out value))
            {
                value = getViewModel(ServiceProvider.ViewModelProvider);
                _viewModelMapping[property] = value;
            }
            return (T)value;
        }

        protected virtual void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(GetPlatformInfo(), iocContainer, GetAssemblies(), GetInitializationContext());
        }

        [NotNull]
        protected virtual IDataContext GetInitializationContext()
        {
            return DataContext.Empty;
        }

        [NotNull]
        protected abstract IList<Assembly> GetAssemblies();

        [NotNull]
        protected abstract PlatformInfo GetPlatformInfo();

        [NotNull]
        protected abstract IMvvmApplication CreateApplication();

        [NotNull]
        protected abstract IIocContainer CreateIocContainer();

        #endregion
    }
}