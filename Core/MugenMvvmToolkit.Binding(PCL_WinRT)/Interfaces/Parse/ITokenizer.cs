#region Copyright
// ****************************************************************************
// <copyright file="ITokenizer.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    /// <summary>
    ///     Represents the tokenizer interface.
    /// </summary>
    public interface ITokenizer
    {
        /// <summary>
        ///     Gets the first token char position.
        /// </summary>
        int FirstCharPosition { get; }

        /// <summary>
        ///     The current column number of the stream being read.
        /// </summary>
        int Position { get; }

        /// <summary>
        ///     Gets the length of source string.
        /// </summary>
        int Length { get; }

        /// <summary>
        ///     Gets the token type of the current token.
        /// </summary>
        TokenType Token { get; }

        /// <summary>
        ///     If the current token is a word token, this field contains a string giving the characters of the word token.
        /// </summary>
        string Value { get; }

        /// <summary>
        ///     Returns the next token.
        /// </summary>
        /// <param name="ignoreWhitespace">Determines is whitespace is ignored. True if whitespace is to be ignored.</param>
        /// <returns>The TokenType of the next token.</returns>
        TokenType NextToken(bool ignoreWhitespace);
    }
}