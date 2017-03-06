#region Copyright

// ****************************************************************************
// <copyright file="DataBindingExtension.cs">
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
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MugenMvvmToolkit.Xamarin.Forms.MarkupExtensions
{
    [ContentProperty("Path")]
    public partial class DataBindingExtension : IMarkupExtension
    {
        #region Implementation of IMarkupExtension

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = serviceProvider.GetService<IProvideValueTarget>();
            if (provideValueTarget == null)
                return GetEmptyValue();
            var targetObject = provideValueTarget.TargetObject;
            if (targetObject == null)
                return GetEmptyValue();

            var path = GetTargetPropertyName(provideValueTarget, serviceProvider);
            if (path == null)
            {
                Tracer.Error($"{GetType().Name}: DataBindingExtension cannot obtain target property on '{targetObject}' object");
                return GetEmptyValue();
            }

            var isDesignMode = ServiceProvider.IsDesignMode;
            var binding = HasValue
                ? CreateBindingBuilder(targetObject, path).Build()
                : CreateBinding(targetObject, path, isDesignMode);

            if (isDesignMode && binding is InvalidDataBinding)
            {
                var exception = ((InvalidDataBinding)binding).Exception;
                throw new InvalidOperationException(exception.Flatten(true), exception);
            }
            return GetDefaultValue(targetObject, null, binding, path);
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static Func<IProvideValueTarget, IServiceProvider, string> GetTargetPropertyNameDelegate { get; set; }

        #endregion

        #region Methods

        protected virtual string GetTargetPropertyName(IProvideValueTarget provideValueTarget, IServiceProvider serviceProvider)
        {
            //NOTE Xamarin doesn't support this property.
            //return serviceProvider.GetService<IProvideValueTarget>().TargetProperty;
            //http://forums.xamarin.com/discussion/36884/missing-implementation-of-iprovidevaluetarget-targetproperty-property-imarkupextension

            if (GetTargetPropertyNameDelegate != null)
                return GetTargetPropertyNameDelegate(provideValueTarget, serviceProvider);

            //Making some reflection magic.
            var xamlNode = GetValue(provideValueTarget, "Node");
            if (xamlNode == null)
            {
                var xamlNodeProvider = GetValue(serviceProvider, "IXamlNodeProvider");
                xamlNode = GetValue(xamlNodeProvider, "XamlNode");
            }

            var properties = (IDictionary)GetValue(GetValue(xamlNode, "Parent"), "Properties");
            object xmlName = null;
            if (properties != null)
            {
                foreach (DictionaryEntry entry in properties)
                {
                    if (ReferenceEquals(entry.Value, xamlNode))
                    {
                        xmlName = entry.Key;
                        break;
                    }
                }
            }
            return (string)GetValue(xmlName, "LocalName");
        }


        private struct CacheKey
        {
            public readonly Type Type;
            public readonly string Name;

            public CacheKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            private sealed class TypeNameEqualityComparer : IEqualityComparer<CacheKey>
            {
                public bool Equals(CacheKey x, CacheKey y)
                {
                    return x.Type.Equals(y.Type) && string.Equals(x.Name, y.Name);
                }

                public int GetHashCode(CacheKey obj)
                {
                    unchecked
                    {
                        return (obj.Type.GetHashCode() * 397) ^ obj.Name.GetHashCode();
                    }
                }
            }

            public static readonly IEqualityComparer<CacheKey> TypeNameComparerInstance = new TypeNameEqualityComparer();
        }

        private static readonly Dictionary<CacheKey, Func<object, object>> PropertiesCache = new Dictionary<CacheKey, Func<object, object>>(CacheKey.TypeNameComparerInstance);

        private static object GetValue(object item, string property)
        {
            if (item == null)
                return null;
            var key = new CacheKey(item.GetType(), property);
            Func<object, object> func;
            if (!PropertiesCache.TryGetValue(key, out func))
            {
                var propertyInfo = key.Type.GetPropertyEx(property, MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Instance);
                if (propertyInfo != null)
                {
                    func = ServiceProvider.ReflectionManager.GetMemberGetter<object>(propertyInfo);
                    PropertiesCache[key] = func;
                }
            }
            return func?.Invoke(item);
        }

        #endregion
    }
}
