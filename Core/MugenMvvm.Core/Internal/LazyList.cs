﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct LazyList<T>
    {
        #region Fields

        public List<T>? List;

        #endregion

        #region Properties

        public readonly int Count => List?.Count ?? 0;

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> Get()
        {
            return List ??= new List<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator List<T>?(LazyList<T> list)
        {
            return list.List;
        }

        #endregion
    }
}