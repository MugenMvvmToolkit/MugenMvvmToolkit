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
        [DataMember(Name = "M")]
        internal MemberInfo? Member;

#pragma warning disable CS8618
        internal StaticMemberMemento()
        {
        }
#pragma warning restore CS8618

        private StaticMemberMemento(object target, MemberInfo member)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(member, nameof(member));
            TargetType = target.GetType();
            Member = member;
        }

        [IgnoreDataMember]
        [XmlIgnore]
        [field: DataMember(Name = "T")]
        public Type TargetType { get; internal set; }

        public static StaticMemberMemento? Create(object target, Type type, string fieldOrPropertyName)
        {
            var member = type.GetField(fieldOrPropertyName, BindingFlagsEx.StaticOnly) ??
                         (MemberInfo?) type.GetProperty(fieldOrPropertyName, BindingFlagsEx.StaticOnly);
            if (member == null)
            {
                Logger.Error()?.Log(MessageConstant.FieldOrPropertyNotFoundFormat2, fieldOrPropertyName, type);
                return null;
            }

            return new StaticMemberMemento(target, member);
        }

        public void Preserve(ISerializationContext serializationContext)
        {
        }

        public MementoResult Restore(ISerializationContext serializationContext)
        {
            if (Member == null)
                return default;

            object? target;
            if (Member is PropertyInfo propertyInfo)
                target = propertyInfo.GetValue(null);
            else
                target = ((FieldInfo) Member).GetValue(null);
            if (target == null)
                return default;
            return new MementoResult(target);
        }
    }
}