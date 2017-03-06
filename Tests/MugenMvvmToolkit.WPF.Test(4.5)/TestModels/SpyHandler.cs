#region Copyright

// ****************************************************************************
// <copyright file="SpyHandler.cs">
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
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class SpyHandler : IHandler<object>
    {
        #region Properties

        public Action<object, object> HandleDelegate { get; set; }

        public int HandleCount { get; set; }

        #endregion

        #region Implementation of IHandler<in object>

        void IHandler<object>.Handle(object sender, object message)
        {
            HandleCount++;
            if (HandleDelegate != null)
                HandleDelegate(sender, message);
        }

        #endregion
    }
}
