using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ExtensionMethodMemberProvider : AttachableComponentBase<IMemberManager>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly Type[] _singleTypeBuffer;
        private readonly HashSet<Type> _types;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ExtensionMethodMemberProvider()
        {
            _singleTypeBuffer = new Type[1];
            _types = new HashSet<Type>
            {
                typeof(Enumerable)
            };
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Extension;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlagEx(MemberType.Method))
                return default;

            var members = ItemOrListEditor.Get<IMemberInfo>();
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

            return members.ToItemOrList<IReadOnlyList<IMemberInfo>>();
        }

        #endregion

        #region Methods

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

        private MethodInfo? TryMakeGenericMethod(MethodInfo method, Type type, out Type[]? genericArguments)
        {
            try
            {
                _singleTypeBuffer[0] = type;
                genericArguments = MugenBindingExtensions.TryInferGenericParameters(method.GetGenericArguments(),
                    method.GetParameters(), info => info.ParameterType, _singleTypeBuffer, (data, i) => data[i], _singleTypeBuffer.Length, out _);
                if (genericArguments == null)
                    return null;
                return method.MakeGenericMethod(genericArguments);
            }
            catch
            {
                genericArguments = null;
                return null;
            }
        }

        #endregion
    }
}