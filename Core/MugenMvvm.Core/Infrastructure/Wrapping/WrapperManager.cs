using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Wrapping
{
//    public class WrapperManager : IConfigurableWrapperManager
//    {
//        
//
//        [StructLayout(LayoutKind.Auto)]
//        protected struct WrapperRegistration
//        {
//            #region Fields
//
//            public readonly Func<Type, IReadOnlyMetadataContext, bool> Condition;
//
//            public readonly Func<object, IReadOnlyMetadataContext, object>? WrapperFactory;
//
//            public readonly Type Type;
//
//            #endregion
//
//            #region Constructors
//
//            public WrapperRegistration(Type type, Func<Type, IReadOnlyMetadataContext, bool> condition, Func<object, IReadOnlyMetadataContext, object>? wrapperFactory)
//            {
//                Type = type;
//                Condition = condition;
//                WrapperFactory = wrapperFactory;
//            }
//
//            #endregion
//        }
//
//        
//
//        
//
//        protected static readonly DataConstant<object> ItemToWrapConstant;
//        private static readonly Func<Type, IReadOnlyMetadataContext, bool> TrueCondition;
//        private readonly Dictionary<Type, List<WrapperRegistration>> _registrations;
//        private readonly IViewModelProvider _viewModelProvider;
//
//       
//
//       
//
//        static WrapperManager()
//        {
//            TrueCondition = (model, context) => true;
//            ItemToWrapConstant = DataConstant.Create<object>(typeof(WrapperManager), nameof(ItemToWrapConstant), true);
//        }
//
//        [Preserve(Conditional = true)]
//        public WrapperManager(IViewModelProvider viewModelProvider)
//        {
//            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
//            _viewModelProvider = viewModelProvider;
//            _registrations = new Dictionary<Type, List<WrapperRegistration>>();
//        }
//
//        
//
//        
//
//        protected IViewModelProvider ViewModelProvider => _viewModelProvider;
//
//        protected IDictionary<Type, List<WrapperRegistration>> Registrations => _registrations;
//
//        
//
//        
//
//        public void AddWrapper(Type wrapperType, Type implementation, Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, object>? wrapperFactory = null)
//        {
//            Should.NotBeNull(wrapperType, nameof(wrapperType));
//            Should.NotBeNull(implementation, nameof(implementation));
//            if (implementation.IsInterfaceUnified() || implementation.IsAbstractUnified())
//                throw ExceptionManager.WrapperTypeShouldBeNonAbstract(implementation);
//            if (!_registrations.TryGetValue(wrapperType, out var list))
//            {
//                list = new List<WrapperRegistration>();
//                _registrations[wrapperType] = list;
//            }
//            list.Add(new WrapperRegistration(implementation, condition ?? TrueCondition, wrapperFactory));
//        }
//
//        public void AddWrapper<TWrapper>(Type implementation,
//            Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
//            where TWrapper : class
//        {
//            AddWrapper(typeof(TWrapper), implementation, condition, wrapperFactory);
//        }
//
//        public void AddWrapper<TWrapper, TImplementation>(Func<Type, IReadOnlyMetadataContext, bool>? condition = null, Func<object, IReadOnlyMetadataContext, TWrapper>? wrapperFactory = null)
//            where TWrapper : class
//            where TImplementation : class, TWrapper
//        {
//            AddWrapper(typeof(TImplementation), condition, wrapperFactory);
//        }
//
//        public void Clear<TWrapper>()
//        {
//            _registrations.Remove(typeof(TWrapper));
//        }
//
//        protected virtual object? WrapInternal(object item, WrapperRegistration wrapperRegistration, IReadOnlyMetadataContext context)
//        {
//            if (wrapperRegistration.WrapperFactory != null)
//                return wrapperRegistration.WrapperFactory(item, context);
//            var wrapperType = wrapperRegistration.Type;
//            if (wrapperType.IsGenericTypeDefinitionUnified())
//                wrapperType = wrapperType.MakeGenericType(item.GetType());
//
//            var viewModel = item as IViewModel;
//            if (viewModel != null && typeof(IWrapperViewModel).IsAssignableFrom(wrapperType))
//            {
//                context = context.ToNonReadOnly();
//                if (!context.Contains(InitializationConstants.ParentViewModel))
//                {
//                    var parentViewModel = viewModel.GetParentViewModel();
//                    if (parentViewModel != null)
//                        context.AddOrUpdate(InitializationConstants.ParentViewModel, parentViewModel);
//                }
//                var vm = (IWrapperViewModel)_viewModelProvider.GetViewModel(wrapperType, context);
//                vm.Wrap(viewModel, context);
//                viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.WrapperViewModel, vm);
//                return vm;
//            }
//
//            var constructor = wrapperType.GetConstructorsUnified(MemberFlags.InstanceOnly).FirstOrDefault();
//            return constructor?.InvokeEx(item);
//        }
//
//        protected virtual object? WrapToDefaultWrapper(object item, Type wrapperType, IReadOnlyMetadataContext context)
//        {
//            return null;
//        }
//
//        protected virtual bool CanWrapInternal(Type type, Type wrapperType, IReadOnlyMetadataContext context)
//        {
//            return false;
//        }
//
//        
//
//        
//
//        public bool CanWrap(Type type, Type wrapperType, IReadOnlyMetadataContext context)
//        {
//            Should.NotBeNull(type, nameof(type));
//            Should.NotBeNull(wrapperType, nameof(wrapperType));
//            if (wrapperType.IsAssignableFromUnified(type))
//                return true;
//            if (context == null)
//                context = Default.MetadataContext;
//            if (_registrations.TryGetValue(wrapperType, out var list))
//            {
//                for (int i = 0; i < list.Count; i++)
//                {
//                    if (list[i].Condition(type, context))
//                        return true;
//                }
//            }
//            return CanWrapInternal(type, wrapperType, context);
//        }
//
//        public object Wrap(object item, Type wrapperType, IReadOnlyMetadataContext context1)
//        {
//            Should.NotBeNull(item, nameof(item));
//            Should.NotBeNull(wrapperType, nameof(wrapperType));
//            if (wrapperType.IsInstanceOfTypeUnified(item))
//                return item;
//
//            var ctx = new MetadataContext();
//            if (context != null)
//                ctx.Merge(context);
//            object? wrapper = null;
//            if (_registrations.TryGetValue(wrapperType, out var list))
//            {
//                ctx.AddOrUpdate(ItemToWrapConstant, item);
//                var type = item.GetType();
//                for (int i = 0; i < list.Count; i++)
//                {
//                    WrapperRegistration registration = list[i];
//                    if (registration.Condition(type, context))
//                        wrapper = WrapInternal(item, registration, context);
//                }
//                context.Remove(ItemToWrapConstant);
//            }
//            if (wrapper == null)
//                wrapper = WrapToDefaultWrapper(item, wrapperType, context);
//            if (wrapper == null)
//                throw ExceptionManager.WrapperTypeNotSupported(wrapperType);
//            return wrapper;
//        }
//
//        
//    }
}
