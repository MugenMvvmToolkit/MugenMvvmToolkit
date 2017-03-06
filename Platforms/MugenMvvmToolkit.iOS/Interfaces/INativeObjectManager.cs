#region Copyright

// ****************************************************************************
// <copyright file="INativeObjectManager.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.iOS.Interfaces
{
    public interface INativeObjectManager
    {
        void Initialize([CanBeNull] object item, [CanBeNull] IDataContext context);

        void Dispose([CanBeNull] object item, [CanBeNull] IDataContext context);
    }
}