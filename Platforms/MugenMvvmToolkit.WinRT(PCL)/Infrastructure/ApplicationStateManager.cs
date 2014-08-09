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
using MugenMvvmToolkit.Interfaces.Navigation;
#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Navigation;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
#endif
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the application state manager.
    /// </summary>
    public class ApplicationStateManager : IApplicationStateManager
    {
        #region Nested types

        /// <summary>
        /// Represents the seralizable container for <see cref="IDataContext"/>.
        /// </summary>
        [DataContract]
        internal sealed class LazySerializableContainer
        {
            #region Fields

            [IgnoreDataMember]
            private readonly ISerializer _serializer;

            [IgnoreDataMember]
            private IDataContext _context;

            [IgnoreDataMember]
            private byte[] _bytes;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="LazySerializableContainer" /> class.
            /// </summary>
            public LazySerializableContainer(ISerializer serializer, IDataContext context)
            {
                _serializer = serializer;
                _context = context;
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

            #endregion

            #region Methods


            /// <summary>
            ///  Deserializes data using stream.
            /// </summary>
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

        private const string VmStateKey = "t@3`3vmstate";

        private static readonly DataConstant<object> ActiveConstant;
        private static readonly Type[] KnownTypesStatic;

        private readonly ISerializer _serializer;

        #endregion

        #region Constructors

        static ApplicationStateManager()
        {
            ActiveConstant = DataConstant.Create(() => ActiveConstant, false);
            KnownTypesStatic = new[] { typeof(LazySerializableContainer) };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationStateManager" /> class.
        /// </summary>
        public ApplicationStateManager([NotNull] ISerializer serializer)
        {
            Should.NotBeNull(serializer, "serializer");
            _serializer = serializer;
        }

        #endregion

        #region Implementation of IApplicationStateManager

        /// <summary>
        /// Gets the collection of known types.
        /// </summary>
        public IList<Type> KnownTypes
        {
            get { return KnownTypesStatic; }
        }

        /// <summary>
        ///     Occurs on save element state.
        /// </summary>
        public void OnSaveState(FrameworkElement element, IDictionary<string, object> state, object args, IDataContext context = null)
        {
            Should.NotBeNull(element, "element");
            Should.NotBeNull(state, "state");
            SaveState(element.DataContext as IViewModel, state);
        }

        /// <summary>
        ///     Occurs on load element state.
        /// </summary>
        public void OnLoadState(FrameworkElement element, IDictionary<string, object> state, object args, IDataContext context = null)
        {
            Should.NotBeNull(element, "element");
            Should.NotBeNull(state, "state");
            var viewModel = element.DataContext as IViewModel;
            if (viewModel == null)
            {
                var eventArgs = args as NavigationEventArgs;
                if (eventArgs != null)
                {
                    INavigationService service;
                    if (ServiceProvider.IocContainer.TryGet(out service))
                        service.OnNavigated(eventArgs);
                    viewModel = element.DataContext as IViewModel;
                }
            }
            RestoreState(viewModel, state);
        }

        #endregion

        #region Methods

        private void RestoreState(IViewModel viewModel, IDictionary<string, object> dictionary)
        {
            if (viewModel == null || viewModel.Settings.Metadata.Contains(ActiveConstant))
                return;
            object value;
            if (!dictionary.TryGetValue(VmStateKey, out value))
                return;
            dictionary.Remove(VmStateKey);
            var container = (LazySerializableContainer)value;
            if (container == null)
                return;
            var state = container.GetContext(_serializer);
            if (state.Count == 0)
                return;
            viewModel.Settings.State.Update(state);
            var hasState = viewModel as IHasState;
            if (hasState != null)
                hasState.LoadState(viewModel.Settings.State);
        }

        private void SaveState(IViewModel viewModel, IDictionary<string, object> dictionary)
        {
            if (viewModel == null)
                return;
            viewModel.Settings.Metadata.AddOrUpdate(ActiveConstant, null);
            IDataContext state = viewModel.Settings.State;
            var hasState = viewModel as IHasState;
            if (hasState != null)
                hasState.SaveState(state);
            dictionary[VmStateKey] = new LazySerializableContainer(_serializer, state);
        }

        #endregion
    }
}