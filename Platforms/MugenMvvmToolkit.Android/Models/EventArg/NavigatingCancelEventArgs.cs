#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgs.cs">
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

namespace MugenMvvmToolkit.Android.Models.EventArg
{
    public class NavigatingCancelEventArgs : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly bool _isCancelable;
        private readonly IViewMappingItem _mapping;
        private readonly NavigationMode _navigationMode;
        private readonly string _parameter;
        private readonly IDataContext _context;

        #endregion

        #region Constructors

        public NavigatingCancelEventArgs(NavigationMode navigationMode, IDataContext context)
        {
            _navigationMode = navigationMode;
            _isCancelable = true;
            _context = context.ToNonReadOnly();
        }

        public NavigatingCancelEventArgs(IViewMappingItem mapping, NavigationMode navigationMode, string parameter, IDataContext context)
        {
            _mapping = mapping;
            _navigationMode = navigationMode;
            _parameter = parameter;
            _context = context.ToNonReadOnly();
            _isCancelable = true;
        }

        #endregion

        #region Properties

        public IViewMappingItem Mapping => _mapping;

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode => _navigationMode;

        public override bool IsCancelable => _isCancelable;

        public override IDataContext Context => _context;

        public override string Parameter => _parameter;

        #endregion
    }
}
