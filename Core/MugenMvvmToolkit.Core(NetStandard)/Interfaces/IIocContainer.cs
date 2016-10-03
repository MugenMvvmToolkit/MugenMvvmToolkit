#region Copyright

// ****************************************************************************
// <copyright file="IIocContainer.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IIocContainer : IDisposableObject, IServiceProvider
    {
        int Id { get; }

        [CanBeNull]
        IIocContainer Parent { get; }

        [NotNull]
        object Container { get; }

        [NotNull]
        IIocContainer CreateChild();

        [Pure]
        object Get([NotNull] Type service, string name = null, params IIocParameter[] parameters);

        [Pure]
        IEnumerable<object> GetAll([NotNull] Type service, string name = null, params IIocParameter[] parameters);

        void BindToConstant([NotNull] Type service, object instance, string name = null);

        void BindToMethod([NotNull] Type service, Func<IIocContainer, IList<IIocParameter>, object> methodBindingDelegate, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters);

        void Bind([NotNull] Type service, [NotNull] Type typeTo, DependencyLifecycle lifecycle, string name = null, params IIocParameter[] parameters);

        void Unbind([NotNull] Type service);

        [Pure]
        bool CanResolve([NotNull] Type service, string name = null);
    }
}
