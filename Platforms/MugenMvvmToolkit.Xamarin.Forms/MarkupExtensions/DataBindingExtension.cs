#region Copyright

// ****************************************************************************
// <copyright file="DataBindingExtension.cs">
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
using System.Collections;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Models.Exceptions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MugenMvvmToolkit.Xamarin.Forms.MarkupExtensions
{
    /// <summary>
    ///     Provides high-level access to the definition of a binding, which connects the properties of binding target objects.
    /// </summary>
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
                Tracer.Error("{0}: DataBindingExtension cannot obtain target property on '{1}' object", GetType().Name, targetObject);
                return GetEmptyValue();
            }

            var binding = HasValue
                ? CreateBindingBuilder(targetObject, path).Build()
                : CreateBinding(targetObject, path);

            if (ServiceProvider.DesignTimeManager.IsDesignMode && binding is InvalidDataBinding)
                throw new DesignTimeException(((InvalidDataBinding)binding).Exception);
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
            var xamlNode = GetValue(provideValueTarget, "Node", false);
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

        private static object GetValue(object item, string property, bool throwOnError = true)
        {
            if (item == null)
                return null;
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(item.GetType(), property, true, throwOnError);
            if (member == null)
                return null;
            return member.GetValue(item, null);
        }

        #endregion
    }
}