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
using System.Reflection;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Binding;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Xamarin.Forms.MarkupExtensions
{
    [ContentProperty("Path")]
    public partial class DataBindingExtension : IMarkupExtension
    {
        #region Constructors

        public DataBindingExtension()
        {
            if (XamarinFormsToolkitExtensions.IsDesignMode)
                XamarinFormsDataBindingExtensions.InitializeFromDesignContext();
        }

        #endregion

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
                Tracer.Error($"{GetType().Name}: DataBindingExtension cannot obtain target property on '{targetObject}'");
                return GetEmptyValue();
            }

            var isDesignMode = XamarinFormsToolkitExtensions.IsDesignMode;
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

        public static Func<IProvideValueTarget, IServiceProvider, string> GetTargetPropertyNameDelegate { get; set; }

        #endregion

        #region Methods

        protected virtual string GetTargetPropertyName(IProvideValueTarget provideValueTarget, IServiceProvider serviceProvider)
        {
            var targetProperty = provideValueTarget.TargetProperty;
            var bindableProperty = targetProperty as BindableProperty;
            if (bindableProperty != null)
                return bindableProperty.PropertyName;
            var memberInfo = targetProperty as MemberInfo;
            if (memberInfo != null)
                return memberInfo.Name;

            return GetTargetPropertyNameDelegate?.Invoke(provideValueTarget, serviceProvider);
        }

        #endregion
    }
}
