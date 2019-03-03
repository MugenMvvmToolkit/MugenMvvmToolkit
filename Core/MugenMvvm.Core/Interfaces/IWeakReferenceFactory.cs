using System;

namespace MugenMvvm.Interfaces
{
    public interface IWeakReferenceFactory
    {
        WeakReference GetWeakReference(object item);//todo add light WeakReferenceDescriptor?
    }
}