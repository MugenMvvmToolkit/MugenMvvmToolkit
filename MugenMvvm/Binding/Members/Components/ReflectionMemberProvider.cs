using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members.Components
{
    public sealed class ReflectionMemberProvider : IMemberProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IObservationManager? _observationManager;
        private readonly HashSet<Type> _types;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionMemberProvider(IObservationManager? observationManager = null)
        {
            _observationManager = observationManager;
            _types = new HashSet<Type>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = MemberComponentPriority.Instance;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> TryGetMembers(IMemberManager memberManager, Type type, string name, MemberType memberTypes, IReadOnlyMetadataContext? metadata)
        {
            _types.Clear();
            if (type == typeof(string) && name == BindingInternalConstant.IndexerGetterName)
                name = BindingInternalConstant.IndexerStringGetterName;
            var hasProperty = !memberTypes.HasFlagEx(MemberType.Accessor);
            var hasField = hasProperty;
            var hasEvent = !memberTypes.HasFlagEx(MemberType.Event);
            ItemOrList<IMemberInfo, List<IMemberInfo>> result = default;
            var types = MugenBindingExtensions.SelfAndBaseTypes(type, types: _types);
            foreach (var t in types)
            {
                if (!hasProperty)
                    hasProperty = AddProperties(type, t, name, ref result);

                if (!hasEvent)
                    hasEvent = AddEvents(type, t, name, ref result, metadata);

                if (!hasField)
                    hasField = AddFields(type, t, name, ref result);

                if (hasEvent && hasField && hasProperty)
                    break;
            }

            if (memberTypes.HasFlagEx(MemberType.Method))
            {
                types.Clear();
                foreach (var t in MugenBindingExtensions.SelfAndBaseTypes(type, false, types: types))
                {
                    var methods = t.GetMethods(BindingFlagsEx.All);
                    for (var index = 0; index < methods.Length; index++)
                    {
                        var methodInfo = methods[index];
                        if (methodInfo.Name == name)
                            result.Add(new MethodMemberInfo(name, methodInfo, false, type));
                    }
                }
            }

            return result.Cast<IReadOnlyList<IMemberInfo>>();
        }

        #endregion

        #region Methods

        private bool AddEvents(Type requestedType, Type t, string name, ref ItemOrList<IMemberInfo, List<IMemberInfo>> result, IReadOnlyMetadataContext? metadata)
        {
            var eventInfo = t.GetEvent(name, BindingFlagsEx.All);
            if (eventInfo == null)
                return false;

            var memberObserver = _observationManager.DefaultIfNull().TryGetMemberObserver(requestedType, eventInfo, metadata);
            if (memberObserver.IsEmpty)
                return false;

            result.Add(new EventMemberInfo(name, eventInfo, memberObserver));
            return true;
        }

        private bool AddFields(Type requestedType, Type t, string name, ref ItemOrList<IMemberInfo, List<IMemberInfo>> result)
        {
            var field = t.GetField(name, BindingFlagsEx.All);
            if (field == null)
                return false;

            result.Add(new FieldAccessorMemberInfo(name, field, requestedType));
            return true;
        }

        private bool AddProperties(Type requestedType, Type t, string name, ref ItemOrList<IMemberInfo, List<IMemberInfo>> result)
        {
            var property = t.GetProperty(name, BindingFlagsEx.All);
            if (property == null)
                return false;

            result.Add(new PropertyAccessorMemberInfo(name, property, requestedType));
            return true;
        }

        #endregion
    }
}