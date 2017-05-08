#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgs.cs">
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

namespace MugenMvvmToolkit.UWP.Models.EventArg
{
    public class NavigationEventArgs : NavigationEventArgsBase
    {
        #region Fields

        private readonly IDataContext _context;
        private readonly string _parameter;
        private readonly object _content;
        private readonly NavigationMode _navigationMode;

        #endregion

        #region Constructors

        public NavigationEventArgs(NavigationMode navigationMode, IDataContext context, object content, string parameter)
        {
            _navigationMode = navigationMode;
            _context = context.ToNonReadOnly();
            _content = content;
            _parameter = parameter;
        }

        #endregion

        #region Properties

        public override string Parameter => _parameter;

        public override object Content => _content;

        public override NavigationMode NavigationMode => _navigationMode;

        public override IDataContext Context => _context;

        #endregion
    }
}