#region Copyright
// ****************************************************************************
// <copyright file="InvalidDataBinding.cs">
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
using MugenMvvmToolkit.Binding.Accessors;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Sources;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Core
{
    /// <summary>
    ///     Represents the invalid data binding.
    /// </summary>
    public sealed class InvalidDataBinding : DataBinding
    {
        #region Fields

        private static readonly ISingleBindingSourceAccessor SourceAccessorStatic;
        private readonly Exception _exception;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InvalidDataBinding" /> class.
        /// </summary>
        static InvalidDataBinding()
        {
            SourceAccessorStatic =
                new BindingSourceAccessor(new BindingSource(new EmptyPathObserver(new object(), BindingPath.Empty)),
                    DataContext.Empty, false);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataBinding" /> class.
        /// </summary>
        public InvalidDataBinding(Exception exception)
            : base(SourceAccessorStatic, SourceAccessorStatic)
        {
            _exception = exception;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the binding exception.
        /// </summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        #endregion

        #region Overrides of DataBinding

        /// <summary>
        ///     Sends the current value back to the source.
        /// </summary>
        public override bool UpdateSource()
        {
            RaiseBindingException(_exception, _exception, BindingAction.UpdateSource);
            return false;
        }

        /// <summary>
        ///     Forces a data transfer from source to target.
        /// </summary>
        public override bool UpdateTarget()
        {
            RaiseBindingException(_exception, _exception, BindingAction.UpdateTarget);
            return false;
        }

        /// <summary>
        ///     Validates the current binding and raises the BindingException event if needed.
        /// </summary>
        public override bool Validate()
        {
            RaiseBindingException(_exception, _exception, BindingAction.UpdateTarget);
            return false;
        }

        #endregion
    }
}