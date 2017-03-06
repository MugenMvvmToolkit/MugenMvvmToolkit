#region Copyright

// ****************************************************************************
// <copyright file="StateProviderMock.cs">
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

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class StateManagerMock : IEntityStateManager
    {
        #region Properties

        public Func<object, IEntitySnapshot> CreateSnapshot { get; set; }

        #endregion

        #region Implementation of IEntityStateManager

        IEntitySnapshot IEntityStateManager.CreateSnapshot(object entity, IDataContext context)
        {
            return CreateSnapshot(entity);
        }

        #endregion
    }
}
