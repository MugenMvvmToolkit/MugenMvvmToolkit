#region Copyright

// ****************************************************************************
// <copyright file="NavigationEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Android.Models.EventArg
{
    public class NavigationEventArgs : NavigationEventArgsBase
    {
        #region Fields

        private readonly object _content;
        private readonly NavigationMode _navigationMode;
        private readonly string _parameter;

        #endregion

        #region Constructors

        public NavigationEventArgs([NotNull] object content, string parameter, NavigationMode navigationMode)
        {
            _content = content;
            _navigationMode = navigationMode;
            _parameter = parameter;
        }

        #endregion

        #region Properties

        public string Parameter
        {
            get { return _parameter; }
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override object Content
        {
            get { return _content; }
        }

        public override NavigationMode Mode
        {
            get { return _navigationMode; }
        }

        #endregion
    }
}
