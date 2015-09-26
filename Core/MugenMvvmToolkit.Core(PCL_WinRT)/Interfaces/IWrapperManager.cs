#region Copyright

// ****************************************************************************
// <copyright file="IWrapperManager.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IWrapperManager
    {
        bool CanWrap([NotNull] Type type, [NotNull] Type wrapperType, [CanBeNull] IDataContext dataContext);

        [NotNull]
        object Wrap([NotNull] object item, [NotNull] Type wrapperType, [CanBeNull] IDataContext dataContext);
    }
}
