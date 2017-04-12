#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgsBase.cs">
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
    public abstract class NavigatingCancelEventArgsBase : EventArgs
    {
        public abstract string Parameter { get; }

        public abstract bool Cancel { get; set; }

        public abstract NavigationMode NavigationMode { get; }

        public abstract bool IsCancelable { get; }

        [NotNull]
        public abstract IDataContext Context { get; }
    }
}
