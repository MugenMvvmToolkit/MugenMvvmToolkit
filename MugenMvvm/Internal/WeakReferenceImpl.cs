using System;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public sealed class WeakReferenceImpl : WeakReference, IWeakReference
    {
        public static readonly IWeakReference Empty = new WeakReferenceImpl(null, false);

        public WeakReferenceImpl(object? target, bool trackResurrection) : base(target, trackResurrection)
        {
        }

        public void Release() => Target = null;
    }
}