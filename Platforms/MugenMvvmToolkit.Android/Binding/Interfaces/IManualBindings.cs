#region Copyright

// ****************************************************************************
// <copyright file="IManualBindings.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
{
    public interface IManualBindings
    {
        [NotNull]
        IList<IDataBinding> SetBindings([NotNull] string bind);
    }
}
