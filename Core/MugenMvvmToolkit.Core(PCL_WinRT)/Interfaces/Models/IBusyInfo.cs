#region Copyright

// ****************************************************************************
// <copyright file="IBusyInfo.cs">
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
using System.Collections.Generic;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IBusyInfo
    {
        bool TryGetMessage<TType>(out TType message, Func<TType, bool> filter = null);

        IList<object> GetMessages();
    }
}