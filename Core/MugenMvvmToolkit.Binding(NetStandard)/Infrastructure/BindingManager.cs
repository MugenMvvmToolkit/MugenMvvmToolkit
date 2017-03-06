#region Copyright

// ****************************************************************************
// <copyright file="BindingManager.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindingManager : IBindingManager
    {
        #region Fields

        private const string BindPrefix = "#${Binding}.";
        private const string IsRegisteredMember = "#$BindingIsAssociated";
        private static readonly UpdateValueDelegate<object, IDataBinding, IDataBinding, object> UpdateValueFactoryDelegate;
        private static readonly Func<string, object, bool> GetBindingPredicateDelegate;

        #endregion

        #region Constructors

        static BindingManager()
        {
            UpdateValueFactoryDelegate = UpdateValueFactory;
            GetBindingPredicateDelegate = GetBindingPredicate;
        }

        #endregion

        #region Implementation of IBindingManager

        public virtual void Register(object target, string path, IDataBinding binding, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));
            Should.NotBeNull(binding, nameof(binding));
            var dataBinding = binding as DataBinding;
            if (dataBinding == null)
            {
                if (ServiceProvider.AttachedValueProvider.GetValue<object>(binding, IsRegisteredMember, false) != null)
                    throw BindingExceptionManager.DuplicateBindingRegistration(binding);
                ServiceProvider.AttachedValueProvider.SetValue(binding, IsRegisteredMember, Empty.TrueObject);
            }
            else
            {
                if (dataBinding.IsAssociated)
                    throw BindingExceptionManager.DuplicateBindingRegistration(binding);
                dataBinding.IsAssociated = true;
            }
            ServiceProvider
                .AttachedValueProvider
                .AddOrUpdate(target, BindPrefix + path, binding, UpdateValueFactoryDelegate);
        }

        public virtual bool IsRegistered(IDataBinding binding)
        {
            Should.NotBeNull(binding, nameof(binding));
            var dataBinding = binding as DataBinding;
            if (dataBinding == null)
                return ServiceProvider.AttachedValueProvider.GetValue<object>(binding, IsRegisteredMember, false) != null;
            return dataBinding.IsAssociated;
        }

        public virtual ICollection<IDataBinding> GetBindings(object target, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            return ServiceProvider
                .AttachedValueProvider
                .GetValues(target, GetBindingPredicateDelegate)
                .ToArrayEx(pair => (IDataBinding)pair.Value);
        }

        public virtual ICollection<IDataBinding> GetBindings(object target, string path, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            object value;
            if (ServiceProvider.AttachedValueProvider.TryGetValue(target, BindPrefix + path, out value))
                return new[] { (IDataBinding)value };
            return Empty.Array<IDataBinding>();
        }

        public virtual void Unregister(IDataBinding binding)
        {
            Should.NotBeNull(binding, nameof(binding));
            object source = binding.TargetAccessor.Source.GetActualSource(false);
            string path = binding.TargetAccessor.Source.Path.Path;
            if (source != null && path != null)
                ClearBindings(source, path);
        }

        public virtual void ClearBindings(object target, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            IAttachedValueProvider provider = ServiceProvider.AttachedValueProvider;
            var values = provider.GetValues(target, GetBindingPredicateDelegate);
            for (int index = 0; index < values.Count; index++)
            {
                var value = values[index];
                ClearBinding((IDataBinding)value.Value);
                provider.Clear(target, value.Key);
            }
        }

        public virtual void ClearBindings(object target, string path, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            path = BindPrefix + path;
            var binding = ServiceProvider.AttachedValueProvider.GetValue<IDataBinding>(target, path, false);
            if (binding != null)
            {
                ClearBinding(binding);
                ServiceProvider.AttachedValueProvider.Clear(target, path);
            }
        }

        #endregion

        #region Methods

        private static bool GetBindingPredicate(string s, object o)
        {
            return s.StartsWith(BindPrefix, StringComparison.Ordinal);
        }

        private static IDataBinding UpdateValueFactory(object o, IDataBinding dataBinding, IDataBinding arg3, object state)
        {
            ClearBinding(arg3);
            return dataBinding;
        }

        private static void ClearBinding(IDataBinding binding)
        {
            binding.Dispose();
            var dataBinding = binding as DataBinding;
            if (dataBinding == null)
                ServiceProvider.AttachedValueProvider.Clear(binding, IsRegisteredMember);
            else
                dataBinding.IsAssociated = false;
        }

        #endregion
    }
}
