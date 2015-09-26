#region Copyright

// ****************************************************************************
// <copyright file="WeakEventListenerWrapper.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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

        private readonly object _item;

        #endregion

        #region Constructors

        static WeakEventListenerWrapper()
        {
            Empty = default(WeakEventListenerWrapper);
        }

        public WeakEventListenerWrapper(IEventListener listener)
        {
            if (listener.IsWeak)
                _item = listener;
            else
                _item = ToolkitExtensions.GetWeakReference(listener);
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return _item == null; }
        }

        public object Source
        {
            get { return _item; }
        }

        [NotNull]
        public IEventListener EventListener
        {
            get
            {
                var listener = _item as IEventListener;
                if (listener != null)
                    return listener;
                if (_item == null)
                    return BindingExtensions.EmptyListener;
                return ((WeakReference)_item).Target as IEventListener ?? BindingExtensions.EmptyListener;
            }
        }

        #endregion
    }
}
