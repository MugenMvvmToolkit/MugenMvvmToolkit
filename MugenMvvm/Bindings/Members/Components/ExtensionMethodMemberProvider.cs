using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Members.Components
{
    public sealed class ExtensionMethodMemberProvider : AttachableComponentBase<IMemberManager>, IMemberProviderComponent, IHasPriority
    {
        private readonly HashSet<Type> _types;

        [Preserve(Conditional = true)]
        public ExtensionMethodMemberProvider()
        {
            _types = new HashSet<Type>
            {
                typeof(Enumerable)
            };
        }

        public int Priority { get; set; } = MemberComponentPriority.Extension;

        public void Add(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (_types.Add(type))
                OwnerOptional.TryInvalidateCache();
        }

        public void Remove(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (_types.Remove(type))
                OwnerOptional.TryInvalidateCache();
        }

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes,
            IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlag(MemberType.Method))
                return default;

            var members = new ItemOrListEditor<IMemberInfo>();
            foreach (var exType in _types)
            {
                var methods = exType.GetMethods(BindingFlagsEx.StaticOnly);
                for (var i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    if (method.Name != name || !method.IsDefined(typeof(ExtensionAttribute), false))
                        continue;

                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                        continue;

                    if (parameters[0].ParameterType.IsAssignableFrom(type))
                    {
                        members.Add(new MethodMemberInfo(name, method, true, type, parameters, null));
                        continue;
                    }

                    if (!method.IsGenericMethodDefinition)
                        continue;

                    method = TryMakeGenericMethod(method, type, out var genericArgs)!;
                    if (method == null)
                        continue;

                    parameters = method.GetParameters();
                    if (parameters[0].ParameterType.IsAssignableFrom(type))
                        members.Add(new MethodMemberInfo(name, method, true, type, parameters, genericArgs));
                }
            }

            return members.ToItemOrList();
        }

        private MethodInfo? TryMakeGenericMethod(MethodInfo method, Type type, out Type[]? genericArguments)
        {
            try
            {
                genericArguments = BindingMugenExtensions.TryInferGenericParameters<ParameterInfo, Type>(method.GetGenericArguments(),
                    method.GetParameters(), info => info.ParameterType, type, (data, i) => data, 1, out _).AsList();
                if (genericArguments.Length == 0)
                    return null;
                return method.MakeGenericMethod(genericArguments);
            }
            catch
            {
                genericArguments = null;
                return null;
            }
        }
    }
}