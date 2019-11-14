using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    public sealed class ArrayMemberAccessorInfo : IMemberAccessorInfo//todo indexer values
    {
        #region Fields

        private readonly int[] _indexes;

        #endregion

        #region Constructors

        public ArrayMemberAccessorInfo(string name, Type arrayType, int[] indexes)
        {
            Should.NotBeNull(indexes, nameof(indexes));
            _indexes = indexes;
            Name = name;
            DeclaringType = arrayType;
            Type = arrayType.GetElementType();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type DeclaringType { get; }

        public Type Type { get; }

        public object? UnderlyingMember => null;

        public MemberType MemberType => MemberType.Property;

        public MemberFlags AccessModifiers => MemberFlags.InstancePublic;

        public bool CanRead => true;

        public bool CanWrite => true;

        #endregion

        #region Implementation of interfaces

        public object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            return ((Array)target!).GetValue(_indexes);
        }

        public void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(target, nameof(target));
            ((Array)target!).SetValue(value, _indexes);
        }

        public ActionToken TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return default;
        }

        #endregion
    }
}