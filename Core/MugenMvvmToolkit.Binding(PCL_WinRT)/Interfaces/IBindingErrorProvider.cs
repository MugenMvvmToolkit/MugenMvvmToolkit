#region Copyright

// ****************************************************************************
// <copyright file="IBindingErrorProvider.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindingErrorProvider
    {
        [NotNull]
        IList<object> GetErrors([NotNull]object target, string key, [CanBeNull] IDataContext context);

        void SetErrors([NotNull]object target, [NotNull] string senderKey, [NotNull] IList<object> errors,
            [CanBeNull] IDataContext context);
    }
}
