#region Copyright
// ****************************************************************************
// <copyright file="BindingManager.cs">
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
using System.Linq;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the binding manager.
    /// </summary>
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

        /// <summary>
        ///     Registers the specified binding.
        /// </summary>
        /// <param name="target">The specified target.</param>
        /// <param name="path">The specified path.</param>
        /// <param name="binding">The specified <see cref="IDataBinding" />.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        public virtual void Register(object target, string path, IDataBinding binding, IDataContext context = null)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(path, "path");
            Should.NotBeNull(binding, "binding");
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

        /// <summary>
        ///     Determine whether the specified binding is available in the <see cref="IBindingManager" />.
        /// </summary>
        /// <param name="binding">The <see cref="IDataBinding" /> to test for the registration of.</param>
        /// <returns>
        ///     True if the binding is registered.
        /// </returns>
        public virtual bool IsRegistered(IDataBinding binding)
        {
            Should.NotBeNull(binding, "binding");
            var dataBinding = binding as DataBinding;
            if (dataBinding == null)
                return ServiceProvider.AttachedValueProvider.GetValue<object>(binding, IsRegisteredMember, false) != null;
            return dataBinding.IsAssociated;
        }

        /// <summary>
        ///     Retrieves the <see cref="IDataBinding" /> objects.
        /// </summary>
        /// <param name="target">The object to get bindings.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        public virtual IEnumerable<IDataBinding> GetBindings(object target, IDataContext context = null)
        {
            Should.NotBeNull(target, "target");
            return ServiceProvider
                .AttachedValueProvider
                .GetValues(target, GetBindingPredicateDelegate)
                .ToArrayEx(pair => (IDataBinding)pair.Value);
        }

        /// <summary>
        ///     Retrieves the <see cref="IDataBinding" /> objects that is set on the specified property.
        /// </summary>
        /// <param name="target">The object where <paramref name="path" /> is.</param>
        /// <param name="path">The binding target property from which to retrieve the binding.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        public virtual IEnumerable<IDataBinding> GetBindings(object target, string path, IDataContext context = null)
        {
            Should.NotBeNull(target, "target");
            object value;
            if (ServiceProvider.AttachedValueProvider.TryGetValue(target, BindPrefix + path, out value))
                return new[] { (IDataBinding)value };
            return Enumerable.Empty<IDataBinding>();
        }

        /// <summary>
        ///     Unregisters the specified <see cref="IDataBinding"/>.
        /// </summary>
        public virtual void Unregister(IDataBinding binding)
        {
            Should.NotBeNull(binding, "binding");
            object source = binding.TargetAccessor.Source.GetSource(false);
            string path = binding.TargetAccessor.Source.Path.Path;
            if (source != null && path != null)
                ClearBindings(source, path);
        }

        /// <summary>
        ///     Removes all bindings from the specified target.
        /// </summary>
        /// <param name="target">The object from which to remove bindings.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        public virtual void ClearBindings(object target, IDataContext context = null)
        {
            Should.NotBeNull(target, "target");
            IAttachedValueProvider provider = ServiceProvider.AttachedValueProvider;
            var values = provider.GetValues(target, GetBindingPredicateDelegate);
            for (int index = 0; index < values.Count; index++)
            {
                var value = values[index];
                ClearBinding((IDataBinding)value.Value);
                provider.Clear(target, value.Key);
            }
        }

        /// <summary>
        ///     Removes the bindings from a property if there is one.
        /// </summary>
        /// <param name="target">The object from which to remove the bindings.</param>
        /// <param name="path">The property path from which to remove the bindings.</param>
        /// <param name="context">The specified <see cref="IDataContext"/>, if any.</param>
        public virtual void ClearBindings(object target, string path, IDataContext context = null)
        {
            Should.NotBeNull(target, "target");
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