#region Copyright

// ****************************************************************************
// <copyright file="ValidatesOnExceptionsBehavior.cs">
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
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Represents the binding behavior that allows to validate target on a binding exception.
    /// </summary>
    public sealed class ValidatesOnExceptionsBehavior : IBindingBehavior
    {
        #region Fields

        private const string Key = "@@#bexc.";

        /// <summary>
        ///     Gets the id of behavior.
        /// </summary>
        public static readonly Guid IdValidatesOnExceptionsBehavior;

        /// <summary>
        ///     Gets the instance of a <see cref="ValidatesOnExceptionsBehavior" /> class.
        /// </summary>
        public static readonly ValidatesOnExceptionsBehavior Instance;

        private static readonly EventHandler<IDataBinding, BindingEventArgs> BindingUpdatedDelegate;
        private static readonly EventHandler<IDataBinding, BindingExceptionEventArgs> BindingExceptionDelegate;

        #endregion

        #region Constructors

        static ValidatesOnExceptionsBehavior()
        {
            IdValidatesOnExceptionsBehavior = new Guid("046EC76A-0DC9-4024-B893-7E2AF9E4F636");
            ShowOriginalException = true;
            Instance = new ValidatesOnExceptionsBehavior();
            BindingUpdatedDelegate = OnBindingUpdated;
            BindingExceptionDelegate = OnBindingException;
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
            if (BindingServiceProvider.ErrorProvider == null)
                return false;
            binding.BindingException += BindingExceptionDelegate;
            binding.BindingUpdated += BindingUpdatedDelegate;
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        public void Detach(IDataBinding binding)
        {
            binding.BindingException -= BindingExceptionDelegate;
            binding.BindingUpdated -= BindingUpdatedDelegate;
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return;

            var context = new DataContext(binding.Context);
            context.AddOrUpdate(BindingErrorProviderBase.ClearErrorsConstant, true);
            SetErrors(errorProvider, binding, Empty.Array<object>(), context);
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
            IBindingErrorProvider errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider != null)
                SetErrors(errorProvider, sender,
                    new object[] { ShowOriginalException ? args.OriginalException.Message : args.Exception.Message }, null);
        }

        private static void OnBindingUpdated(IDataBinding sender, BindingEventArgs args)
        {
            IBindingErrorProvider errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider != null)
                SetErrors(errorProvider, sender, Empty.Array<object>(), null);
        }

        private static void SetErrors(IBindingErrorProvider errorProvider, IDataBinding sender, object[] errors, IDataContext context)
        {
            IBindingPathMembers pathMembers = sender.TargetAccessor.Source.GetPathMembers(false);
            object target = pathMembers.PenultimateValue;
            if (target != null && !target.IsUnsetValue())
                errorProvider.SetErrors(target, Key + pathMembers.Path.Path, errors, context ?? sender.Context);
        }

        #endregion
    }
}