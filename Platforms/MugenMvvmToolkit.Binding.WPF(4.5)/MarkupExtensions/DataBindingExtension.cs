#region Copyright
// ****************************************************************************
// <copyright file="DataBindingExtension.cs">
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
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models.Exceptions;

namespace MugenMvvmToolkit.Binding.MarkupExtensions
{
    /// <summary>
    ///     Provides high-level access to the definition of a binding, which connects the properties of binding target objects.
    /// </summary>
    public partial class DataBindingExtension : MarkupExtension
    {
        #region Fields

        private string _targetMemberName;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of a class derived from <see cref="DataBindingExtension" />.
        /// </summary>
        public DataBindingExtension()
        {
            _targetMemberName = string.Empty;
        }

#if WPF
        /// <summary>
        ///     Initializes a new instance of a class derived from <see cref="DataBindingExtension" />.
        /// </summary>
        public DataBindingExtension(string path)
            : this()
        {
            Path = path;
        }
#endif
        #endregion

        #region Overrides of MarkupExtension

        /// <summary>
        ///     When implemented in a derived class, returns an object that is provided as the value of the target property for
        ///     this markup extension.
        /// </summary>
        /// <returns>
        ///     The object value to set on the property where the extension is applied.
        /// </returns>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService<IProvideValueTarget>();
            if (target == null)
                return GetEmptyValue();
            var targetObject = target.TargetObject;
            var targetProperty = target.TargetProperty;
            if (targetObject == null || targetProperty == null)
                return GetEmptyValue();
            if (!(targetObject is DependencyObject) && targetObject.GetType().FullName == "System.Windows.SharedDp")
                return this;
#if WPF
            if (targetObject is Setter || targetObject is DataTrigger || targetObject is Condition)
#else
            if (targetObject is Setter)
#endif
                return this;

            if (_targetMemberName == string.Empty)
                _targetMemberName = GetMemberName(targetObject, targetProperty);
            if (_targetMemberName == null)
                return GetEmptyValue();

            IDataBinding binding = HasValue
                ? CreateBindingBuilder(targetObject, _targetMemberName).Build()
                : CreateBinding(targetObject, _targetMemberName);

            if (ServiceProvider.DesignTimeManager.IsDesignMode && binding is InvalidDataBinding)
                throw new DesignTimeException(((InvalidDataBinding)binding).Exception);
            return GetDefaultValue(targetObject, targetProperty, binding, _targetMemberName);
        }

        #endregion

        #region Methods

        protected virtual string GetMemberName(object targetObject, object targetProperty)
        {
#if WPF
            var depProp = targetProperty as DependencyProperty;
            if (depProp != null)
            {
                var descriptor = DependencyPropertyDescriptor.FromProperty(depProp, targetObject.GetType());
                if (descriptor != null && descriptor.IsAttached)
                    return RegisterAttachedProperty(depProp, targetObject);
                return depProp.Name;
            }
#endif
            var member = (MemberInfo)targetProperty;
#if !WPF
            if (member.MemberType == MemberTypes.Method)
                return RegisterAttachedProperty((MethodInfo)member, targetObject);
#endif
            return member.Name;
        }

#if WPF
        private static string RegisterAttachedProperty(DependencyProperty property, object target)
        {
            var targetType = target.GetType();
            var path = property.Name + "Property";
            var member = BindingServiceProvider
                                        .MemberProvider
                                        .GetBindingMember(targetType, path, false, false);
            if (member == null)
            {
                BindingServiceProvider
                               .MemberProvider
                               .Register(targetType,
                                   new DependencyPropertyBindingMember(property, path, property.PropertyType,
                                       property.ReadOnly, null, null), true);
            }
            return path;
        }
#else
        private static string RegisterAttachedProperty(MethodInfo method, object target)
        {
            if (!(target is DependencyObject))
                return method.Name;

            Type reflectedType = method.ReflectedType;
            if (reflectedType == null)
                return null;

            var targetType = target.GetType();
            string name = method.Name.Replace("Get", string.Empty) + "Property";
            var memberInfo = BindingServiceProvider
                                            .MemberProvider
                                            .GetBindingMember(targetType, name, false, false);
            if (memberInfo != null)
                return name;

            var fullName = string.Format("_attached_{0}_{1}", reflectedType.FullName.Replace(".", "_"), name);
            memberInfo = BindingServiceProvider
                                        .MemberProvider
                                        .GetBindingMember(targetType, fullName, false, false);

            if (memberInfo != null)
                return fullName;

            FieldInfo fieldInfo = reflectedType.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo == null)
                return null;

            BindingServiceProvider
                           .MemberProvider
                           .Register(method.GetParameters()[0].ParameterType,
                               new DependencyPropertyBindingMember((DependencyProperty)fieldInfo.GetValue(null), fullName, method.ReturnType, false, method, null), true);
            return fullName;
        }
#endif

        #endregion
    }
}