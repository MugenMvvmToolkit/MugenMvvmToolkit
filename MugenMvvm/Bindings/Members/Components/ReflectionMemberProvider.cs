using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Members.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Members.Components
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

        public ItemOrIReadOnlyList<IMemberInfo> TryGetMembers(IMemberManager memberManager, Type type, string name, EnumFlags<MemberType> memberTypes, IReadOnlyMetadataContext? metadata)
        {
            _types.Clear();
            var hasProperty = !memberTypes.HasFlag(MemberType.Accessor);
            var hasField = hasProperty;
            var hasEvent = !memberTypes.HasFlag(MemberType.Event);
            var result = new ItemOrListEditor<IMemberInfo>();
            var types = BindingMugenExtensions.SelfAndBaseTypes(type, types: _types);
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

            if (memberTypes.HasFlag(MemberType.Method))
            {
                types.Clear();
                bool isGetter = name == BindingInternalConstant.IndexerGetterName;
                var isSetter = name == BindingInternalConstant.IndexerSetterName;
                foreach (var t in BindingMugenExtensions.SelfAndBaseTypes(type, false, types: types))
                {
                    if (isGetter || isSetter)
                    {
                        var propertyInfos = t.GetProperties(BindingFlagsEx.All);
                        for (int i = 0; i < propertyInfos.Length; i++)
                        {
                            var propertyInfo = propertyInfos[i];
                            var indexParameters = propertyInfo.GetIndexParameters();
                            if (indexParameters.Length > 0)
                            {
                                var method = isGetter ? propertyInfo.GetGetMethod(true) : propertyInfo.GetSetMethod(true);
                                if (method != null)
                                    result.Add(new MethodMemberInfo(name, method, false, type, isGetter ? indexParameters : null, null));
                            }
                        }
                    }
                    else
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
            }

            return result.ToItemOrList();
        }

        #endregion

        #region Methods

        private bool AddEvents(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result, IReadOnlyMetadataContext? metadata)
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

        private static bool AddFields(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result)
        {
            var field = t.GetField(name, BindingFlagsEx.All);
            if (field == null)
                return false;

            result.Add(new FieldAccessorMemberInfo(name, field, requestedType));
            return true;
        }

        private static bool AddProperties(Type requestedType, Type t, string name, ref ItemOrListEditor<IMemberInfo> result)
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