#region Copyright
// ****************************************************************************
// <copyright file="BindingResourceMethod.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the binding expression method that can be used in multi binding expression.
    /// </summary>
    public class BindingResourceMethod : IBindingResourceMethod
    {
        #region Fields

        private readonly Func<IList<Type>, IList<Type>, IDataContext, Type> _getReturnType;
        private readonly Func<IList<Type>, object[], IDataContext, object> _method;
        private readonly Type _returnType;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceMethod" /> class.
        /// </summary>
        public BindingResourceMethod([NotNull] Func<IList<Type>, object[], IDataContext, object> method,
            [NotNull] Type returnType)
        {
            Should.NotBeNull(method, "method");
            Should.NotBeNull(returnType, "returnType");
            _method = method;
            _returnType = returnType;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceMethod" /> class.
        /// </summary>
        public BindingResourceMethod([NotNull] Func<IList<Type>, object[], IDataContext, object> method,
            [NotNull] Func<IList<Type>, IList<Type>, IDataContext, Type> getReturnType)
        {
            Should.NotBeNull(method, "method");
            Should.NotBeNull(getReturnType, "getReturnType");
            _method = method;
            _getReturnType = getReturnType;
        }

        #endregion

        #region Implementation of IBindingResourceMethod

        /// <summary>
        ///     Gets the return type of method.
        /// </summary>
        public Type GetReturnType(IList<Type> parameters, IList<Type> typeArgs, IDataContext context)
        {
            if (_getReturnType == null)
                return _returnType;
            return _getReturnType(parameters, typeArgs, context);
        }

        /// <summary>
        ///     Invokes the method
        /// </summary>
        public object Invoke(IList<Type> typeArgs, object[] args, IDataContext context)
        {
            return _method(typeArgs, args, context);
        }

        #endregion
    }
}