#region Copyright

// ****************************************************************************
// <copyright file="ISourceValue.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    public interface ISourceValue
    {
        bool IsAlive { get; }

        [CanBeNull]
        object Value { get; }

        [Preserve]
        event EventHandler<ISourceValue, EventArgs> ValueChanged;
    }
}
