#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgsBase.cs">
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

namespace MugenMvvmToolkit.Models.EventArg
{
    public abstract class NavigationEventArgsBase : EventArgs
    {
        public abstract object Content { get; }

        public abstract NavigationMode Mode { get; }
    }
}
