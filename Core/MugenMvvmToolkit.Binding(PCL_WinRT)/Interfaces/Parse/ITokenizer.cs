#region Copyright

// ****************************************************************************
// <copyright file="ITokenizer.cs">
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

using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Interfaces.Parse
{
    public interface ITokenizer
    {
        int FirstCharPosition { get; }

        int Position { get; }

        int Length { get; }

        TokenType Token { get; }

        string Value { get; }

        TokenType NextToken(bool ignoreWhitespace);
    }
}
