#region Copyright
// ****************************************************************************
// <copyright file="TokenType.cs">
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
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the type of token created by the Tokenizer class.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public class TokenType : StringConstantBase<TokenType>
    {
        #region Fields

        /// <summary>
        ///     Indictaes that the token is white space (space, tab, newline).
        /// </summary>
        public static readonly TokenType Whitespace = new TokenType("Whitespace", " ");

        /// <summary>
        ///     Indicates that the token is a '!='.
        /// </summary>
        public static readonly TokenType ExclamationEqual = new TokenType("ExclamationEqual", "!=");

        /// <summary>
        ///     Indicates that the token is a '!'.
        /// </summary>
        public static readonly TokenType Exclamation = new TokenType("Exclamation", "!");

        /// <summary>
        ///     Indicates that the token is a '%'.
        /// </summary>
        public static readonly TokenType Percent = new TokenType("Percent", "%");

        /// <summary>
        ///     Indicates that the token is a '$'.
        /// </summary>
        public static readonly TokenType Dollar = new TokenType("Dollar", "$");

        /// <summary>
        ///     Indicates that the token is a '$$'.
        /// </summary>
        public static readonly TokenType DoubleDollar = new TokenType("DoubleDollar", "$$");

        /// <summary>
        ///     Indicates that the token is a '&&'.
        /// </summary>
        public static readonly TokenType DoubleAmphersand = new TokenType("DoubleAmphersand", "&&");

        /// <summary>
        ///     Indicates that the token is a '&'.
        /// </summary>
        public static readonly TokenType Amphersand = new TokenType("Amphersand", "&");

        /// <summary>
        ///     Indicates that the token is a '('.
        /// </summary>
        public static readonly TokenType OpenParen = new TokenType("OpenParen", "(");

        /// <summary>
        ///     Indicates that the token is a ')'.
        /// </summary>
        public static readonly TokenType CloseParen = new TokenType("CloseParen", ")");

        /// <summary>
        ///     Indicates that the token is a '()'.
        /// </summary>
        public static readonly TokenType EmptyParen = new TokenType("EmptyParen", "()");

        /// <summary>
        ///     Indicates that the token is a '*'.
        /// </summary>
        public static readonly TokenType Asterisk = new TokenType("Asterisk", "*");

        /// <summary>
        ///     Indicates that the token is a '+'.
        /// </summary>
        public static readonly TokenType Plus = new TokenType("Plus", "+");

        /// <summary>
        ///     Indicates that the token is a ','.
        /// </summary>
        public static readonly TokenType Comma = new TokenType("Comma", ",");

        /// <summary>
        ///     Indicates that the token is a ';'.
        /// </summary>
        public static readonly TokenType Semicolon = new TokenType("Semicolon", ";");

        /// <summary>
        ///     Indicates that the token is a '-'.
        /// </summary>
        public static readonly TokenType Minus = new TokenType("Minus", "-");

        /// <summary>
        ///     Indicates that the token is a '.'.
        /// </summary>
        public static readonly TokenType Dot = new TokenType("Dot", ".");

        /// <summary>
        ///     Indicates that the token is a '/'.
        /// </summary>
        public static readonly TokenType Slash = new TokenType("Slash", "/");

        /// <summary>
        ///     Indicates that the token is a ':'.
        /// </summary>
        public static readonly TokenType Colon = new TokenType("Colon", ":");

        /// <summary>
        ///     Indicates that the token is a '=>'.
        /// </summary>
        public static readonly TokenType Lambda = new TokenType("Lambda", "=>");

        /// <summary>
        ///     Indicates that the token is a '<='.
        /// </summary>
        public static readonly TokenType LessThanEqual = new TokenType("LessThanEqual", "<=");

        /// <summary>
        ///     Indicates that the token is a '<'.
        /// </summary>
        public static readonly TokenType LessThan = new TokenType("LessThan", "<");

        /// <summary>
        ///     Indicates that the token is a '=='.
        /// </summary>
        public static readonly TokenType DoubleEqual = new TokenType("DoubleEqual", "==");

        /// <summary>
        ///     Indicates that the token is a '='.
        /// </summary>
        public static readonly TokenType Equal = new TokenType("Equal", "=");

        /// <summary>
        ///     Indicates that the token is a '>='.
        /// </summary>
        public static readonly TokenType GreaterThanEqual = new TokenType("GreaterThanEqual", ">=");

        /// <summary>
        ///     Indicates that the token is a '>'.
        /// </summary>
        public static readonly TokenType GreaterThan = new TokenType("GreaterThan", ">");

        /// <summary>
        ///     Indicates that the token is a '?'.
        /// </summary>
        public static readonly TokenType Question = new TokenType("Question", "?");

        /// <summary>
        ///     Indicates that the token is a '??'.
        /// </summary>
        public static readonly TokenType DoubleQuestion = new TokenType("DoubleQuestion", "??");

        /// <summary>
        ///     Indicates that the token is a '['.
        /// </summary>
        public static readonly TokenType OpenBracket = new TokenType("OpenBracket", "[");

        /// <summary>
        ///     Indicates that the token is a ']'.
        /// </summary>
        public static readonly TokenType CloseBracket = new TokenType("CloseBracket", "]");

        /// <summary>
        ///     Indicates that the token is a '||'.
        /// </summary>
        public static readonly TokenType DoubleBar = new TokenType("DoubleBar", "||");

        /// <summary>
        ///     Indicates that the token is a '|'.
        /// </summary>
        public static readonly TokenType Bar = new TokenType("Bar", "|");

        /// <summary>
        ///     Indicates that the token is a '~'.
        /// </summary>
        public static readonly TokenType Tilde = new TokenType("Tilde", "~");

        /// <summary>
        ///     Indicates that the token is a string literal.
        /// </summary>
        public static readonly TokenType StringLiteral = new TokenType("StringLiteral", string.Empty);

        /// <summary>
        ///     Indicates that the token is an identifier.
        /// </summary>
        public static readonly TokenType Identifier = new TokenType("Identifier", string.Empty);

        /// <summary>
        ///     Indicates that the token is an integer literal.
        /// </summary>
        public static readonly TokenType IntegerLiteral = new TokenType("IntegerLiteral", string.Empty);

        /// <summary>
        ///     Indicates that the token is an real literal.
        /// </summary>
        public static readonly TokenType RealLiteral = new TokenType("RealLiteral", string.Empty);

        /// <summary>
        ///     Indicates that the token is an open brace '{'.
        /// </summary>
        public static readonly TokenType OpenBrace = new TokenType("OpenBrace", "{");

        /// <summary>
        ///     Indicates that the token is a close brace '}'.
        /// </summary>
        public static readonly TokenType CloseBrace = new TokenType("CloseBrace", "}");

        /// <summary>
        ///     Indicates that the end of the input stream has been reached.
        /// </summary>
        public static readonly TokenType Eof = new TokenType("Eof", string.Empty);

        /// <summary>
        /// Indicates that the token was not recognized.
        /// </summary>
        public static readonly TokenType Unknown = new TokenType("Unknown", string.Empty);

        /// <summary>
        /// Gets the token value.
        /// </summary>
        public readonly string Value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TokenType" /> class.
        /// </summary>
        public TokenType([NotNull] string id, string value)
            : base(id)
        {
            Value = value;
        }

        #endregion

        #region Overrides

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Value))
                return Id;
            return string.Format("{0} '{1}'", Id, Value);
        }

        #endregion
    }
}