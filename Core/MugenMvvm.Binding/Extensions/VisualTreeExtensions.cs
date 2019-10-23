using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static object? GetParent(object? target, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            return target?.GetBindableMemberValue(BindableMembers.Object.Parent, null, MemberFlags.All, metadata, provider);
        }

        public static object? FindByName(object target, string elementName, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(elementName, nameof(elementName));
            var args = new object[1];
            while (target != null)
            {
                args[0] = elementName;
                var result = target.TryInvokeBindableMethod(BindableMembers.Object.FindByNameMethod, args, MemberFlags.All, metadata, provider);
                if (result != null)
                    return result;
                target = GetParent(target, metadata, provider)!;
            }

            return null;
        }

        public static object? FindRelativeSource(object target, string typeName, int level, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(typeName, nameof(typeName));
            object? fullNameSource = null;
            object? nameSource = null;
            var fullNameLevel = 0;
            var nameLevel = 0;

            target = GetParent(target, metadata, provider)!;
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

                target = GetParent(target, metadata, provider)!;
            }

            return null;
        }

        private static void TypeNameEqual(Type type, string typeName, out bool shortNameEqual, out bool fullNameEqual)
        {
            shortNameEqual = false;
            fullNameEqual = false;
            while (type != null)
            {
                if (!shortNameEqual)
                {
                    if (type.Name == typeName)
                    {
                        shortNameEqual = true;
                        if (fullNameEqual)
                            break;
                    }
                }

                if (!fullNameEqual && (type.FullName == typeName || type.AssemblyQualifiedName == typeName))
                {
                    fullNameEqual = true;
                    if (shortNameEqual)
                        break;
                }

                type = type.GetBaseTypeUnified();
            }
        }

        #endregion
    }
}