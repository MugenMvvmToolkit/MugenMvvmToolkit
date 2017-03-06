#region Copyright

// ****************************************************************************
// <copyright file="IBindingPath.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    public interface IBindingPath
    {
        [NotNull]
        string Path { get; }

        [NotNull]
        IList<string> Parts { get; }

        bool IsEmpty { get; }

        bool IsSingle { get; }

        bool IsDebuggable { get; }

        string DebugTag { get; }
    }
}
