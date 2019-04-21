using System;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IWeakReferenceFactory//todo provider
    {
        WeakReference GetWeakReference(object item); //todo add light WeakReferenceDescriptor?
    }
}