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

        public static object? FindElementSource(object target, string elementName, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(elementName, nameof(elementName));
            var methodDescriptor = Members.BindableMembers.For<object>().FindElementByNameMethod();
            var args = new object[1];
            while (target != null)
            {
                args[0] = elementName;
                var result = methodDescriptor.RawMethod.Invoke(target, args, MemberFlags.InstancePublicAll, metadata, memberManager);
                if (result != null)
                    return result;
                target = Members.BindableMembers.For<object>().Parent().GetValue(target, metadata: metadata, memberManager: memberManager)!;
            }

            return null;
        }

        public static object? FindRelativeSource(object target, string typeName, int level, IReadOnlyMetadataContext? metadata = null, IMemberManager? memberManager = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(typeName, nameof(typeName));
            object? fullNameSource = null;
            object? nameSource = null;
            var fullNameLevel = 0;
            var nameLevel = 0;

            target = Members.BindableMembers.For<object>().Parent().GetValue(target, MemberFlags.InstancePublicAll, metadata, memberManager)!;
            while (target != null)
            {
                TypeNameEqual(target.GetType(), typeName, out var shortNameEqual, out var fullNameEqual);
                if (shortNameEqual)
                {
                    nameSource = target;
                    nameLevel++;
                }

                if (fullNameEqual)
                {
                    fullNameSource = target;
                    fullNameLevel++;
                }

                if (fullNameSource != null && fullNameLevel == level)
                    return fullNameSource;
                if (nameSource != null && nameLevel == level)
                    return nameSource;

                target = Members.BindableMembers.For<object>().Parent().GetValue(target, MemberFlags.InstancePublicAll, metadata, memberManager)!;
            }

            return null;
        }

        #endregion
    }
}