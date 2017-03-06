#region Copyright

// ****************************************************************************
// <copyright file="StateTransitionManagerMock.cs">
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class StateTransitionManagerMock : IStateTransitionManager
    {
        #region Properties

        public Func<object, EntityState, EntityState, EntityState> ChangeState { get; set; }

        #endregion

        #region Implementation of IStateTransitionManager

        EntityState IStateTransitionManager.ChangeState(object item, EntityState @from, EntityState to)
        {
            if (ChangeState == null)
                return to;
            return ChangeState(item, @from, to);
        }

        #endregion
    }
}
