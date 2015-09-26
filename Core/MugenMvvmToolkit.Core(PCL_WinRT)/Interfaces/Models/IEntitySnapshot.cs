#region Copyright

// ****************************************************************************
// <copyright file="IEntitySnapshot.cs">
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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IEntitySnapshot
    {
        bool SupportChangeDetection { get; }

        void Restore([NotNull] object entity);

        [Pure]
        bool HasChanges([NotNull] object entity);

        [Pure]
        bool HasChanges([NotNull] object entity, [NotNull] string propertyName);

        [NotNull]
        IDictionary<string, Tuple<object, object>> Dump([NotNull] object entity);
    }
}
