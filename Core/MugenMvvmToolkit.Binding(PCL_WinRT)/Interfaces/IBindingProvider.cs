#region Copyright

// ****************************************************************************
// <copyright file="IBindingProvider.cs">
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
using System.Linq.Expressions;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface IBindingProvider
    {
        [NotNull]
        ICollection<IBindingBehavior> DefaultBehaviors { get; }

        [NotNull]
        IBindingParser Parser { get; set; }

        [NotNull]
        IBindingBuilder CreateBuilder(IDataContext context = null);

        [NotNull]
        IDataBinding CreateBinding([NotNull] IDataContext context);

        [NotNull]
        IList<IBindingBuilder> CreateBuildersFromString([NotNull] object target, [NotNull] string bindingExpression, IList<object> sources = null,
            IDataContext context = null);

        [NotNull]
        IList<IDataBinding> CreateBindingsFromString([NotNull] object target, [NotNull] string bindingExpression, IList<object> sources = null,
            IDataContext context = null);

        void BuildFromLambdaExpression(IBindingBuilder builder, Func<LambdaExpression> expression);

        void BuildParameterFromLambdaExpression<TValue>(IBindingBuilder builder, Func<LambdaExpression> expression,
            DataConstant<Func<IDataContext, TValue>> parameterConstant);

        event Action<IBindingProvider, IDataContext> BindingInitializing;

        event Action<IBindingProvider, IDataBinding> BindingInitialized;
    }
}
