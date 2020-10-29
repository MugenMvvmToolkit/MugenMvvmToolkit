using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Methods

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

            var itemOrList = binding.GetComponents();
            var components = itemOrList.List;
            var component = itemOrList.Item;
            if (components == null)
                return (component as IHasEventArgsBindingComponent)?.EventArgs;

            for (var i = 0; i < components.Length; i++)
            {
                var args = (components[i] as IHasEventArgsBindingComponent)?.EventArgs;
                if (args != null)
                    return args;
            }

            return null;
        }

        public static bool HasErrors(object target, string[] members)
        {
            Should.NotBeNull(members, nameof(members));
            if (target is IHasService<IValidator> hasValidator)
            {
                var validator = hasValidator.ServiceOptional;
                if (validator != null)
                {
                    for (var i = 0; i < members.Length; i++)
                    {
                        if (validator.HasErrors(members[i]))
                            return true;
                    }
                }
            }
            else if (target is INotifyDataErrorInfo dataErrorInfo)
            {
                for (var i = 0; i < members.Length; i++)
                {
                    if (dataErrorInfo.GetErrors(members[i]).Any())
                        return true;
                }
            }

            return false;
        }


        public static object? GetError(object target, string[] members)
        {
            Should.NotBeNull(members, nameof(members));
            if (target is IHasService<IValidator> hasValidator)
            {
                var validator = hasValidator.ServiceOptional;
                if (validator != null)
                {
                    for (var i = 0; i < members.Length; i++)
                    {
                        var error = validator.GetErrors(members[i]).FirstOrDefault();
                        if (error != null)
                            return error;
                    }
                }
            }
            else if (target is INotifyDataErrorInfo dataErrorInfo)
            {
                for (var i = 0; i < members.Length; i++)
                {
                    var error = dataErrorInfo.GetErrors(members[i]).FirstOrDefault();
                    if (error != null)
                        return error;
                }
            }

            return null;
        }

        public static IReadOnlyList<object> GetErrors(object target, string[] members)
        {
            Should.NotBeNull(members, nameof(members));
            if (target is IHasService<IValidator> hasValidator)
            {
                var validator = hasValidator.ServiceOptional;
                if (validator != null)
                {
                    var editor = ItemOrListEditor.Get<object>();
                    for (var i = 0; i < members.Length; i++)
                        editor.AddRange(validator.GetErrors(members[i]));

                    if (editor.Count != 0)
                        return editor.ToItemOrList().Iterator().ToArray();
                }
            }
            else if (target is INotifyDataErrorInfo dataErrorInfo)
            {
                IReadOnlyList<object>? errors = null;
                var isList = false;
                for (var i = 0; i < members.Length; i++)
                {
                    var list = dataErrorInfo.GetErrors(members[i]).ToReadOnlyList();
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