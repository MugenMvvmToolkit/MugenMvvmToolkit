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
using System.IO;
using Android.App;
using Android.OS;
using Android.Support.V4.App;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ApplicationStateManager : IApplicationStateManager
    {
        #region Fields

        private static readonly DataConstant<object> ActiveConstant = DataConstant.Create(() => ActiveConstant, false);
        private const string VmStateActivityBundleKey = "~@#actvmstate";
#if !API8
        private const string VmStateFragmentBundleKey = "~@#fragvmstate";
#endif
        private readonly ISerializer _serializer;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationStateManager" /> class.
        /// </summary>
        public ApplicationStateManager([NotNull] ISerializer serializer)
        {
            Should.NotBeNull(serializer, "serializer");
            _serializer = serializer;
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

        #endregion

        #region Implementation of IApplicationStateManager

        /// <summary>
        ///     Raised as part of the activity lifecycle when an activity is going into the background.
        /// </summary>
        public virtual void OnSaveInstanceStateActivity(Activity activity, Bundle bundle, IDataContext context = null)
        {
            if (bundle == null)
                return;
            var viewModel = BindingProvider.Instance
                .ContextManager
                .GetBindingContext(activity)
                .DataContext as IViewModel;
            SaveState(viewModel, bundle, VmStateActivityBundleKey);
        }

        /// <summary>
        ///     Called when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        public virtual void OnCreateActivity(Activity activity, Bundle bundle, IDataContext context = null)
        {
            if (bundle == null)
                return;
            var viewModel = BindingProvider.Instance
                .ContextManager
                .GetBindingContext(activity)
                .DataContext as IViewModel;
            RestoreState(viewModel, bundle, VmStateActivityBundleKey);
        }

#if !API8
        /// <summary>
        ///     Raised as part of the activity lifecycle when an activity is going into the background.
        /// </summary>
        public virtual void OnSaveInstanceStateFragment(Fragment fragment, Bundle bundle, IDataContext context = null)
        {
            if (bundle == null)
                return;
            var viewModel = BindingProvider.Instance
                .ContextManager
                .GetBindingContext(fragment)
                .DataContext as IViewModel;
            SaveState(viewModel, bundle, VmStateFragmentBundleKey);
        }

        /// <summary>
        ///     Called when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        public virtual void OnCreateFragment(Fragment fragment, Bundle bundle, IDataContext context = null)
        {
            if (bundle == null)
                return;
            var viewModel = BindingProvider.Instance
                .ContextManager
                .GetBindingContext(fragment)
                .DataContext as IViewModel;
            RestoreState(viewModel, bundle, VmStateFragmentBundleKey);
        }
#endif
        #endregion

        #region Methods

        private void RestoreState(IViewModel viewModel, Bundle bundle, string stateKey)
        {
            if (viewModel == null || viewModel.Settings.Metadata.Contains(ActiveConstant))
                return;
            var bytes = bundle.GetByteArray(stateKey);
            bundle.Remove(stateKey);
            if (bytes == null)
                return;
            IDataContext state;
            using (var ms = new MemoryStream(bytes))
                state = (IDataContext)_serializer.Deserialize(ms);
            viewModel.Settings.State.Update(state);
            var hasState = viewModel as IHasState;
            if (hasState != null)
                hasState.LoadState(viewModel.Settings.State);
        }

        private void SaveState(IViewModel viewModel, Bundle bundle, string stateKey)
        {
            if (viewModel == null)
                return;
            viewModel.Settings.Metadata.AddOrUpdate(ActiveConstant, null);
            IDataContext state = viewModel.Settings.State;
            var hasState = viewModel as IHasState;
            if (hasState != null)
                hasState.SaveState(state);
            if (state.Count == 0)
                bundle.Remove(stateKey);
            else
            {
                using (var stream = Serializer.Serialize(state))
                    bundle.PutByteArray(stateKey, stream.ToArray());
            }
        }

        #endregion
    }
}