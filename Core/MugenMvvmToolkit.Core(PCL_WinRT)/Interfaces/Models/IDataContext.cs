#region Copyright
// ****************************************************************************
// <copyright file="IDataContext.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the specific operation context.
    /// </summary>
    public interface IDataContext
    {
        /// <summary>
        ///     Gets the number of elements contained in the <see cref="IDataContext" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="IDataContext" />.
        /// </returns>
        int Count { get; }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="IDataContext" /> is read-only.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="IDataContext" /> is read-only; otherwise, false.
        /// </returns>
        bool IsReadOnly { get; }

        /// <summary>
        ///     Adds the data constant value.
        /// </summary>
        void Add<T>([NotNull] DataConstant<T> dataConstant, T value);

        /// <summary>
        ///     Adds the data constant value or update existing.
        /// </summary>
        void AddOrUpdate<T>([NotNull] DataConstant<T> dataConstant, T value);

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        T GetData<T>([NotNull] DataConstant<T> dataConstant);

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        bool TryGetData<T>([NotNull] DataConstant<T> dataConstant, out T data);

        /// <summary>
        ///     Determines whether the <see cref="IDataContext" /> contains the specified key.
        /// </summary>
        [Pure]
        bool Contains([NotNull] DataConstant dataConstant);

        /// <summary>
        ///     Removes the data constant value.
        /// </summary>
        bool Remove([NotNull] DataConstant dataConstant);

        /// <summary>
        ///     Updates the current context.
        /// </summary>
        void Update([NotNull] IDataContext context);

        /// <summary>
        /// Removes all values from current context.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Creates an instance of <see cref="IList{DataConstantValue}" /> from current context.
        /// </summary>
        IList<DataConstantValue> ToList();
    }
}