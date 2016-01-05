#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgsWrapper.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

#if SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Models.EventArg
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Models.EventArg
#endif
{
    public sealed class NavigationEventArgsWrapper : NavigationEventArgsBase
    {
        #region Fields

        private readonly NavigationEventArgs _args;

        #endregion

        #region Constructors

        public NavigationEventArgsWrapper([NotNull] NavigationEventArgs args)
        {
            Should.NotBeNull(args, nameof(args));
            _args = args;
        }

        #endregion

        #region Properties

        public NavigationEventArgs Args => _args;

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override object Content => _args.Content;

        public override NavigationMode Mode => _args.NavigationMode.ToNavigationMode();

        #endregion
    }
}
