#region Copyright

// ****************************************************************************
// <copyright file="InvalidDataBinding.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public sealed class InvalidDataBinding : DataBinding
    {
        #region Fields

        private static readonly ISingleBindingSourceAccessor SourceAccessorStatic;
        private readonly Exception _exception;

        #endregion

        #region Constructors

        static InvalidDataBinding()
        {
            SourceAccessorStatic = new BindingSourceAccessor(new EmptyPathObserver(new object(), BindingPath.Empty), DataContext.Empty, false);
        }

        public InvalidDataBinding(Exception exception)
            : base(SourceAccessorStatic, SourceAccessorStatic)
        {
            _exception = exception;
        }

        #endregion

        #region Properties

        public Exception Exception => _exception;

        #endregion

        #region Overrides of DataBinding

        public override bool UpdateSource()
        {
            RaiseBindingException(_exception, _exception, BindingAction.UpdateSource);
            return false;
        }

        public override bool UpdateTarget()
        {
            RaiseBindingException(_exception, _exception, BindingAction.UpdateTarget);
            return false;
        }

        public override bool Validate()
        {
            RaiseBindingException(_exception, _exception, BindingAction.UpdateTarget);
            return false;
        }

        #endregion
    }
}
