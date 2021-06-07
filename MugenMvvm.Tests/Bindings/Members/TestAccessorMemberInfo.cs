using System;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Tests.Bindings.Members
{
    public class TestAccessorMemberInfo : TestMemberInfoBase, IAccessorMemberInfo
    {
        public Func<object?, IReadOnlyMetadataContext?, object?>? GetValue { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? SetValue { get; set; }

        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        object? IAccessorMemberInfo.GetValue(object? target, IReadOnlyMetadataContext? metadata) => GetValue?.Invoke(target, metadata);

        void IAccessorMemberInfo.SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata) => SetValue?.Invoke(target, value, metadata);
    }
}