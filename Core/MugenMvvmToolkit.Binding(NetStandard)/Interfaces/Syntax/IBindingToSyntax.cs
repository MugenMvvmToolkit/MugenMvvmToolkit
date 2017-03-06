#region Copyright

// ****************************************************************************
// <copyright file="IBindingToSyntax.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Syntax
{
    public interface IBindingToSyntax : IBuilderSyntax
    {
    }

    public interface IBindingToSyntax<out TTarget> : IBindingToSyntax
        where TTarget : class
    {
    }

    public interface IBindingToSyntax<out TTarget, in TSource> : IBindingToSyntax<TTarget>
        where TTarget : class
    {
    }
}
