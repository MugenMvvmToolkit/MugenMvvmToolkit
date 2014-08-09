#region Copyright
// ****************************************************************************
// <copyright file="NoneBindingMode.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Updates the binding source only when you call the <see cref="IDataBinding.UpdateSource" /> method.
    /// </summary>
    public sealed class NoneBindingMode : IBindingBehavior
    {
        #region Fields

        /// <summary>
        ///     Gets the instance of <see cref="NoneBindingMode" /> use this instance as prototype.
        /// </summary>
        public static readonly NoneBindingMode Instance = new NoneBindingMode();

        #endregion

        #region Constructors

        private NoneBindingMode()
        {
        }

        #endregion

        #region Implementation of IBindingBehavior

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public Guid Id
        {
            get { return BindingModeBase.IdBindingMode; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public int Priority
        {
            get { return int.MinValue; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        /// <param name="binding">The binding to attach to.</param>
        bool IBindingBehavior.Attach(IDataBinding binding)
        {
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        void IBindingBehavior.Detach(IDataBinding binding)
        {
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}