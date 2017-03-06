#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgsWrapper.cs">
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

using Windows.UI.Xaml.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.UWP.Models.EventArg
{
    public sealed class NavigationEventArgsWrapper : NavigationEventArgsBase
    {
        #region Fields

        private readonly NavigationEventArgs _args;
        private readonly bool _bringToFront;
        private readonly IDataContext _context;

        #endregion

        #region Constructors

        public NavigationEventArgsWrapper([NotNull] NavigationEventArgs args, bool bringToFront, IDataContext context)
        {
            Should.NotBeNull(args, nameof(args));
            _args = args;
            _bringToFront = bringToFront;
            _context = context;
        }

        #endregion

        #region Properties

        public NavigationEventArgs Args => _args;

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override object Content => _args.Content;

        public override NavigationMode NavigationMode
        {
            get
            {
                var mode = _args.NavigationMode.ToNavigationMode();
                if (_bringToFront && mode == NavigationMode.New)
                    return NavigationMode.Refresh;
                return mode;
            }
        }

        public override IDataContext Context => _context;

        #endregion
    }
}
