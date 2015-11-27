#region Copyright

// ****************************************************************************
// <copyright file="IOperationCallbackStateManager.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    public interface IOperationCallbackStateManager
    {
        object SaveValue([CanBeNull] object value, [CanBeNull] FieldInfo field, [CanBeNull] IAsyncOperation asyncOperation, [CanBeNull] IDataContext context);

        object RestoreValue([CanBeNull] object value, [CanBeNull] FieldInfo field, [CanBeNull] IDictionary<Type, object> items,
            [CanBeNull] ICollection<IViewModel> viewModels, [CanBeNull] IOperationResult result, [CanBeNull] IDataContext context);

        IViewModel RestoreViewModelValue([NotNull] Type viewModelType, Guid viewModelId, [CanBeNull] IDictionary<Type, object> items,
            [CanBeNull] ICollection<IViewModel> viewModels, [CanBeNull] IOperationResult result, [CanBeNull] IDataContext context);
    }
}