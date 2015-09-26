#region Copyright

// ****************************************************************************
// <copyright file="IBindingManager.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindingManager
    {
        void Register([NotNull] object target, [NotNull] string path, [NotNull] IDataBinding binding, IDataContext context = null);

        bool IsRegistered([NotNull] IDataBinding binding);

        IEnumerable<IDataBinding> GetBindings([NotNull] object target, IDataContext context = null);

        IEnumerable<IDataBinding> GetBindings([NotNull] object target, [NotNull] string path, IDataContext context = null);

        void Unregister(IDataBinding binding);

        void ClearBindings([NotNull] object target, IDataContext context = null);

        void ClearBindings([NotNull] object target, [NotNull] string path, IDataContext context = null);
    }
}
