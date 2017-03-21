#region Copyright

// ****************************************************************************
// <copyright file="ViewModelPreservedEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelPreservedEventArgs : ViewModelPreservingEventArgs
    {
        #region Fields

        private IDataContext _state;

        #endregion

        #region Constructors

        public ViewModelPreservedEventArgs(IViewModel viewModel)
            : base(viewModel)
        {
        }

        #endregion

        #region Properties

        [NotNull]
        public IDataContext State
        {
            get { return _state; }
            set
            {
                Should.PropertyNotBeNull(value);
                _state = value;                
            }
        }

        #endregion
    }
}