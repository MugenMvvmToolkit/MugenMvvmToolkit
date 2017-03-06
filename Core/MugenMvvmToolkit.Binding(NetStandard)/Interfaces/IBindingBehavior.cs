#region Copyright

// ****************************************************************************
// <copyright file="IBindingBehavior.cs">
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

using System;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindingBehavior
    {
        Guid Id { get; }

        int Priority { get; }

        bool Attach([NotNull] IDataBinding binding);

        void Detach([NotNull] IDataBinding binding);

        [NotNull]
        IBindingBehavior Clone();
    }
}
