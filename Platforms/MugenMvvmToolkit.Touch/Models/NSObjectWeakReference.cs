#region Copyright
// ****************************************************************************
// <copyright file="NSObjectWeakReference.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MonoTouch.Foundation;

namespace MugenMvvmToolkit.Models
{
    internal class NSObjectWeakReference : WeakReference
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.WeakReference" /> class, referencing the specified object and
        ///     using the specified resurrection tracking.
        /// </summary>
        /// <param name="target">An object to track. </param>
        /// <param name="trackResurrection">
        ///     Indicates when to stop tracking the object. If true, the object is tracked after
        ///     finalization; if false, the object is only tracked until finalization.
        /// </param>
        public NSObjectWeakReference(NSObject target, bool trackResurrection)
            : base(target, trackResurrection)
        {
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
            get { return Target != null; }
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
            get
            {
                var target = base.Target as NSObject;
                if (target != null)
                {
                    if (target.Handle == IntPtr.Zero)
                    {
                        base.Target = null;
                        return null;
                    }
                    return target;
                }
                return null;
            }
            set { base.Target = value; }
        }

        #endregion
    }
}