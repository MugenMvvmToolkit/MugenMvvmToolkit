using System;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions
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

        public static object? FindRelativeSource(object target, string typeName, int level, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(typeName, nameof(typeName));
            int nameLevel = 0;

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
                type = type.BaseType;
            }

            return false;
        }

        #endregion
    }
}