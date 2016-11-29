#region Copyright

// ****************************************************************************
// <copyright file="ApplicationStateManager.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.UWP.Infrastructure.Presenters;
using MugenMvvmToolkit.UWP.Interfaces;

namespace MugenMvvmToolkit.UWP.Infrastructure
{
    public class ApplicationStateManager : IApplicationStateManager
    {
        #region Nested types

        [DataContract]
        public sealed class LazySerializableContainer
        {
            #region Fields

            [IgnoreDataMember]
            private readonly ISerializer _serializer;

            [IgnoreDataMember]
            private byte[] _bytes;

            [IgnoreDataMember]
            private IDataContext _context;

            #endregion

            #region Constructors

            //Only for serialization
            public LazySerializableContainer() { }

            public LazySerializableContainer(ISerializer serializer, IDataContext context)
            {
                _serializer = serializer;
                _context = context;
            }

            #endregion

            #region Properties

            [DataMember(Name = "b")]
            internal byte[] Bytes
            {
                get
                {
                    if (_bytes == null && _context.Count != 0)
                        _bytes = _serializer.Serialize(_context).ToArray();
                    return _bytes;
                }
                set { _bytes = value; }
            }

            #endregion

            #region Methods

            public IDataContext GetContext(ISerializer serializer)
            {
                if (_context == null)
                {
                    if (_bytes == null)
                        return DataContext.Empty;
                    using (var ms = new MemoryStream(_bytes))
                        _context = (IDataContext)serializer.Deserialize(ms);
                }
                return _context;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string VmStateKey = "@`vmstate";
        private const string VmTypeKey = "@`vmtype";

        private static readonly Type[] KnownTypesStatic;

        private readonly ISerializer _serializer;
        private readonly IViewManager _viewManager;
        private readonly IViewModelPresenter _viewModelPresenter;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        static ApplicationStateManager()
        {
            KnownTypesStatic = new[] { typeof(LazySerializableContainer), typeof(DataContext) };
        }

        [Preserve(Conditional = true)]
        public ApplicationStateManager([NotNull] ISerializer serializer, [NotNull] IViewModelProvider viewModelProvider,
            [NotNull] IViewManager viewManager, [NotNull] IViewModelPresenter viewModelPresenter)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(viewModelPresenter, nameof(viewModelPresenter));
            _serializer = serializer;
            _viewModelProvider = viewModelProvider;
            _viewManager = viewManager;
            _viewModelPresenter = viewModelPresenter;
        }

        #endregion

        #region Properties

        protected ISerializer Serializer => _serializer;

        protected IViewModelProvider ViewModelProvider => _viewModelProvider;

        protected IViewManager ViewManager => _viewManager;

        protected IViewModelPresenter ViewModelPresenter => _viewModelPresenter;

        #endregion

        #region Implementation of IApplicationStateManager

        public virtual IList<Type> KnownTypes => KnownTypesStatic;

        public void OnSaveState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, nameof(element));
            Should.NotBeNull(state, nameof(state));
            var viewModel = element.DataContext as IViewModel;
            if (viewModel != null)
            {
                state[VmTypeKey] = viewModel.GetType().AssemblyQualifiedName;
                PreserveViewModel(viewModel, element, state, args, context ?? DataContext.Empty);
            }
        }

        public void OnLoadState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, nameof(element));
            Should.NotBeNull(state, nameof(state));
            object value;
            if (!state.TryGetValue(VmTypeKey, out value))
                return;
            state.Remove(VmTypeKey);
            object dataContext = element.DataContext;
            Type vmType = Type.GetType(value as string, false);
            if (vmType == null || (dataContext != null && dataContext.GetType().Equals(vmType)))
                return;

            if (context == null)
                context = DataContext.Empty;
            var viewModelState = RestoreViewModelState(element, state, args, context);
            //The navigation is already handled.
            var eventArgs = args as NavigationEventArgs;
            if (eventArgs != null && eventArgs.GetHandled())
            {
                eventArgs.SetHandled(false);
                PlatformExtensions.SetViewModelState(eventArgs.Content, viewModelState);
            }
            else
                RestoreViewModel(vmType, viewModelState, element, state, args, context);
        }

        #endregion

        #region Methods

        [NotNull]
        protected virtual IDataContext RestoreViewModelState([NotNull] FrameworkElement element, [NotNull] IDictionary<string, object> state,
             [NotNull] object args, [NotNull] IDataContext context)
        {
            object value;
            if (state.TryGetValue(VmStateKey, out value))
                return ((LazySerializableContainer)value).GetContext(_serializer);
            return DataContext.Empty;
        }

        protected virtual void RestoreViewModel([NotNull] Type viewModelType, [NotNull] IDataContext viewModelState, [NotNull] FrameworkElement element,
            [NotNull] IDictionary<string, object> state, [NotNull] object args, [NotNull] IDataContext context)
        {
            context = context.ToNonReadOnly();
            context.AddOrUpdate(InitializationConstants.ViewModelType, viewModelType);

#if WINDOWS_UWP
            context.Add(DynamicViewModelWindowPresenter.RestoredViewConstant, element);
            context.Add(DynamicViewModelWindowPresenter.IsOpenViewConstant, true);
#endif
            IViewModel viewModel = _viewModelProvider.RestoreViewModel(viewModelState, context, true);
            _viewManager.InitializeViewAsync(viewModel, element, context).WithTaskExceptionHandler(this);
            _viewModelPresenter.Restore(viewModel, context);
        }

        protected virtual void PreserveViewModel([NotNull] IViewModel viewModel, [NotNull] FrameworkElement element,
             [NotNull] IDictionary<string, object> state, [NotNull] object args, [NotNull] IDataContext context)
        {
            state[VmStateKey] = new LazySerializableContainer(_serializer, _viewModelProvider.PreserveViewModel(viewModel, context));
        }

        #endregion
    }
}
