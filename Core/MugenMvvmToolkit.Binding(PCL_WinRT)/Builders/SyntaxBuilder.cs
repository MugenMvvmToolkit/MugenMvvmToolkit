#region Copyright

// ****************************************************************************
// <copyright file="SyntaxBuilder.cs">
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

using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Syntax;

namespace MugenMvvmToolkit.Binding.Builders
{
    public sealed class SyntaxBuilder<TTarget, TSource> : IBindingModeInfoBehaviorSyntax<TSource>,
        IBindingToSyntax<TTarget, TSource>, IBindingInfoBehaviorSyntax<TSource>
        where TTarget : class
    {
        #region Fields

        private readonly IBindingBuilder _builder;

        #endregion

        #region Constructors

        public SyntaxBuilder(IBindingBuilder builder)
        {
            Should.NotBeNull(builder, nameof(builder));
            _builder = builder;
        }

        #endregion

        #region Implementation of IBuilderSyntax

        public IBindingBuilder Builder
        {
            get { return _builder; }
        }

        #endregion
    }
}
