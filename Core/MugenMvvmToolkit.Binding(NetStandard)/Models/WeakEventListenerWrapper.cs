#region Copyright

// ****************************************************************************
// <copyright file="WeakEventListenerWrapper.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    [StructLayout(LayoutKind.Auto)]
    public struct WeakEventListenerWrapper
    {
        #region Fields

        public static readonly WeakEventListenerWrapper Empty;
        private object _item;
        private bool _isWeak;

        #endregion

        #region Constructors

        static WeakEventListenerWrapper()
        {
            Empty = default(WeakEventListenerWrapper);
        }

        public WeakEventListenerWrapper(IEventListener listener)
        {
            _isWeak = listener.IsWeak;
            if (_isWeak)
                _item = listener;
            else
                _item = ToolkitExtensions.GetWeakReference(listener);
        }

        #endregion

        #region Properties

        public bool IsEmpty => _item == null;

        public object Source => _item;

        [NotNull]
        public IEventListener EventListener
        {
#if NET_STANDARD
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            get
            {
                if (_isWeak)
                    return (IEventListener)_item;
                if (_item == null)
                    return BindingExtensions.EmptyListener;
                return (IEventListener)((WeakReference)_item).Target ?? BindingExtensions.EmptyListener;
            }
        }

        #endregion
    }
}
