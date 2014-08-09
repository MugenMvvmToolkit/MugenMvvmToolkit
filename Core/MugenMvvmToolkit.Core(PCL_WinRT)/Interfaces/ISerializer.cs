#region Copyright
// ****************************************************************************
// <copyright file="ISerializer.cs">
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
using System.IO;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the serializer interface that allows to serialize and deserialize objects.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        ///     Adds a known type.
        /// </summary>
        void AddKnownType([NotNull] Type type);

        /// <summary>
        ///     Adds a known type.
        /// </summary>
        bool RemoveKnownType([NotNull] Type type);

        /// <summary>
        ///     Serializes data to stream.
        /// </summary>
        [NotNull]
        Stream Serialize([NotNull] object item);

        /// <summary>
        ///     Deserializes data using stream.
        /// </summary>
        [NotNull]
        object Deserialize([NotNull] Stream stream);
    }
}