using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static MacrosBindingInitializer GetMacrosPreInitializer(this IBindingManager bindingManager) => bindingManager.GetMacrosInitializer(BindingComponentPriority.MacrosPreInitializer);

        public static MacrosBindingInitializer GetMacrosPostInitializer(this IBindingManager bindingManager) => bindingManager.GetMacrosInitializer(BindingComponentPriority.MacrosPostInitializer);

        public static bool IsMacros(this IUnaryExpressionNode? expression)
        {
            if (expression == null)
                return false;
            return expression.Token == UnaryTokenType.DynamicExpression || expression.Token == UnaryTokenType.StaticExpression;
        }

        [Preserve(Conditional = true)]
        public static IBinding? GetBinding(IReadOnlyMetadataContext? metadata = null) => metadata?.Get(BindingMetadata.Binding);

        [Preserve(Conditional = true)]
        public static object? GetEventArgs(IReadOnlyMetadataContext? metadata = null)
        {
            var binding = metadata?.Get(BindingMetadata.Binding);
            if (binding == null)
                return metadata?.Get(BindingMetadata.EventArgs);

            foreach (object component in binding.GetComponents())
            {
                var args = (component as IHasEventArgsComponent)?.EventArgs;
                if (args != null)
                    return args;
            }

            return null;
        }

        public static bool HasErrors(object target, object? membersRaw)
        {
            if (target is IHasService<IValidator> hasValidator)
            {
                var validator = hasValidator.ServiceOptional;
                if (validator != null)
                {
                    foreach (var member in ItemOrArray.FromRawValue<string>(membersRaw))
                    {
                        if (validator.HasErrors(member))
                            return true;
                    }
                }
            }
            else if (target is INotifyDataErrorInfo dataErrorInfo)
            {
                foreach (var member in ItemOrArray.FromRawValue<string>(membersRaw))
                {
                    if (dataErrorInfo.GetErrors(member).Any())
                        return true;
                }
            }

            return false;
        }


        public static object? GetError(object target, object? membersRaw)
        {
            if (target is IHasService<IValidator> hasValidator)
            {
                var validator = hasValidator.ServiceOptional;
                if (validator != null)
                {
                    foreach (var member in ItemOrArray.FromRawValue<string>(membersRaw))
                    {
                        var error = validator.GetErrors(member).FirstOrDefault();
                        if (error != null)
                            return error;
                    }
                }
            }
            else if (target is INotifyDataErrorInfo dataErrorInfo)
            {
                foreach (var member in ItemOrArray.FromRawValue<string>(membersRaw))
                {
                    var error = dataErrorInfo.GetErrors(member).FirstOrDefault();
                    if (error != null)
                        return error;
                }
            }

            return null;
        }

        public static IReadOnlyList<object> GetErrors(object target, object? membersRaw)
        {
            if (target is IHasService<IValidator> hasValidator)
            {
                var validator = hasValidator.ServiceOptional;
                if (validator != null)
                {
                    var editor = new ItemOrListEditor<object>();
                    foreach (var member in ItemOrArray.FromRawValue<string>(membersRaw))
                        editor.AddRange(validator.GetErrors(member));

                    if (editor.Count != 0)
                        return editor.ToItemOrList().ToArray();
                }
            }
            else if (target is INotifyDataErrorInfo dataErrorInfo)
            {
                IReadOnlyList<object>? errors = null;
                var isList = false;
                foreach (var member in ItemOrArray.FromRawValue<string>(membersRaw))
                {
                    var list = dataErrorInfo.GetErrors(member).ToReadOnlyList();
                    if (list == null || list.Count == 0)
                        continue;
                    if (errors == null)
                        errors = list;
                    else
                    {
                        if (!isList)
                        {
                            isList = true;
                            errors = new List<object>(errors);
                        }

                        ((List<object>) errors).AddRange(list);
                    }
                }

                if (errors != null)
                    return errors;
            }

            return Default.Array<object>();
        }

        [return: NotNullIfNotNull("target")]
        public static object? GetRoot(object? target, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            if (target == null)
                return null;

            while (true)
            {
                var parent = Members.BindableMembers.For<object>().Parent().GetValue(target, MemberFlags.InstancePublicAll, metadata, memberManager);
                if (parent == null)
                    return target;
                target = parent;
            }
        }

        public static T? TryFindParent<T>(object? target, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null) where T : class
        {
            if (target == null)
                return null;

            target = Members.BindableMembers.For<object>().Parent().GetValue(target, MemberFlags.InstancePublicAll, metadata, memberManager);
            while (target != null)
            {
                if (target is T r)
                    return r;

                target = Members.BindableMembers.For<object>().Parent().GetValue(target, MemberFlags.InstancePublicAll, metadata, memberManager)!;
            }

            return null;
        }

        public static object? FindRelativeSource(object target, string typeName, int level, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(typeName, nameof(typeName));
            var nameLevel = 0;
            target = Members.BindableMembers.For<object>().Parent().GetValue(target, MemberFlags.InstancePublicAll, metadata, memberManager)!;
            while (target != null)
            {
                if (TypeNameEqual(target.GetType(), typeName) && ++nameLevel == level)
                    return target;

                target = Members.BindableMembers.For<object>().Parent().GetValue(target, MemberFlags.InstancePublicAll, metadata, memberManager)!;
            }

            return null;
        }

        private static MacrosBindingInitializer GetMacrosInitializer(this IBindingManager bindingManager, int priority)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            foreach (var c in bindingManager.GetComponents<MacrosBindingInitializer>())
            {
                if (c.Priority == priority)
                    return c;
            }

            var initializer = new MacrosBindingInitializer {Priority = priority};
            bindingManager.AddComponent(initializer);
            return initializer;
        }

        private static bool TypeNameEqual(Type type, string typeName)
        {
            while (type != null)
            {
                if (type.Name == typeName)
                    return true;
                type = type.BaseType!;
            }

            return false;
        }

        #endregion
    }
}