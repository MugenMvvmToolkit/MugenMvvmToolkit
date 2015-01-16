#region Copyright

// ****************************************************************************
// <copyright file="NativeObjectWeakReference.cs">
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
using Foundation;

namespace MugenMvvmToolkit.Models
{
    internal sealed class NativeObjectWeakReference : WeakReference
    {
        #region Fields

        private NativeReference _reference;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and
        ///     using the specified resurrection tracking.
        /// </summary>
        /// <param name="target">An object to track. </param>
        public NativeObjectWeakReference(NSObject target)
        {
            _reference = new NativeReference(target);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Overrides of WeakReference

        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="T:System.WeakReference" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="T:System.WeakReference" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        public override bool IsAlive
        {
            get { return _reference.IsAlive; }
        }

        /// <summary>
        ///     Gets or sets the object (the target) referenced by the current <see cref="T:System.WeakReference" /> object.
        /// </summary>
        /// <returns>
        ///     null if the object referenced by the current <see cref="T:System.WeakReference" /> object has been garbage
        ///     collected; otherwise, a reference to the object referenced by the current <see cref="T:System.WeakReference" />
        ///     object.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">
        ///     The reference to the target object is invalid. This exception can
        ///     be thrown while setting this property if the value is a null reference or if the object has been finalized during
        ///     the set operation.
        /// </exception>
        public override object Target
        {
            get { return _reference.Target; }
            set { _reference = value == null ? default(NativeReference) : new NativeReference((NSObject) value); }
        }

        #endregion
    }
}