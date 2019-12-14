using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Internal;

namespace MugenMvvm.Serialization
{
    [Serializable]
    [DataContract(Namespace = BuildConstant.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public sealed class StaticMemberMemento : IMemento
    {
        #region Fields

        [DataMember(Name = "M")]
        internal MemberInfo? Member;

        [DataMember(Name = "T")]
        internal Type TargetTypeField;

        #endregion

        #region Constructors

#pragma warning disable CS8618
        internal StaticMemberMemento()
        {
        }
#pragma warning restore CS8618

        private StaticMemberMemento(object target, MemberInfo member)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(member, nameof(member));
            TargetTypeField = target.GetType();
            Member = member;
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        [XmlIgnore]
        public Type TargetType => TargetTypeField;

        #endregion

        #region Implementation of interfaces

        public void Preserve(ISerializationContext serializationContext)
        {
        }

        public IMementoResult Restore(ISerializationContext serializationContext)
        {
            if (Member == null)
                return MementoResult.Unrestored;

            object target;
            if (Member is PropertyInfo propertyInfo)
                target = propertyInfo.GetValue(null);
            else
                target = ((FieldInfo) Member).GetValue(null);
            if (target == null)
                return MementoResult.Unrestored;
            return new MementoResult(target, serializationContext);
        }

        #endregion

        #region Methods

        public static StaticMemberMemento? Create(object target, Type type, string fieldOrPropertyName)
        {
            var member = type.GetField(fieldOrPropertyName, BindingFlagsEx.StaticOnly) ??
                         (MemberInfo?) type.GetProperty(fieldOrPropertyName, BindingFlagsEx.StaticOnly);
            if (member == null)
            {
                Tracer.Error()?.Trace(MessageConstant.FieldOrPropertyNotFoundFormat2, fieldOrPropertyName, type);
                return null;
            }

            return new StaticMemberMemento(target, member);
        }

        #endregion
    }
}