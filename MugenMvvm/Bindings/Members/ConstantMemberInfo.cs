using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Members
{
    public sealed class ConstantMemberInfo : IAccessorMemberInfo
    {
        private static readonly object TargetObj = new();
        public static readonly ConstantMemberInfo Target = new("", TargetObj, false);
        public static readonly ConstantMemberInfo Null = new("", null, false);
        public static readonly ConstantMemberInfo Unset = new("", BindingMetadata.UnsetValue, false);

        private readonly object? _result;

        public ConstantMemberInfo(string name, object? result, bool canWrite)
        {
            Should.NotBeNull(name, nameof(name));
            _result = result;
            Name = name;
            CanWrite = canWrite;
        }

        public bool CanRead => true;

        public bool CanWrite { get; }

        public string Name { get; }

        public Type DeclaringType => typeof(object);

        public Type Type => typeof(object);

        public object? UnderlyingMember => null;

        public MemberType MemberType => MemberType.Accessor;

        public EnumFlags<MemberFlags> AccessModifiers => MemberFlags.Public | MemberFlags.Dynamic;

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null) => TargetObj == _result ? target : _result;

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (!CanWrite)
                ExceptionManager.ThrowBindingMemberMustBeWritable(this);
        }

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null) => default;
    }
}