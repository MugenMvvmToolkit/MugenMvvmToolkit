#region Copyright

// ****************************************************************************
// <copyright file="ViewResult.cs">
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

using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    public sealed class ViewResult
    {
        #region Fields

        private readonly IDataContext _dataContext;
        private readonly View _view;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewResult" /> class.
        /// </summary>
        public ViewResult([NotNull] View view, IDataContext dataContext)
        {
            Should.NotBeNull(view, "view");
            _view = view;
            _dataContext = dataContext ?? Models.DataContext.Empty;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the <see cref="View" />.
        /// </summary>
        [NotNull]
        public View View
        {
            get { return _view; }
        }

        /// <summary>
        ///     Gets the <see cref="IDataContext" />.
        /// </summary>
        [NotNull]
        public IDataContext DataContext
        {
            get { return _dataContext; }
        }

        #endregion
    }
}