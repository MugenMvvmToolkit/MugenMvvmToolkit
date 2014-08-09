#region Copyright
// ****************************************************************************
// <copyright file="ViewModelProvider.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the provider which creates a view models.
    /// </summary>
    public class ViewModelProvider : IViewModelProvider
    {
        #region Fields

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        private readonly IIocContainer _iocContainer;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelProvider" /> class.
        /// </summary>
        /// <param name="iocContainer">The specified <see cref="IIocContainer" /> value.</param>
        /// <param name="bindIocContainer">
        ///     If <c>true</c> it indicates that provider should bind IocContainer to self, when creates the
        ///     view-model.
        /// </param>
        /// <param name="useParentIocContainer">The value that is responsible to initialize the IocContainer using the IocContainer of parent view model.</param>
        public ViewModelProvider([NotNull] IIocContainer iocContainer, bool bindIocContainer = false, bool useParentIocContainer = false)
        {
            Should.NotBeNull(iocContainer, "iocContainer");
            _iocContainer = iocContainer;
            BindIocContainer = bindIocContainer;
            UseParentIocContainer = useParentIocContainer;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        protected IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        /// <summary>
        ///     Gets or sets value, if <c>true</c> it indicates that provider should bind IocContainer to self, when creates the
        ///     view-model.
        /// </summary>
        public bool BindIocContainer { get; set; }

        /// <summary>
        ///     Gets the value that is responsible to initialize the IocContainer using the IocContainer of parent view model.
        /// </summary>
        public bool UseParentIocContainer { get; set; }

        #endregion

        #region Implementation of IViewModelProvider

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="getViewModel">The specified delegate to create view model.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        public virtual IViewModel GetViewModel(GetViewModelDelegate<IViewModel> getViewModel, IDataContext dataContext)
        {
            Should.NotBeNull(getViewModel, "getViewModel");
            Should.NotBeNull(dataContext, "dataContext");
            dataContext = dataContext.ToNonReadOnly();
            IIocContainer iocContainer = CreateViewModelIocContainer(dataContext);
            dataContext.Remove(ActivationConstants.IocContainer);
            dataContext.Add(ActivationConstants.IocContainer, iocContainer);
            IViewModel viewModel = getViewModel(iocContainer);
            if (!viewModel.IsInitialized)
                viewModel.InitializeViewModel(dataContext);
            MergeParameters(viewModel, dataContext);
            return viewModel;
        }

        /// <summary>
        ///     Creates an instance of the specified view model.
        /// </summary>
        /// <param name="viewModelType">The type of view model.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        public virtual IViewModel GetViewModel(Type viewModelType, IDataContext dataContext)
        {
            Should.NotBeNull(viewModelType, "viewModelType");
            Should.NotBeNull(dataContext, "dataContext");
            var viewModelBindingName = dataContext.GetData(ActivationConstants.ViewModelBindingName);
            var parameters = dataContext.GetData(ActivationConstants.IocParameters);
            return GetViewModel(adapter => (IViewModel)adapter.Get(viewModelType, viewModelBindingName, parameters),
                dataContext);
        }

        /// <summary>
        ///     Initializes the specified <see cref="IViewModel" />, use this method if you have created an <see cref="IViewModel" />
        ///     without using the GetViewModel method.
        /// </summary>
        /// <param name="viewModel">
        ///     The specified <see cref="IViewModel" />.
        /// </param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />.</param>
        public virtual void InitializeViewModel(IViewModel viewModel, IDataContext dataContext = null)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (viewModel.IsInitialized)
                throw ExceptionManager.ObjectInitialized("ViewModel", viewModel);
            dataContext = dataContext.ToNonReadOnly();
            var iocContainer = CreateViewModelIocContainer(dataContext);
            dataContext.Remove(ActivationConstants.IocContainer);
            dataContext.Add(ActivationConstants.IocContainer, iocContainer);
            viewModel.InitializeViewModel(dataContext);
            MergeParameters(viewModel, dataContext);            
        }

        #endregion

        #region Methods

        private static void MergeParameters(IViewModel vm, IDataContext ctx)
        {
            var data = ctx.GetData(ActivationConstants.ViewName);
            if (!string.IsNullOrEmpty(data) && !vm.Settings.Metadata.Contains(ActivationConstants.ViewName))
                vm.Settings.Metadata.Add(ActivationConstants.ViewName, data);
        }

        /// <summary>
        ///     Creates an instance of <see cref="IocContainer"/> using activation policy and the parent view model if any.
        /// </summary>
        protected virtual IIocContainer CreateViewModelIocContainer([NotNull] IDataContext dataContext)
        {
            var container = dataContext.GetData(ActivationConstants.ExplicitIocContainer);
            if (container == null)
            {
                bool useParent;
                if (!dataContext.TryGetData(ActivationConstants.UseParentIocContainer, out useParent))
                    useParent = UseParentIocContainer;
                var parentViewModel = dataContext.GetData(ActivationConstants.ParentViewModel);
                if (useParent && parentViewModel != null)
                    container = parentViewModel.GetIocContainer(false).CreateChild();
                else
                    container = _iocContainer.GetRoot().CreateChild();
            }
            if (BindIocContainer)
                container.BindToConstant(container);
            return container;
        }

        #endregion
    }
}