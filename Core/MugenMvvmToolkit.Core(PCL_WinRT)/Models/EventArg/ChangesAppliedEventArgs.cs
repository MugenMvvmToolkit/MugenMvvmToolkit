#region Copyright
// ****************************************************************************
// <copyright file="ChangesAppliedEventArgs.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ChangesAppliedEventArgs : EventArgs
    {
        #region Fields

        private readonly IList<IEntityStateEntry> _changes;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes the <see cref="ChangesAppliedEventArgs" />.
        /// </summary>
        public ChangesAppliedEventArgs([NotNull]IList<IEntityStateEntry> changes)
        {
            Should.NotBeNull(changes, "changes");
            _changes = changes;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the applied changes.
        /// </summary>
        [NotNull]
        public IList<IEntityStateEntry> Changes
        {
            get { return _changes; }
        }

        #endregion
    }
}