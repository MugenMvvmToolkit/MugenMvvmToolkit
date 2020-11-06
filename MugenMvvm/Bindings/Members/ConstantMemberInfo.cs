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
        #region Fields

        private readonly object? _result;

        private static readonly object TargetObj = new object();

        public static readonly ConstantMemberInfo Target = new ConstantMemberInfo("", TargetObj, false);
        public static readonly ConstantMemberInfo Null = new ConstantMemberInfo("", null, false);
        public static readonly ConstantMemberInfo Unset = new ConstantMemberInfo("", BindingMetadata.UnsetValue, false);

        public static readonly IMemberInfo[] NullArray = {Null};
        public static readonly IMemberInfo[] TargetArray = {Target};
        public static readonly IMemberInfo[] UnsetArray = {Unset};

        #endregion

        #region Constructors

        public ConstantMemberInfo(string name, object? result, bool canWrite)
        {
            Should.NotBeNull(name, nameof(name));
            _result = result;
            Name = name;
            CanWrite = canWrite;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType => typeof(object);

        public Type Type => typeof(object);

        public object? UnderlyingMember => null;

        public MemberType MemberType => MemberType.Accessor;

        public EnumFlags<MemberFlags> AccessModifiers => MemberFlags.Public | MemberFlags.Dynamic;

        public bool CanRead => true;

        public bool CanWrite { get; }

        #endregion

        #region Implementation of interfaces

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null) => default;

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null) => TargetObj == _result ? target : _result;

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            if (!CanWrite)
                ExceptionManager.ThrowBindingMemberMustBeWritable(this);
        }

        #endregion
    }
}