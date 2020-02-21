using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class TestMemberAccessorInfo : IMemberAccessorInfo
    {
        #region Properties

        public string Name { get; set; } = default!;

        public Type DeclaringType { get; set; } = default!;

        public Type Type { get; set; } = default!;

        public object? UnderlyingMember { get; set; } = default!;

        public MemberType MemberType { get; set; } = default!;

        public MemberFlags AccessModifiers { get; set; } = default!;

        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        public Func<object?, IEventListener, IReadOnlyMetadataContext?, ActionToken>? TryObserve { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, object?>? GetValue { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? SetValue { get; set; }

        #endregion

        #region Implementation of interfaces

        ActionToken IObservableMemberInfo.TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata)
        {
            return TryObserve?.Invoke(target, listener, metadata) ?? default;
        }

        object? IMemberAccessorInfo.GetValue(object? target, IReadOnlyMetadataContext? metadata)
        {
            return GetValue?.Invoke(target, metadata);
        }

        void IMemberAccessorInfo.SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata)
        {
            SetValue?.Invoke(target, value, metadata);
        }

        #endregion
    }
}