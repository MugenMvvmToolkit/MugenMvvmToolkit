#region Copyright
// ****************************************************************************
// <copyright file="ApplicationStateManager.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Navigation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
#endif

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the application state manager.
    /// </summary>
    public class ApplicationStateManager : IApplicationStateManager
    {
        #region Nested types

        [DataContract]
        internal sealed class LazySerializableContainer
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

            public LazySerializableContainer(ISerializer serializer, IDataContext context, IViewModel viewModel)
            {
                _serializer = serializer;
                _context = context;
                ViewModelType = viewModel.GetType().AssemblyQualifiedName;
            }

            #endregion

            #region Properties

            [DataMember]
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

            [DataMember]
            public string ViewModelType { get; set; }

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

        private static readonly Type[] KnownTypesStatic;

        private readonly ISerializer _serializer;
        private readonly IViewManager _viewManager;
        private readonly IViewModelPresenter _viewModelPresenter;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        static ApplicationStateManager()
        {
            KnownTypesStatic = new[] { typeof(LazySerializableContainer) };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationStateManager" /> class.
        /// </summary>
        public ApplicationStateManager([NotNull] ISerializer serializer, [NotNull] IViewModelProvider viewModelProvider,
            [NotNull] IViewManager viewManager, [NotNull] IViewModelPresenter viewModelPresenter)
        {
            Should.NotBeNull(serializer, "serializer");
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(viewManager, "viewManager");
            Should.NotBeNull(viewModelPresenter, "viewModelPresenter");
            _serializer = serializer;
            _viewModelProvider = viewModelProvider;
            _viewManager = viewManager;
            _viewModelPresenter = viewModelPresenter;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="ISerializer" />.
        /// </summary>
        protected ISerializer Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelProvider" />.
        /// </summary>
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewManager" />.
        /// </summary>
        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelPresenter" />.
        /// </summary>
        protected IViewModelPresenter ViewModelPresenter
        {
            get { return _viewModelPresenter; }
        }

        #endregion

        #region Implementation of IApplicationStateManager

        /// <summary>
        ///     Gets the collection of known types.
        /// </summary>
        public virtual IList<Type> KnownTypes
        {
            get { return KnownTypesStatic; }
        }

        /// <summary>
        ///     Occurs on save element state.
        /// </summary>
        public virtual void OnSaveState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, "element");
            Should.NotBeNull(state, "state");
            var viewModel = element.DataContext as IViewModel;
            if (viewModel != null)
                state[VmStateKey] = new LazySerializableContainer(_serializer,
                    _viewModelProvider.PreserveViewModel(viewModel, context ?? DataContext.Empty), viewModel);
        }

        /// <summary>
        ///     Occurs on load element state.
        /// </summary>
        public virtual void OnLoadState(FrameworkElement element, IDictionary<string, object> state, object args,
            IDataContext context = null)
        {
            Should.NotBeNull(element, "element");
            Should.NotBeNull(state, "state");
            object value;
            if (!state.TryGetValue(VmStateKey, out value))
                return;
            state.Remove(VmStateKey);
            var container = (LazySerializableContainer)value;
            if (container == null)
                return;
            object dataContext = element.DataContext;
            Type vmType = Type.GetType(container.ViewModelType, false);
            if (vmType == null || (dataContext != null && dataContext.GetType().Equals(vmType)))
                return;
            context = context.ToNonReadOnly();
            context.AddOrUpdate(InitializationConstants.ViewModelType, vmType);

            //The navigation is already handled.
            var eventArgs = args as NavigationEventArgs;
            if (eventArgs != null && eventArgs.GetHandled())
            {
                eventArgs.SetHandled(false);
                PlatformExtensions.SetViewModelState(eventArgs.Content, container.GetContext(_serializer));
            }
            else
            {
                IViewModel viewModel = _viewModelProvider.RestoreViewModel(container.GetContext(_serializer), context, false);
                _viewManager.InitializeViewAsync(viewModel, element).WithTaskExceptionHandler(this);
                _viewModelPresenter.Restore(viewModel, context);
            }
        }

        #endregion
    }
}