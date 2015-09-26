#region Copyright

// ****************************************************************************
// <copyright file="IBindingResourceResolver.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindingResourceResolver
    {
        [NotNull]
        string SelfResourceName { get; set; }

        [NotNull]
        string RootElementResourceName { get; set; }

        [NotNull]
        string BindingSourceResourceName { get; set; }

        [NotNull]
        string DataContextResourceName { get; set; }

        [NotNull]
        IList<Type> GetKnownTypes();

        IBindingValueConverter ResolveConverter([NotNull] string name, IDataContext context, bool throwOnError);

        Type ResolveType([NotNull] string typeName, IDataContext context, bool throwOnError);

        IBindingResourceMethod ResolveMethod([NotNull] string name, IDataContext context, bool throwOnError);

        ISourceValue ResolveObject([NotNull] string name, IDataContext context, bool throwOnError);

        IBindingBehavior ResolveBehavior([NotNull] string name, IDataContext context, IList<object> args, bool throwOnError);

        void AddBehavior([NotNull] string name, [NotNull] Func<IDataContext, IList<object>, IBindingBehavior> getBehavior, bool rewrite = true);

        void AddConverter([NotNull] string name, [NotNull] IBindingValueConverter converter, bool rewrite = true);

        void AddType([NotNull] string name, [NotNull] Type type, bool rewrite = true);

        void AddMethod([NotNull] string name, [NotNull] IBindingResourceMethod method, bool rewrite = true);

        void AddObject([NotNull] string name, [NotNull] ISourceValue obj, bool rewrite = true);

        bool RemoveBehavior(string name);

        bool RemoveConverter(string name);

        bool RemoveType(string name);

        bool RemoveMethod(string name);

        bool RemoveObject(string name);
    }
}
