using System;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public sealed class WeakReferenceImpl : WeakReference, IWeakReference
    {
        #region Fields

        public static readonly IWeakReference Empty = new WeakReferenceImpl(null, false);

        #endregion

        #region Constructors

        public WeakReferenceImpl(object? target, bool trackResurrection) : base(target, trackResurrection)
        {
        }

        #endregion

        #region Implementation of interfaces

        public void Release() => Target = null;

        #endregion
    }
}