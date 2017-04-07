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

using System.Windows.Navigation;
using MugenMvvmToolkit.Models.EventArg;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.WPF.Models.EventArg
{
    public sealed class NavigationEventArgsWrapper : NavigationEventArgsBase
    {
        #region Fields

        private readonly NavigationEventArgs _args;
        private readonly NavigationMode _mode;
        private readonly IDataContext _context;

        #endregion

        #region Constructors

        public NavigationEventArgsWrapper([NotNull] NavigationEventArgs args, NavigationMode mode, IDataContext context)
        {
            Should.NotBeNull(args, nameof(args));
            _args = args;
            _mode = mode;
            _context = context;
        }

        #endregion

        #region Properties

        public NavigationEventArgs Args => _args;

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override string Parameter => Args.ExtraData as string;

        public override object Content => _args.Content;

        public override NavigationMode NavigationMode => _mode;

        public override IDataContext Context => _context;

        #endregion
    }
}
