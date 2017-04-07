#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgsBase.cs">
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

using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models.EventArg
{
    public abstract class NavigationEventArgsBase : EventArgs
    {
        public abstract string Parameter { get; }

        public abstract object Content { get; }

        public abstract NavigationMode NavigationMode { get; }

        [CanBeNull]
        public abstract IDataContext Context { get; }
    }
}
