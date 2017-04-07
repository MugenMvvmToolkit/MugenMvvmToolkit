#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgsWrapper.cs">
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

using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.WPF.Models.EventArg
{
    public sealed class NavigatingCancelEventArgsWrapper : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly NavigatingCancelEventArgs _args;
        private readonly IDataContext _context;

        #endregion

        #region Constructors

        public NavigatingCancelEventArgsWrapper([NotNull] NavigatingCancelEventArgs args, IDataContext context)
        {
            Should.NotBeNull(args, nameof(args));
            _args = args;
            _context = context;
        }

        #endregion

        #region Properties

        public NavigatingCancelEventArgs Args => _args;

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override string Parameter => Args.ExtraData as string;

        public override bool Cancel
        {
            get { return _args.Cancel; }
            set { _args.Cancel = value; }
        }

        public override NavigationMode NavigationMode => _args.NavigationMode.ToNavigationMode();

        public override bool IsCancelable => true;

        public override IDataContext Context => _context;

        #endregion
    }
}
