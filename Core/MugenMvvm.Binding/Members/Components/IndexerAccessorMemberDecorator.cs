using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Members.Components
{
    public class IndexerAccessorMemberDecorator : DecoratorComponentBase<IMemberManager, IMemberProviderComponent>, IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IGlobalValueConverter? _globalValueConverter;
        private readonly List<IMemberInfo> _members;
        private readonly MemberDictionary _membersDictionary;
        private readonly IObserverProvider? _observerProvider;

        #endregion

        #region Constructors

        public IndexerAccessorMemberDecorator(IGlobalValueConverter? globalValueConverter = null, IObserverProvider? observerProvider = null)
        {
            _globalValueConverter = globalValueConverter;
            _observerProvider = observerProvider;
            _members = new List<IMemberInfo>();
            _membersDictionary = new MemberDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.Decorator;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IMemberInfo>? TryGetMembers(Type type, string name, IReadOnlyMetadataContext? metadata)
        {
            var indexerArgsRaw = MugenBindingExtensions.GetIndexerArgsRaw(name);
            if (indexerArgsRaw == null)
                return Components.TryGetMembers(type, name, metadata);

            string getterName;
            string? setterName;
            if (type.IsArray)
            {
                getterName = "Get";
                setterName = "Set";
            }
            else if (type == typeof(string))
            {
                getterName = "get_Char";
                setterName = null;
            }
            else
            {
                getterName = "get_Item";
                setterName = "set_Item";
            }

            _membersDictionary.Clear();
            _members.Clear();
            Components.TryAddMembers(_members, type, getterName, metadata);
            for (var i = 0; i < _members.Count; i++)
            {
                if (!(_members[i] is IMethodInfo method) || method.Type == typeof(void))
                    continue;

                var parameters = method.GetParameters();
                var args = _globalValueConverter.TryGetInvokeArgs(parameters, indexerArgsRaw, metadata, out var isLastParameterMetadata);
                if (args == null || args.Length == 0)
                    continue;

                var key = new MemberKey(method.DeclaringType, method.Type, parameters, false);
                if (!_membersDictionary.TryGetValue(key, out var value))
                {
                    value = (new List<IMethodInfo>(), null, args, isLastParameterMetadata);
                    _membersDictionary[key] = value;
                }

                value.Item1!.Add(method);
            }

            if (setterName != null)
            {
                _members.Clear();
                Components.TryAddMembers(_members, type, setterName, metadata);

                for (var i = 0; i < _members.Count; i++)
                {
                    if (!(_members[i] is IMethodInfo method) || method.Type != typeof(void))
                        continue;

                    var parameters = method.GetParameters();
                    if (parameters.Count < 2)
                        continue;

                    var lastParameter = parameters[parameters.Count - 1];
                    var key = new MemberKey(method.DeclaringType, lastParameter.ParameterType, parameters, true);
                    if (!_membersDictionary.TryGetValue(key, out var value))
                    {
                        var args = _globalValueConverter.TryGetInvokeArgs(parameters.Take(parameters.Count - 1).ToList(), indexerArgsRaw, metadata, out var isLastParameterMetadata);
                        if (args == null || args.Length == 0)
                            continue;

                        value = (null, new List<IMethodInfo>(), args, isLastParameterMetadata);
                        _membersDictionary[key] = value;
                    }

                    if (value.Item2 == null)
                    {
                        value = (value.Item1, new List<IMethodInfo>(), value.Item3, value.Item4);
                        _membersDictionary[key] = value;
                    }

                    value.Item2.Add(method);
                }
            }


            var selectors = Owner.GetComponents<IMemberSelectorComponent>();
            foreach (var item in _membersDictionary)
            {
                IMethodInfo? getter = null, setter = null;
                if (item.Value.Item1 != null)
                    getter = selectors.TrySelectMembers(item.Value.Item1, type, MemberType.Method, MemberFlags.All, metadata).FirstOrDefault() as IMethodInfo;
                if (item.Value.Item2 != null)
                    setter = selectors.TrySelectMembers(item.Value.Item2, type, MemberType.Method, MemberFlags.All, metadata).FirstOrDefault() as IMethodInfo;

                if (getter != null || setter != null)
                    _members.Add(new MethodMemberAccessorInfo(name, getter, setter, item.Value.Item3, item.Value.Item4, type, _observerProvider));
            }

            Components.TryAddMembers(_members, type, name, metadata);
            _membersDictionary.Clear();
            var result = _members.ToArray();
            _members.Clear();
            return result;
        }

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public readonly struct MemberKey
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

        private sealed class MemberDictionary : LightDictionary<MemberKey, (List<IMethodInfo>?, List<IMethodInfo>?, object?[], bool)>
        {
            #region Constructors

            public MemberDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(MemberKey x, MemberKey y)
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

            protected override int GetHashCode(MemberKey key)
            {
                return HashCode.Combine(key.Type, key.ReturnType, key.ParametersCount);
            }

            #endregion
        }

        #endregion
    }
}