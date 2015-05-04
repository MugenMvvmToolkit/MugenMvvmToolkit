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
    /// <summary>
    ///     Used to define a basic binding syntax builder.
    /// </summary>
    public sealed class SyntaxBuilder<TTarget, TSource> : IBindingModeInfoBehaviorSyntax<TSource>,
        IBindingToSyntax<TTarget, TSource>, IBindingInfoBehaviorSyntax<TSource>
    {
        #region Fields

        private readonly IBindingBuilder _builder;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SyntaxBuilder{TTarget, TSource}" /> class.
        /// </summary>
        public SyntaxBuilder(IBindingBuilder builder)
        {
            Should.NotBeNull(builder, "builder");
            _builder = builder;
        }

        #endregion

        #region Implementation of IBuilderSyntax

        /// <summary>
        ///     Gets the current <see cref="IBindingBuilder" />.
        /// </summary>
        public IBindingBuilder Builder
        {
            get { return _builder; }
        }

        #endregion
    }
}