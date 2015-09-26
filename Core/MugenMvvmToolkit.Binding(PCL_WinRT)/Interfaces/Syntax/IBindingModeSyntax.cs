#region Copyright

// ****************************************************************************
// <copyright file="IBindingModeSyntax.cs">
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
    public interface IBindingModeSyntax : IBuilderSyntax
    {
    }

    public interface IBindingModeSyntax<in TSource> : IBindingModeSyntax
    {
    }
}
