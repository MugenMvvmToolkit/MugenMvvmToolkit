using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Infrastructure.Internal
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
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

        internal StaticMemberMemento()
        {
        }

        private StaticMemberMemento(object target, MemberInfo member)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(member, nameof(member));
            TargetTypeField = target.GetType();
            Member = member;
        }

        #endregion

        #region Properties

        [IgnoreDataMember, XmlIgnore]
        public Type TargetType => TargetTypeField;

        #endregion

        #region Implementation of interfaces

        public void Preserve(ISerializationContext serializationContext)
        {
        }

        public IMementoResult Restore(ISerializationContext serializationContext)
        {
            object target;
            if (Member is PropertyInfo propertyInfo)
                target = propertyInfo.GetValue(null);
            else
                target = ((FieldInfo)Member).GetValue(null);
            if (target == null)
                return MementoResult.Unrestored;
            return new MementoResult(target, serializationContext);
        }

        #endregion

        #region Methods

        public static StaticMemberMemento? Create(object target, Type type, string fieldOrPropertyName)
        {
            MemberInfo member = type.GetFieldUnified(fieldOrPropertyName, MemberFlags.StaticOnly);
            if (member == null)
                member = type.GetPropertyUnified(fieldOrPropertyName, MemberFlags.StaticOnly);
            if (member == null)
            {
                Tracer.Error(MessageConstants.FieldOrPropertyNotFoundFormat2, fieldOrPropertyName, type);
                return null;
            }
            return new StaticMemberMemento(target, member);
        }

        #endregion
    }
}