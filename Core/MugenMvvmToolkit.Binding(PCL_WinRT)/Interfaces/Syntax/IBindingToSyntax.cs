#region Copyright

// ****************************************************************************
// <copyright file="IBindingToSyntax.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Syntax
{
    /// <summary>
    ///     Used to define a basic binding syntax builder.
    /// </summary>
    public interface IBindingToSyntax : IBuilderSyntax
    {
    }

    /// <summary>
    ///     Used to define a basic binding syntax builder.
    /// </summary>
    public interface IBindingToSyntax<TTarget> : IBindingToSyntax
    {
    }

    /// <summary>
    ///     Used to define a basic binding syntax builder.
    /// </summary>
    public interface IBindingToSyntax<TTarget, TSource> : IBindingToSyntax<TTarget>
    {
    }
}