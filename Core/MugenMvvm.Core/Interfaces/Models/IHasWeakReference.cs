using System;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasWeakReference
    {
        WeakReference WeakReference { get; }
    }

    internal interface IHasWeakReferenceInternal : IHasWeakReference
    {
    }
}