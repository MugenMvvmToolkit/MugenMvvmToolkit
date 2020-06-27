using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.UnitTest.Serialization.Internal
{
    public class TestSerializerComponent : ISerializerComponent, IHasPriority
    {
        #region Properties

        public Func<object, Type, IReadOnlyMetadataContext?, bool>? CanSerialize { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, string?>? TrySerialize { get; set; }

        public Func<string, IReadOnlyMetadataContext?, object?>? TryDeserialize { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISerializerComponent.CanSerialize<TTarget>([DisallowNull]in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            return CanSerialize?.Invoke(target, typeof(TTarget), metadata) ?? false;
        }

        string? ISerializerComponent.TrySerialize<TTarget>([DisallowNull]in TTarget target, IReadOnlyMetadataContext? metadata)
        {
            return TrySerialize?.Invoke(target, typeof(TTarget), metadata);
        }

        bool ISerializerComponent.TryDeserialize(string data, IReadOnlyMetadataContext? metadata, out object? value)
        {
            value = TryDeserialize?.Invoke(data, metadata);
            return value != null;
        }

        #endregion
    }
}