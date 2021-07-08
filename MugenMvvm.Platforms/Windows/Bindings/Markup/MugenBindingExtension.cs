using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Extensions;

namespace MugenMvvm.Windows.Bindings.Markup
{
    public sealed partial class MugenBindingExtension : MarkupExtension
    {
        private static Type? _sharedDpType;

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget?)serviceProvider.GetService(typeof(IProvideValueTarget));
            if (provideValueTarget == null)
                return DependencyProperty.UnsetValue;

            var targetObject = provideValueTarget.TargetObject;
            var targetProperty = provideValueTarget.TargetProperty;
            if (targetObject == null || targetProperty == null)
                return DependencyProperty.UnsetValue;

            if (targetObject is not DependencyObject)
            {
                var type = targetObject.GetType();
                if (_sharedDpType == type || "System.Windows.SharedDp".Equals(type.FullName))
                {
                    _sharedDpType ??= type;
                    return this;
                }
            }

            if (targetObject is Setter || targetObject is DataTrigger || targetObject is Condition)
                return this;

            if (_targetPath == null)
                _defaultValue = Initialize(targetObject, targetProperty);

            if (Mugen.CanBind())
            {
                if (_bindingBuilders.IsEmpty)
                    _bindingBuilders = MugenService.BindingManager.ParseBindingExpression(this);

                if (Mugen.IsInDesignMode())
                {
                    foreach (var bindingBuilder in _bindingBuilders)
                        Mugen.BindDesignMode(bindingBuilder.Build(targetObject));
                }
                else
                {
                    foreach (var bindingBuilder in _bindingBuilders)
                        bindingBuilder.Build(targetObject);
                }
            }

            return _defaultValue;
        }

        private static void RegisterAttachedProperty(DependencyProperty property, object target)
        {
            var targetType = target.GetType();
            var member = MugenService.MemberManager.TryGetMembers(targetType, MemberType.Accessor, MemberFlags.InstancePublicAll, property.Name).Item;
            if (member == null || !member.MemberFlags.HasFlag(MemberFlags.Attached))
            {
                MugenService.MemberManager
                            .GetAttachedMemberProvider()
                            .Register(new DependencyPropertyAccessorMemberInfo(property, property.Name, targetType, MemberFlags.InstancePublic | MemberFlags.Attached));
            }
        }

        private object? Initialize(object targetObject, object targetProperty)
        {
            if (targetProperty is DependencyProperty depProp)
            {
                _targetPath = depProp.Name;
                var descriptor = DependencyPropertyDescriptor.FromProperty(depProp, targetObject.GetType());
                if (descriptor != null && descriptor.IsAttached)
                    RegisterAttachedProperty(depProp, targetObject);
                return ((DependencyObject)targetObject).GetValue(depProp);
            }

            if (targetProperty is EventInfo eventInfo)
            {
                _targetPath = eventInfo.Name;
                return CreateDelegateForEvent(eventInfo.EventHandlerType!);
            }

            if (targetProperty is MethodInfo methodInfo && methodInfo.IsStatic && methodInfo.Name.StartsWith("Add", StringComparison.Ordinal) &&
                methodInfo.Name.EndsWith("Handler", StringComparison.Ordinal))
            {
                _targetPath = methodInfo.Name.Substring(3, methodInfo.Name.Length - 10);
                var parameters = methodInfo.GetParameters();
                if (parameters.Length == 2)
                    return CreateDelegateForEvent(parameters[1].ParameterType);
            }

            _targetPath = ((MemberInfo)targetProperty).Name;
            return DependencyProperty.UnsetValue;
        }
    }
}