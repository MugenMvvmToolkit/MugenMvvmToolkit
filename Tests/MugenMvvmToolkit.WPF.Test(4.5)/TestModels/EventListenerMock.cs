#region Copyright

// ****************************************************************************
// <copyright file="EventListenerMock.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class EventListenerMock : IEventListener
    {
        #region Constructors

        public EventListenerMock()
        {
            IsAlive = true;
        }

        #endregion

        #region Properties

        public Action<object, object> Handle { get; set; }

        #endregion

        #region Implementation of IEventListener

        public bool IsAlive { get; set; }

        public bool IsWeak { get; set; }

        public bool TryHandle(object sender, object message)
        {
            if (Handle != null)
                Handle(sender, message);
            return IsAlive;
        }

        #endregion
    }
}
