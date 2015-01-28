#region Copyright

// ****************************************************************************
// <copyright file="WrapperManager.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that allows to wrap an object to another object.
    /// </summary>
    public class WrapperManager : IWrapperManager
    {
        #region Nested types

        /// <summary>
        ///     Represents the wrapper registration.
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        protected struct WrapperRegistration
        {
            #region Fields

            /// <summary>
            ///     Gets the condition.
            /// </summary>
            [NotNull]
            public readonly Func<Type, IDataContext, bool> Condition;

            /// <summary>
            ///     Gets the factory delegate.
            /// </summary>
            [CanBeNull]
            public readonly Func<object, IDataContext, object> WrapperFactory;

            /// <summary>
            ///     Gets the type of wrapper.
            /// </summary>
            [NotNull]
            public readonly Type Type;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="WrapperRegistration" /> class.
            /// </summary>
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

        private static readonly Func<Type, IDataContext, bool> TrueCondition;
        private readonly Dictionary<Type, List<WrapperRegistration>> _registrations;
        private readonly IViewModelProvider _viewModelProvider;

        #endregion

        #region Constructors

        static WrapperManager()
        {
            TrueCondition = (model, context) => true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WrapperManager" /> class.
        /// </summary>
        public WrapperManager([NotNull] IViewModelProvider viewModelProvider)
        {
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            _viewModelProvider = viewModelProvider;
            _registrations = new Dictionary<Type, List<WrapperRegistration>>();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="IViewModelProvider" />.
        /// </summary>
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider; }
        }

        /// <summary>
        ///     Gets the wrapper mappings.
        /// </summary>
        protected IDictionary<Type, List<WrapperRegistration>> Registrations
        {
            get { return _registrations; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds the wrapper mapping.
        /// </summary>
        public void AddWrapper<TWrapper, TImplementation>(Func<Type, IDataContext, bool> condition = null, Func<object, IDataContext, TWrapper> wrapperFactory = null)
            where TWrapper : class
            where TImplementation : class, TWrapper
        {
            AddWrapper(typeof(TImplementation), condition, wrapperFactory);
        }

        /// <summary>
        ///     Adds the wrapper mapping.
        /// </summary>
        public void AddWrapper<TWrapper>([NotNull] Type implementation,
            Func<Type, IDataContext, bool> condition = null, Func<object, IDataContext, TWrapper> wrapperFactory = null)
            where TWrapper : class
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
#if PCL_Silverlight
            list.Add(new WrapperRegistration(implementation, condition ?? TrueCondition,
                wrapperFactory == null
                    ? null
                    : new Func<object, IDataContext, object>((o, context) => wrapperFactory(o, context))));
#else
            list.Add(new WrapperRegistration(implementation, condition ?? TrueCondition, wrapperFactory));
#endif
        }

        /// <summary>
        ///     Clears the wrapper types.
        /// </summary>
        public void Clear<TWrapper>()
        {
            _registrations.Remove(typeof(TWrapper));
        }

        /// <summary>
        ///     Creates the wrapper view model.
        /// </summary>
        [CanBeNull]
        protected virtual object WrapInternal(object item, WrapperRegistration wrapperRegistration, IDataContext dataContext)
        {
            if (wrapperRegistration.WrapperFactory != null)
                return wrapperRegistration.WrapperFactory(item, dataContext);
            var wrapperType = wrapperRegistration.Type;
#if PCL_WINRT
            if (wrapperType.GetTypeInfo().IsGenericTypeDefinition)
#else
            if (wrapperType.IsGenericTypeDefinition)
#endif
                wrapperType = wrapperType.MakeGenericType(item.GetType());

            var viewModel = item as IViewModel;
            if (viewModel != null && typeof(IWrapperViewModel).IsAssignableFrom(wrapperType))
            {
                var vm = (IWrapperViewModel)_viewModelProvider.GetViewModel(wrapperType, dataContext ?? DataContext.Empty);
                vm.Wrap(viewModel, dataContext);
                return vm;
            }

#if PCL_WINRT
            var constructor = wrapperType.GetTypeInfo()
                .DeclaredConstructors
                .FirstOrDefault(info => !info.IsStatic);
#else
            var constructor = wrapperType
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault();
#endif
            if (constructor == null)
                return null;
            return constructor.InvokeEx(item);
        }

        /// <summary>
        ///     Creates the default wrapper if mapping was not found.
        /// </summary>
        [CanBeNull]
        protected virtual object WrapToDefaultWrapper(object item, Type wrapperType, IDataContext dataContext)
        {
            return null;
        }

        #endregion

        #region Implementation of IWrapperManager

        /// <summary>
        ///     Determines whether the specified view can be wrapped to wrapper type.
        /// </summary>
        public bool CanWrap(Type type, Type wrapperType, IDataContext dataContext)
        {
            Should.NotBeNull(type, "type");
            Should.NotBeNull(wrapperType, "wrapperType");
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

        /// <summary>
        ///     Wraps the specified view object to the wrapper type.
        /// </summary>
        public object Wrap(object item, Type wrapperType, IDataContext dataContext)
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(wrapperType, "wrapperType");
            if (wrapperType.IsInstanceOfType(item))
                return item;
            if (dataContext == null)
                dataContext = DataContext.Empty;
            object wrapper = null;
            List<WrapperRegistration> list;
            if (_registrations.TryGetValue(wrapperType, out list))
            {
                var type = item.GetType();
                for (int i = 0; i < list.Count; i++)
                {
                    WrapperRegistration registration = list[i];
                    if (registration.Condition(type, dataContext))
                        wrapper = WrapInternal(item, registration, dataContext);
                }
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