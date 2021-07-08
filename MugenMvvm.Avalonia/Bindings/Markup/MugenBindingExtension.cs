using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;

namespace MugenMvvm.Avalonia.Bindings.Markup
{
    public sealed partial class MugenBindingExtension : IBinding
    {
        public object? ProvideValue(IServiceProvider serviceProvider)
        {
            var provideValueTarget = (IProvideValueTarget?)serviceProvider.GetService(typeof(IProvideValueTarget));
            if (provideValueTarget == null)
                return AvaloniaProperty.UnsetValue;

            var targetObject = provideValueTarget.TargetObject;
            var targetProperty = provideValueTarget.TargetProperty;
            if (targetObject == null || targetProperty == null)
                return AvaloniaProperty.UnsetValue;

            if (targetObject is Setter)
                ExceptionManager.ThrowNotSupported("Setter is not supported by custom binding");

            if (_targetPath == null)
                _defaultValue = Initialize(targetObject, targetProperty);
            if (Mugen.CanBind())
            {
                if (_bindingBuilders.IsEmpty)
                    _bindingBuilders = MugenService.BindingManager.ParseBindingExpression(this);

                if (Design.IsDesignMode)
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

        private static void RegisterAttached(object target, AvaloniaProperty property)
        {
            var targetType = target.GetType();
            var path = property.Name;
            var member = MugenService.MemberManager.TryGetMembers(targetType, MemberType.Accessor, MemberFlags.InstancePublicAll, path).Item;
            if (member == null || !member.MemberFlags.HasFlag(MemberFlags.Attached))
            {
                MugenService.MemberManager
                            .GetAttachedMemberProvider()
                            .Register(new DelegateAccessorMemberInfo<IAvaloniaObject, object?, AvaloniaProperty>(path, targetType, property.PropertyType,
                                MemberFlags.InstancePublic | MemberFlags.Attached, property, property, (info, o, _) => o.GetValue(info.State),
                                (info, o, value, _) => o.SetValue(info.State, value), null, null));
            }
        }

        private object Initialize(object target, object targetProperty)
        {
            if (targetProperty is AvaloniaProperty avaloniaProperty)
            {
                if (avaloniaProperty.IsAttached)
                    RegisterAttached(target, avaloniaProperty);
                _targetPath = avaloniaProperty.Name;
                return this;
            }

            _targetPath = (string)targetProperty;
            var eventInfo = target.GetType().GetEvent(_targetPath, BindingFlagsEx.InstancePublic);
            if (eventInfo == null)
                return this;
            return CreateDelegateForEvent(eventInfo.EventHandlerType!);
        }

        InstancedBinding? IBinding.Initiate(IAvaloniaObject target, AvaloniaProperty targetProperty, object? anchor, bool enableDataValidation) => null;
    }
}