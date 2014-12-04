#region Copyright
// ****************************************************************************
// <copyright file="JavaObjectWeakReference.cs">
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
using Object = Java.Lang.Object;
using WeakReference = System.WeakReference;

namespace MugenMvvmToolkit.Models
{
    //see https://bugzilla.xamarin.com/show_bug.cgi?id=16343
    internal sealed class JavaObjectWeakReference : WeakReference
    {
        #region Fields

        private readonly string _name;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="JavaObjectWeakReference" /> class.
        /// </summary>
        public JavaObjectWeakReference(Object item, bool trackResurrection)
            : base(item, trackResurrection)
        {
            _name = item.ToString();
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
                var target = base.Target as Object;
                if (target != null)
                {
                    if (target.IsAlive())
                        return target;
                    base.Target = null;
                    return null;
                }
                return null;
            }
            set { base.Target = value; }
        }

        #endregion
    }
}