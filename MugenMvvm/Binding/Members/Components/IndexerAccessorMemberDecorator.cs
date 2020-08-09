using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Convert;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class IndexerAccessorMemberDecorator : ComponentDecoratorBase<IMemberManager, IMemberProviderComponent>, IMemberProviderComponent, IHasPriority,
        IEqualityComparer<IndexerAccessorMemberDecorator.MemberKey>
    {
        #region Fields

        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly List<IMemberInfo> _members;
        private readonly Dictionary<MemberKey, (List<IMethodMemberInfo>? getters, List<IMethodMemberInfo>? setters, object?[] args, ArgumentFlags flags)> _membersDictionary;
        private readonly IObservationManager? _observationManager;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public IndexerAccessorMemberDecorator(IGlobalValueConverter? globalValueConverter = null, IObservationManager? observationManager = null)
        {
            _globalValueConverter = globalValueConverter;
            _observationManager = observationManager;
            _members = new List<IMemberInfo>();
            _membersDictionary = new Dictionary<MemberKey, (List<IMethodMemberInfo>? getters, List<IMethodMemberInfo>? setters, object?[] args, ArgumentFlags flags)>(this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        bool IEqualityComparer<MemberKey>.Equals(MemberKey x, MemberKey y)
        {
            if (x.Type != y.Type || x.ReturnType != y.ReturnType)
                return false;

            var xCount = x.ParametersCount;
            if (xCount != y.ParametersCount)
                return false;

            for (var i = 0; i < xCount; i++)
            {
                if (x.Parameters[i].ParameterType != y.Parameters[i].ParameterType)
                    return false;
            }

            return true;
        }

        int IEqualityComparer<MemberKey>.GetHashCode(MemberKey key) => HashCode.Combine(key.Type, key.ReturnType, key.ParametersCount);

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            if (!memberTypes.HasFlagEx(MemberType.Accessor))
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            var indexerArgsRaw = MugenBindingExtensions.GetIndexerArgsRaw(name);
            if (indexerArgsRaw == null)
                return Components.TryGetMembers(memberManager, type, name, memberTypes, metadata);

            string getterName;
            string? setterName;
            if (type.IsArray)
            {
                getterName = BindingInternalConstant.ArrayGetterName;
                setterName = BindingInternalConstant.ArraySetterName;
            }
            else if (type == typeof(string))
            {
                getterName = BindingInternalConstant.IndexerStringGetterName;
                setterName = null;
            }
            else
            {
                getterName = BindingInternalConstant.IndexerGetterName;
                setterName = BindingInternalConstant.IndexerSetterName;
            }

            _membersDictionary.Clear();
            _members.Clear();
            Components.TryAddMembers(memberManager, _members, type, getterName, MemberType.Method, metadata);
            for (var i = 0; i < _members.Count; i++)
            {
                if (!(_members[i] is IMethodMemberInfo method) || method.Type == typeof(void))
                    continue;

                var parameters = method.GetParameters();
                var args = _globalValueConverter.TryGetInvokeArgs(parameters, indexerArgsRaw, metadata, out var flags);
                if (args == null || args.Length == 0)
                    continue;

                var key = new MemberKey(method.DeclaringType, method.Type, parameters, false);
                if (!_membersDictionary.TryGetValue(key, out var value))
                {
                    value = (new List<IMethodMemberInfo>(), null, args, flags);
                    _membersDictionary[key] = value;
                }

                value.getters!.Add(method);
            }

            if (setterName != null)
            {
                _members.Clear();
                Components.TryAddMembers(memberManager, _members, type, setterName, MemberType.Method, metadata);

                for (var i = 0; i < _members.Count; i++)
                {
                    if (!(_members[i] is IMethodMemberInfo method) || method.Type != typeof(void))
                        continue;

                    var parameters = method.GetParameters();
                    if (parameters.Count < 2)
                        continue;

                    var lastParameter = parameters[parameters.Count - 1];
                    var key = new MemberKey(method.DeclaringType, lastParameter.ParameterType, parameters, true);
                    if (!_membersDictionary.TryGetValue(key, out var value))
                    {
                        var args = _globalValueConverter.TryGetInvokeArgs(parameters.Take(parameters.Count - 1).ToList(), indexerArgsRaw, metadata, out var flags);
                        if (args == null || args.Length == 0)
                            continue;

                        value = (null, new List<IMethodMemberInfo>(), args, flags);
                        _membersDictionary[key] = value;
                    }

                    if (value.setters == null)
                    {
                        value = (value.getters, new List<IMethodMemberInfo>(), value.args, value.flags);
                        _membersDictionary[key] = value;
                    }

                    value.setters!.Add(method);
                }
            }

            _members.Clear();
            foreach (var item in _membersDictionary)
            {
                IMethodMemberInfo? getter = null, setter = null;
                if (item.Value.getters != null)
                    getter = Owner.TryGetMember(type, MemberType.Method, MemberFlags.All, item.Value.getters, metadata) as IMethodMemberInfo;
                if (item.Value.setters != null)
                    setter = Owner.TryGetMember(type, MemberType.Method, MemberFlags.All, item.Value.setters, metadata) as IMethodMemberInfo;

                if (getter != null || setter != null)
                    _members.Add(new MethodAccessorMemberInfo(name, getter, setter, item.Value.args, item.Value.flags, type, _observationManager));
            }

            _membersDictionary.Clear();
            Components.TryAddMembers(memberManager, _members, type, name, memberTypes, metadata);
            if (_members.Count == 1)
            {
                var memberInfo = _members[0];
                _members.Clear();
                return ItemOrList.FromItem(memberInfo);
            }

            var result = _members.ToArray();
            _members.Clear();
            return ItemOrList.FromListToReadOnly(result);
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct MemberKey
        {
            #region Fields

            public readonly IReadOnlyList<IParameterInfo> Parameters;

            public readonly Type ReturnType;
            public readonly bool Setter;
            public readonly Type Type;

            #endregion

            #region Constructors

            public MemberKey(Type type, Type returnType, IReadOnlyList<IParameterInfo> parameters, bool setter)
            {
                Parameters = parameters;
                Type = type;
                ReturnType = returnType;
                Setter = setter;
            }

            #endregion

            #region Properties

            public int ParametersCount => Setter ? Parameters.Count - 1 : Parameters.Count;

            #endregion
        }

        #endregion
    }
}