#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgsWrapper.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.WPF.Models.EventArg
{
    public sealed class NavigationEventArgsWrapper : NavigationEventArgsBase
    {
        #region Fields

        private readonly NavigationEventArgs _args;
        private readonly NavigationMode _mode;

        #endregion

        #region Constructors

        public NavigationEventArgsWrapper([NotNull] NavigationEventArgs args, NavigationMode mode)
        {
            Should.NotBeNull(args, "args");
            _args = args;
            _mode = mode;
        }

        #endregion

        #region Properties

        public NavigationEventArgs Args
        {
            get { return _args; }
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override object Content
        {
            get { return _args.Content; }
        }

        public override NavigationMode Mode
        {
            get { return _mode; }
        }

        #endregion
    }
}
