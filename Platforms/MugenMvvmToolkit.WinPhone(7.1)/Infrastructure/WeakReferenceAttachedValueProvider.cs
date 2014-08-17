#region Copyright
// ****************************************************************************
// <copyright file="WeakReferenceAttachedValueProvider.cs">
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using MugenMvvmToolkit.Collections;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the attached value provider class, that allows to attach a value to an object using path.
    /// </summary>
    public sealed class WeakReferenceAttachedValueProvider : AttachedValueProviderBase
    {
        #region Nested types

        private sealed class AttachedValueDictionary : LightDictionaryBase<string, object>
        {
            #region Fields

            private const string ProviderKey = "$$!!~~ProviderKey";
            private WeakKey _reference;

            #endregion

            #region Constructors

            public AttachedValueDictionary()
                : base(true)
            {
            }

            public AttachedValueDictionary(WeakKey reference, WeakReferenceAttachedValueProvider provider)
                : base(true)
            {
                _reference = reference;
                Add(ProviderKey, provider);
            }

            #endregion

            #region Methods

            public new void Clear()
            {
                base.Clear();
            }

            #endregion

            #region Overrides of LightDictionaryBase<string,object>

            protected override bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.Ordinal);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion

            #region Destructors

            ~AttachedValueDictionary()
            {
                if (!ContainsKey(ProviderKey))
                    return;
                try
                {
                    if (_reference.IsAlive)
                        GC.ReRegisterForFinalize(this);
                    else
                    {
                        var value = (WeakReferenceAttachedValueProvider)this[ProviderKey];
                        value.ClearInternal(ref _reference);
                    }
                }
                catch (Exception)
                {
                }
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct WeakKey
        {
            #region Fields

            public readonly int HashCode;
            public readonly object Reference;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="WeakKey" /> class.
            /// </summary>
            public WeakKey(object key, bool isWeak)
            {
                Reference = isWeak ? ServiceProvider.WeakReferenceFactory(key, true) : key;
                HashCode = RuntimeHelpers.GetHashCode(key);
            }

            #endregion

            #region Properties

            public object Target
            {
                get
                {
                    var weak = Reference as WeakReference;
                    if (weak == null)
                        return Reference;
                    return weak.Target;
                }
            }

            public bool IsAlive
            {
                get
                {
                    var weak = Reference as WeakReference;
                    if (weak == null)
                        return true;
                    return weak.IsAlive;
                }
            }

            #endregion

            #region Methods

            public void Free()
            {
                var weak = Reference as WeakReference;
                if (weak != null)
                    weak.Target = null;
            }

            #endregion
        }

        private sealed class WeakKeyLightDictionary : LightDictionaryBase<WeakKey, WeakReference>
        {
            #region Constructors

            public WeakKeyLightDictionary()
                : base(353)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<WeakKey,WeakReference>

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            protected override bool Equals(WeakKey x, WeakKey y)
            {
                if (ReferenceEquals(x.Reference, y.Reference))
                    return true;
                object target1 = x.Target;
                if (target1 == null)
                    return false;
                object target2 = y.Target;
                if (target2 == null)
                    return false;
                return ReferenceEquals(target1, target2);
            }

            /// <summary>
            ///     Returns a hash code for the specified object.
            /// </summary>
            protected override int GetHashCode(WeakKey key)
            {
                return key.HashCode;
            }

            #endregion

            #region Methods

            public new void Add(WeakKey key, WeakReference reference)
            {
                base.Add(key, reference);
            }

            public new bool TryGetValue(WeakKey key, out WeakReference reference)
            {
                return base.TryGetValue(key, out reference);
            }

            public new bool Remove(WeakKey key)
            {
                return base.Remove(key);
            }

            #endregion
        }

        #endregion

        #region Fields

        //NOTE ConditionalWeakTable not supported on WP 7.8, we should use attached propertyt to simulate it.
        private static readonly DependencyProperty AttachedValueDictionaryProperty = DependencyProperty.RegisterAttached(
            "AttachedValueDictionary", typeof(AttachedValueDictionary), typeof(WeakReferenceAttachedValueProvider), new PropertyMetadata(default(AttachedValueDictionary)));
        private readonly WeakKeyLightDictionary _internalDictionary;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WeakReferenceAttachedValueProvider" /> class.
        /// </summary>
        public WeakReferenceAttachedValueProvider()
        {
            _internalDictionary = new WeakKeyLightDictionary();
        }

        #endregion

        #region Overrides of AttachedValueProviderBase

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
        protected override bool ClearInternal(object item)
        {
            var dependencyObject = item as DependencyObject;
            if (dependencyObject != null)
            {
                dependencyObject.ClearValue(AttachedValueDictionaryProperty);
                return true;
            }

            var weakKey = new WeakKey(item, false);
            return ClearInternal(ref weakKey);
        }

        /// <summary>
        ///     Gets or adds the attached values container.
        /// </summary>
        protected override LightDictionaryBase<string, object> GetOrAddAttachedValues(object item, bool addNew)
        {
            var dependencyObject = item as DependencyObject;
            if (dependencyObject != null)
            {
                var dict = (AttachedValueDictionary)dependencyObject.GetValue(AttachedValueDictionaryProperty);
                if (dict == null && addNew)
                {
                    dict = new AttachedValueDictionary();
                    dependencyObject.SetValue(AttachedValueDictionaryProperty, dict);
                }
                return dict;
            }

            lock (_internalDictionary)
            {
                WeakReference reference;
                if (!_internalDictionary.TryGetValue(new WeakKey(item, false), out reference))
                {
                    if (!addNew)
                        return null;
                    var weakKey = new WeakKey(item, true);
                    reference = new WeakReference(new AttachedValueDictionary(weakKey, this), true);
                    _internalDictionary.Add(weakKey, reference);
                }
                return (LightDictionaryBase<string, object>)reference.Target;
            }
        }

        #endregion

        #region Methods

        private bool ClearInternal(ref WeakKey key)
        {
            lock (_internalDictionary)
            {
                WeakReference value;
                if (!_internalDictionary.TryGetValue(key, out value) || !_internalDictionary.Remove(key))
                    return false;
                var dictionary = (AttachedValueDictionary)value.Target;
                if (dictionary != null)
                    dictionary.Clear();
                key.Free();
                return true;
            }
        }

        #endregion
    }
}