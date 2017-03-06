#region Copyright

// ****************************************************************************
// <copyright file="IBusyToken.cs">
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

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IBusyToken : IDisposable
    {
        bool IsCompleted { get; }

        object Message { get; }

        void Register(IBusyTokenCallback callback);
    }

    public interface IBusyTokenCallback
    {
        void OnCompleted(IBusyToken token);
    }
}
