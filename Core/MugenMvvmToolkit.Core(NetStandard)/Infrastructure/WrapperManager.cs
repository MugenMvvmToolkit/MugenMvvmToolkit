#region Copyright

// ****************************************************************************
// <copyright file="WrapperManager.cs">
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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    public class WrapperManager : IConfigurableWrapperManager
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        protected struct WrapperRegistration
        {
            #region Fields

            [NotNull]
            public readonly Func<Type, IDataContext, bool> Condition;

            [CanBeNull]
            public readonly Func<object, IDataContext, object> WrapperFactory;

            [NotNull]
            public readonly Type Type;

            #endregion

            #region Constructors

            public WrapperRegistration(Type type, Func<Type, IDataContext, bool> condition, Func<object, IDataContext, object> wrapperFactory)
            {
                Type = type;
                Condition = condition;
                WrapperFactory = wrapperFactory;
            }

            #endregion
        }

        #endregion

        #region Fields

        protected static readonly DataConstant<object> ItemToWrapConstant;
        private static readonly Func<Type, IDataContext, bool> TrueCondition;
        private readonly Dictionary<Type, List<WrapperRegistration>> _registrations;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        static WrapperManager()
        {
            TrueCondition = (model, context) => true;
            ItemToWrapConstant = DataConstant.Create<object>(typeof(WrapperManager), nameof(ItemToWrapConstant), true);
        }

        [Preserve(Conditional = true)]
        public WrapperManager([NotNull] IViewModelProvider viewModelProvider)
        {
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            _viewModelProvider = viewModelProvider;
            _registrations = new Dictionary<Type, List<WrapperRegistration>>();
        }

        #endregion

        #region Properties

        protected IViewModelProvider ViewModelProvider => _viewModelProvider;

        protected IDictionary<Type, List<WrapperRegistration>> Registrations => _registrations;

        #endregion

        #region Methods

        public void AddWrapper(Type wrapperType, Type implementation, Func<Type, IDataContext, bool> condition = null, Func<object, IDataContext, object> wrapperFactory = null)
        {
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(implementation, nameof(implementation));
#if NET_STANDARD
            TypeInfo typeInfo = implementation.GetTypeInfo();
            if (typeInfo.IsInterface || typeInfo.IsAbstract)
#else
            if (implementation.IsInterface || implementation.IsAbstract)
#endif

                throw ExceptionManager.WrapperTypeShouldBeNonAbstract(implementation);
            List<WrapperRegistration> list;
            if (!_registrations.TryGetValue(wrapperType, out list))
            {
                list = new List<WrapperRegistration>();
                _registrations[wrapperType] = list;
            }
            list.Add(new WrapperRegistration(implementation, condition ?? TrueCondition, wrapperFactory));
        }

        public void AddWrapper<TWrapper>(Type implementation,
            Func<Type, IDataContext, bool> condition = null, Func<object, IDataContext, TWrapper> wrapperFactory = null)
            where TWrapper : class
        {
            AddWrapper(typeof(TWrapper), implementation, condition, wrapperFactory);
        }

        public void AddWrapper<TWrapper, TImplementation>(Func<Type, IDataContext, bool> condition = null, Func<object, IDataContext, TWrapper> wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {
            AddWrapper(typeof(TImplementation), condition, wrapperFactory);
        }

        public void Clear<TWrapper>()
        {
            _registrations.Remove(typeof(TWrapper));
        }

        [CanBeNull]
        protected virtual object WrapInternal(object item, WrapperRegistration wrapperRegistration, IDataContext dataContext)
        {
            if (wrapperRegistration.WrapperFactory != null)
                return wrapperRegistration.WrapperFactory(item, dataContext);
            var wrapperType = wrapperRegistration.Type;
#if NET_STANDARD
            if (wrapperType.GetTypeInfo().IsGenericTypeDefinition)
#else
            if (wrapperType.IsGenericTypeDefinition)
#endif
                wrapperType = wrapperType.MakeGenericType(item.GetType());

            var viewModel = item as IViewModel;
            if (viewModel != null && typeof(IWrapperViewModel).IsAssignableFrom(wrapperType))
            {
                dataContext = dataContext.ToNonReadOnly();
                if (!dataContext.Contains(InitializationConstants.ParentViewModel))
                {
                    var parentViewModel = viewModel.GetParentViewModel();
                    if (parentViewModel != null)
                        dataContext.AddOrUpdate(InitializationConstants.ParentViewModel, parentViewModel);
                }
                var vm = (IWrapperViewModel)_viewModelProvider.GetViewModel(wrapperType, dataContext);
                vm.Wrap(viewModel, dataContext);
                return vm;
            }

#if NET_STANDARD
            var constructor = wrapperType.GetTypeInfo()
                .DeclaredConstructors
                .FirstOrDefault(info => !info.IsStatic);
#else
            var constructor = wrapperType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault();
#endif
            return constructor?.InvokeEx(item);
        }

        [CanBeNull]
        protected virtual object WrapToDefaultWrapper(object item, Type wrapperType, IDataContext dataContext)
        {
            return null;
        }

        #endregion

        #region Implementation of IWrapperManager

        public bool CanWrap(Type type, Type wrapperType, IDataContext dataContext)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsAssignableFrom(type))
                return true;
            if (dataContext == null)
                dataContext = DataContext.Empty;
            List<WrapperRegistration> list;
            if (_registrations.TryGetValue(wrapperType, out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Condition(type, dataContext))
                        return true;
                }
            }
            return false;
        }

        public object Wrap(object item, Type wrapperType, IDataContext dataContext)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsInstanceOfType(item))
                return item;
            dataContext = dataContext.ToNonReadOnly();
            object wrapper = null;
            List<WrapperRegistration> list;
            if (_registrations.TryGetValue(wrapperType, out list))
            {
                dataContext.AddOrUpdate(ItemToWrapConstant, item);
                var type = item.GetType();
                for (int i = 0; i < list.Count; i++)
                {
                    WrapperRegistration registration = list[i];
                    if (registration.Condition(type, dataContext))
                        wrapper = WrapInternal(item, registration, dataContext);
                }
                dataContext.Remove(ItemToWrapConstant);
            }
            if (wrapper == null)
                wrapper = WrapToDefaultWrapper(item, wrapperType, dataContext);
            if (wrapper == null)
                throw ExceptionManager.WrapperTypeNotSupported(wrapperType);
            return wrapper;
        }

        #endregion
    }
}
