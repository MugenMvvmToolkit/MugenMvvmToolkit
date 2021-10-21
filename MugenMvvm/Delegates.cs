using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Delegates
{
    public delegate object? BoxingDelegate<T>(T? value);

    public delegate void UpdateGroupDelegate<in T, in TKey, in TGroup>(TKey key, TGroup group, IReadOnlyCollection<T> items, CollectionGroupChangedAction action, T? item,
        object? args);

    public delegate bool MappingPostConditionDelegate(IViewMapping mapping, Type requestedType, bool isViewMapping, object? target, IReadOnlyMetadataContext? metadata);
}