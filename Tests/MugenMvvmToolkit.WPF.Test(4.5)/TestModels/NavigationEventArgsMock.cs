#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgsMock.cs">
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

using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class NavigationEventArgsMock : NavigationEventArgsBase
    {
        #region Constructors

        public NavigationEventArgsMock(object content, NavigationMode mode, string parameter = null, IDataContext context = null)
        {
            Content = content;
            NavigationMode = mode;
            Context = context;
            Parameter = parameter;
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override string Parameter { get; }

        public override object Content { get; }

        public override NavigationMode NavigationMode { get; }

        public override IDataContext Context { get; }

        #endregion
    }
}