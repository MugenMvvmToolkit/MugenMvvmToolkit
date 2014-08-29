#region Copyright
// ****************************************************************************
// <copyright file="ValidatesOnExceptionsBehavior.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Represents the binding behavior that allows to validate target on a binding exception.
    /// </summary>
    public sealed class ValidatesOnExceptionsBehavior : IBindingBehavior
    {
        #region Fields

        /// <summary>
        ///     Gets the id of behavior.
        /// </summary>
        public static readonly Guid IdValidatesOnExceptionsBehavior;

        /// <summary>
        ///     Gets the instance of a <see cref="ValidatesOnExceptionsBehavior" /> class.
        /// </summary>
        public static readonly ValidatesOnExceptionsBehavior Instance;

        private static readonly SenderType ErrorsConstant;

        #endregion

        #region Constructors

        static ValidatesOnExceptionsBehavior()
        {
            IdValidatesOnExceptionsBehavior = new Guid("046EC76A-0DC9-4024-B893-7E2AF9E4F636");
            ErrorsConstant = new SenderType("VEB.ErrorsConstant");
            ShowOriginalException = true;
            Instance = new ValidatesOnExceptionsBehavior();
        }

        private ValidatesOnExceptionsBehavior()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value that is responsible for the selection exception for the display.
        /// </summary>
        public static bool ShowOriginalException { get; set; }

        #endregion

        #region Implementation of IBindingBehavior

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public Guid Id
        {
            get { return IdValidatesOnExceptionsBehavior; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public int Priority
        {
            get { return 0; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        /// <param name="binding">The binding to attach to.</param>
        public bool Attach(IDataBinding binding)
        {
            var bindingTarget = binding.TargetAccessor.Source as IBindingTarget;
            if (bindingTarget == null || !bindingTarget.Validatable)
                return false;
            binding.BindingException += OnBindingException;
            binding.BindingUpdated += OnBindingUpdated;
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        public void Detach(IDataBinding binding)
        {
            binding.BindingException -= OnBindingException;
            binding.BindingUpdated -= OnBindingUpdated;
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion

        #region Methods

        private static void OnBindingException(IDataBinding sender, BindingExceptionEventArgs args)
        {
            var validatableBindingTarget = (IBindingTarget)sender.TargetAccessor.Source;
            validatableBindingTarget.SetErrors(ErrorsConstant,
                new object[] { ShowOriginalException ? args.OriginalException.Message : args.Exception.Message });
        }

        private static void OnBindingUpdated(IDataBinding sender, BindingEventArgs bindingEventArgs)
        {
            var validatableBindingTarget = (IBindingTarget)sender.TargetAccessor.Source;
            validatableBindingTarget.SetErrors(ErrorsConstant, null);
        }

        #endregion
    }
}