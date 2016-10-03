#region Copyright

// ****************************************************************************
// <copyright file="IEventListener.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Models
{
    public interface IEventListener
    {
        bool IsAlive { get; }

        bool IsWeak { get; }

        bool TryHandle(object sender, object message);
    }
}
