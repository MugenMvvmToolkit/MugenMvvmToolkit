using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Wrapping;

namespace MugenMvvm.Infrastructure.Wrapping
{
    public class WrapperManager : IConfigurableWrapperManager
    {
        #region Fields

        private static readonly Func<Type, IReadOnlyMetadataContext, bool> TrueCondition;

        #endregion

        #region Constructors

        static WrapperManager()
        {
            TrueCondition = (type, context) => true;
        }

        [Preserve(Conditional = true)]
        public WrapperManager(IServiceProvider serviceProvider)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            ServiceProvider = serviceProvider;
            Registrations = new Dictionary<Type, List<WrapperRegistration>>(MemberInfoComparer.Instance);
        }

        #endregion

        #region Properties

        protected IServiceProvider ServiceProvider { get; }

        protected Dictionary<Type, List<WrapperRegistration>> Registrations { get; }

        #endregion

        #region Implementation of interfaces

        public bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsAssignableFromUnified(type))
                return true;
            if (metadata == null)
                metadata = Default.MetadataContext;
            if (Registrations.TryGetValue(wrapperType, out var list))
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].Condition(type, metadata))
                        return true;
                }
            }

            return CanWrapInternal(type, wrapperType, metadata);
        }

        public object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(item, nameof(item));
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            if (wrapperType.IsInstanceOfTypeUnified(item))
                return item;

            object? wrapper = null;
            if (Registrations.TryGetValue(wrapperType, out var list))
            {
                var type = item.GetType();
                for (var i = 0; i < list.Count; i++)
                {
                    var registration = list[i];
                    if (registration.Condition(type, metadata))
                        wrapper = WrapInternal(item, registration, metadata);
                }
            }

            if (wrapper == null)
                wrapper = WrapToDefaultWrapper(item, wrapperType, metadata);
            if (wrapper == null)
                throw ExceptionManager.WrapperTypeNotSupported(wrapperType);
            return wrapper;
        }

        public void AddWrapper(Type wrapperType, Type implementation, Func<Type, IReadOnlyMetadataContext, bool>? condition = null,
            Func<object, IReadOnlyMetadataContext, object>? wrapperFactory = null)
        {
            Should.NotBeNull(wrapperType, nameof(wrapperType));
            Should.NotBeNull(implementation, nameof(implementation));
            if (implementation.IsInterfaceUnified() || implementation.IsAbstractUnified())
                throw ExceptionManager.WrapperTypeShouldBeNonAbstract(implementation);
            if (!Registrations.TryGetValue(wrapperType, out var list))
            {
                list = new List<WrapperRegistration>();
                Registrations[wrapperType] = list;
            }

            list.Add(new WrapperRegistration(implementation, condition ?? TrueCondition, wrapperFactory));
        }

        public void RemoveWrapper(Type wrapperType)
        {
            Registrations.Remove(wrapperType);
        }

        #endregion

        #region Methods

        protected virtual object? WrapInternal(object item, WrapperRegistration wrapperRegistration, IReadOnlyMetadataContext metadata)
        {
            if (wrapperRegistration.WrapperFactory != null)
                return wrapperRegistration.WrapperFactory(item, metadata);
            var wrapperType = wrapperRegistration.Type;
            if (wrapperType.IsGenericTypeDefinitionUnified())
                wrapperType = wrapperType.MakeGenericType(item.GetType());

            //            var viewModel = item as IViewModel;//todo check wrapper view model
            //            if (viewModel != null && typeof(IWrapperViewModel).IsAssignableFrom(wrapperType))
            //            {
            //                metadata = metadata.ToNonReadOnly();
            //                if (!metadata.Contains(InitializationConstants.ParentViewModel))
            //                {
            //                    var parentViewModel = viewModel.GetParentViewModel();
            //                    if (parentViewModel != null)
            //                        metadata.AddOrUpdate(InitializationConstants.ParentViewModel, parentViewModel);
            //                }
            //                var vm = (IWrapperViewModel)_viewModelProvider.GetViewModel(wrapperType, metadata);
            //                vm.Wrap(viewModel, metadata);
            //                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.WrapperViewModel, vm);
            //                return vm;
            //            }

            var constructor = wrapperType.GetConstructorsUnified(MemberFlags.InstanceOnly).FirstOrDefault();
            return constructor?.InvokeEx(item);
        }

        protected virtual object? WrapToDefaultWrapper(object item, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        protected virtual bool CanWrapInternal(Type type, Type wrapperType, IReadOnlyMetadataContext metadata)
        {
            return false;
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        protected struct WrapperRegistration
        {
            #region Fields

            // ReSharper disable FieldCanBeMadeReadOnly.Local
            public Func<Type, IReadOnlyMetadataContext, bool> Condition;
            public Func<object, IReadOnlyMetadataContext, object>? WrapperFactory;
            public Type Type;
            // ReSharper restore FieldCanBeMadeReadOnly.Local

            #endregion

            #region Constructors

            public WrapperRegistration(Type type, Func<Type, IReadOnlyMetadataContext, bool> condition, Func<object, IReadOnlyMetadataContext, object>? wrapperFactory)
            {
                Type = type;
                Condition = condition;
                WrapperFactory = wrapperFactory;
            }

            #endregion
        }

        #endregion
    }
}