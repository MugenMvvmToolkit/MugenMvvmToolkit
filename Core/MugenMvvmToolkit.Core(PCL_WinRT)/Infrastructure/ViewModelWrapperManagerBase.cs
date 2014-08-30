#region Copyright
// ****************************************************************************
// <copyright file="ViewModelWrapperManagerBase.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the wrapper manager that allows to wrap a view model to another view model.
    /// </summary>
    public abstract class ViewModelWrapperManagerBase : IViewModelWrapperManager
    {
        #region Nested types

        /// <summary>
        ///     Represents the wrapper registration.
        /// </summary>
        protected struct WrapperRegistration
        {
            #region Fields

            /// <summary>
            ///     Gets the condition.
            /// </summary>
            public readonly Func<IViewModel, IDataContext, bool> Condition;

            /// <summary>
            ///     Gets the type of wrapper.
            /// </summary>
            public readonly Type Type;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="WrapperRegistration" /> class.
            /// </summary>
            public WrapperRegistration(Type type, Func<IViewModel, IDataContext, bool> condition)
            {
                Type = type;
                Condition = condition;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Func<IViewModel, IDataContext, bool> TrueCondition = (model, context) => true;
        private readonly Dictionary<Type, List<WrapperRegistration>> _registrations;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewModelWrapperManagerBase" /> class.
        /// </summary>
        protected ViewModelWrapperManagerBase([NotNull] IViewModelProvider viewModelProvider)
        {
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            _viewModelProvider = viewModelProvider;
            _registrations = new Dictionary<Type, List<WrapperRegistration>>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="IViewMappingProvider" />.
        /// </summary>
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider; }
        }

        /// <summary>
        ///     Gets the wrappers.
        /// </summary>
        protected IDictionary<Type, List<WrapperRegistration>> Registrations
        {
            get { return _registrations; }
        }

        /// <summary>
        ///     Gets or sets the default type of wrapper, this type is used if mappig was not found.
        /// </summary>
        protected Type DefaultWrapperType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds the wrapper mapping.
        /// </summary>
        protected void AddWrapper<TWrapper, TImplementation>(Func<IViewModel, IDataContext, bool> condition = null)
            where TWrapper : class, IViewModel
            where TImplementation : class, IWrapperViewModel
        {
            AddWrapper<TWrapper>(typeof(TImplementation), condition);
        }

        /// <summary>
        ///     Adds the wrapper mapping.
        /// </summary>
        protected void AddWrapper<TWrapper>([NotNull] Type implementation,
            Func<IViewModel, IDataContext, bool> condition = null)
            where TWrapper : class, IViewModel
        {
            Should.NotBeNull(implementation, "implementation");
#if PCL_WINRT
            TypeInfo typeInfo = implementation.GetTypeInfo();
            if (typeInfo.IsInterface || typeInfo.IsAbstract)
#else
            if (implementation.IsInterface || implementation.IsAbstract)
#endif

                throw ExceptionManager.WrapperTypeShouldBeNonAbstract(implementation);
            List<WrapperRegistration> list;
            if (!_registrations.TryGetValue(typeof(TWrapper), out list))
            {
                list = new List<WrapperRegistration>();
                _registrations[typeof(TWrapper)] = list;
            }
            list.Add(new WrapperRegistration(implementation, condition ?? TrueCondition));
        }

        /// <summary>
        ///     Creates the wrapper view model.
        /// </summary>
        [NotNull]
        protected virtual IViewModel WrapInternal(IViewModel viewModel, Type wrapperType, IDataContext dataContext)
        {
#if PCL_WINRT
            if (wrapperType.GetTypeInfo().IsGenericTypeDefinition)
#else
            if (wrapperType.IsGenericTypeDefinition)
#endif
                wrapperType = wrapperType.MakeGenericType(viewModel.GetType());
            var vm = (IWrapperViewModel)_viewModelProvider.GetViewModel(wrapperType, dataContext ?? DataContext.Empty);
            vm.Wrap(viewModel, dataContext);
            return vm;
        }

        /// <summary>
        ///     Creates the default wrapper if mappig was not found.
        /// </summary>
        [CanBeNull]
        protected virtual IViewModel WrapToDefaultWrapper(IViewModel viewModel, Type wrapperType, IDataContext dataContext)
        {
            if (DefaultWrapperType == null)
                return null;
            return WrapInternal(viewModel, DefaultWrapperType, dataContext);
        }

        #endregion

        #region Implementation of IViewModelWrapperManager

        /// <summary>
        ///     Wraps the specified view-model to a specified type.
        /// </summary>
        /// <param name="viewModel">The specified view-model.</param>
        /// <param name="wrapperType">The specified type to wrap.</param>
        /// <param name="dataContext">The specified <see cref="IDataContext" />, if any.</param>
        /// <returns>
        ///     An instance of <see cref="IViewModel" />.
        /// </returns>
        public IViewModel Wrap(IViewModel viewModel, Type wrapperType, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            Should.NotBeNull(wrapperType, "wrapperType");
            List<WrapperRegistration> list;
            if (_registrations.TryGetValue(wrapperType, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    WrapperRegistration registration = list[i];
                    if (registration.Condition(viewModel, dataContext))
                        return WrapInternal(viewModel, registration.Type, dataContext);
                }
            }
            var wrapper = WrapToDefaultWrapper(viewModel, wrapperType, dataContext);
            if (wrapper == null)
                throw ExceptionManager.WrapperTypeNotSupported(wrapperType);
            return wrapper;
        }

        #endregion
    }
}