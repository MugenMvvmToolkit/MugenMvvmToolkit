#region Copyright

// ****************************************************************************
// <copyright file="BindingResourceMethod.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    public class BindingResourceMethod : IBindingResourceMethod
    {
        #region Fields

        private readonly Func<IList<Type>, IList<Type>, IDataContext, Type> _getReturnType;
        private readonly Func<IList<Type>, object[], IDataContext, object> _method;
        private readonly Type _returnType;

        #endregion

        #region Constructors

        public BindingResourceMethod([NotNull] Func<IList<Type>, object[], IDataContext, object> method,
            [NotNull] Type returnType)
        {
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(returnType, nameof(returnType));
            _method = method;
            _returnType = returnType;
        }

        public BindingResourceMethod([NotNull] Func<IList<Type>, object[], IDataContext, object> method,
            [NotNull] Func<IList<Type>, IList<Type>, IDataContext, Type> getReturnType)
        {
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(getReturnType, nameof(getReturnType));
            _method = method;
            _getReturnType = getReturnType;
        }

        #endregion

        #region Implementation of IBindingResourceMethod

        public Type GetReturnType(IList<Type> parameters, IList<Type> typeArgs, IDataContext context)
        {
            if (_getReturnType == null)
                return _returnType;
            return _getReturnType(parameters, typeArgs, context);
        }

        public object Invoke(IList<Type> typeArgs, object[] args, IDataContext context)
        {
            return _method(typeArgs, args, context);
        }

        #endregion
    }
}
