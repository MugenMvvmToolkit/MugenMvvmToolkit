﻿using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingPathObserver : IDisposable
    {
        bool IsAlive { get; }

        IBindingPath Path { get; }

        object? Source { get; }

        void AddListener(IBindingPathObserverListener listener);

        void RemoveListener(IBindingPathObserverListener listener);

        BindingPathMembers GetMembers(IReadOnlyMetadataContext metadata);

        BindingPathLastMember GetLastMember(IReadOnlyMetadataContext metadata);
    }
}