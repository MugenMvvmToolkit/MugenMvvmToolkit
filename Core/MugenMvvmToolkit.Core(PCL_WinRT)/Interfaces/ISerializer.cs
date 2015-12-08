#region Copyright

// ****************************************************************************
// <copyright file="ISerializer.cs">
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
using System.IO;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces
{
    public interface ISerializer
    {
        [NotNull]
        Stream Serialize([NotNull] object item);

        [NotNull]
        object Deserialize([NotNull] Stream stream);

        [Pure]
        bool IsSerializable([NotNull]Type type);
    }
}
