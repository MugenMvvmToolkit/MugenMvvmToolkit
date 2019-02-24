using System;

namespace MugenMvvm.Interfaces
{
    public interface IWeakReferenceFactory
    {
        WeakReference CreateWeakReference(object item);//todo add light WeakReferenceDescriptor?
    }
}