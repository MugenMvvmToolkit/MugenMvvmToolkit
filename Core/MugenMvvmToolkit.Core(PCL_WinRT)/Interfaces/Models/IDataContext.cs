#region Copyright

// ****************************************************************************
// <copyright file="IDataContext.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IDataContext
    {
        int Count { get; }

        bool IsReadOnly { get; }

        void Add<T>([NotNull] DataConstant<T> dataConstant, T value);

        void AddOrUpdate<T>([NotNull] DataConstant<T> dataConstant, T value);

        T GetData<T>([NotNull] DataConstant<T> dataConstant);

        bool TryGetData<T>([NotNull] DataConstant<T> dataConstant, out T data);

        [Pure]
        bool Contains([NotNull] DataConstant dataConstant);

        bool Remove([NotNull] DataConstant dataConstant);

        void Merge([NotNull] IDataContext context);

        void Clear();

        IList<DataConstantValue> ToList();
    }
}
