#region Copyright

// ****************************************************************************
// <copyright file="IBindingResourceMethod.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    public interface IBindingResourceMethod
    {
        [NotNull]
        Type GetReturnType(IList<Type> parameters, [NotNull] IList<Type> typeArgs, [NotNull] IDataContext context);

        [CanBeNull]
        object Invoke([NotNull] IList<Type> typeArgs, object[] args, [NotNull] IDataContext context);
    }
}
