#region Copyright

// ****************************************************************************
// <copyright file="TokenType.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the type of token created by the Tokenizer class.
    /// </summary>
    public class TokenType : StringConstantBase<TokenType>
    {
        #region Fields

        /// <summary>
        ///     Indictaes that the token is white space (space, tab, newline).
        /// </summary>
        public static readonly TokenType Whitespace;

        /// <summary>
        ///     Indicates that the token is a '!='.
        /// </summary>
        public static readonly TokenType ExclamationEqual;

        /// <summary>
        ///     Indicates that the token is a '!'.
        /// </summary>
        public static readonly TokenType Exclamation;

        /// <summary>
        ///     Indicates that the token is a '%'.
        /// </summary>
        public static readonly TokenType Percent;

        /// <summary>
        ///     Indicates that the token is a '$'.
        /// </summary>
        public static readonly TokenType Dollar;

        /// <summary>
        ///     Indicates that the token is a '$$'.
        /// </summary>
        public static readonly TokenType DoubleDollar;

        /// <summary>
        ///     Indicates that the token is a '&&'.
        /// </summary>
        public static readonly TokenType DoubleAmphersand;

        /// <summary>
        ///     Indicates that the token is a '&'.
        /// </summary>
        public static readonly TokenType Amphersand;

        /// <summary>
        ///     Indicates that the token is a '('.
        /// </summary>
        public static readonly TokenType OpenParen;

        /// <summary>
        ///     Indicates that the token is a ')'.
        /// </summary>
        public static readonly TokenType CloseParen;

        /// <summary>
        ///     Indicates that the token is a '()'.
        /// </summary>
        public static readonly TokenType EmptyParen;

        /// <summary>
        ///     Indicates that the token is a '*'.
        /// </summary>
        public static readonly TokenType Asterisk;

        /// <summary>
        ///     Indicates that the token is a '+'.
        /// </summary>
        public static readonly TokenType Plus;

        /// <summary>
        ///     Indicates that the token is a ','.
        /// </summary>
        public static readonly TokenType Comma;

        /// <summary>
        ///     Indicates that the token is a ';'.
        /// </summary>
        public static readonly TokenType Semicolon;

        /// <summary>
        ///     Indicates that the token is a '-'.
        /// </summary>
        public static readonly TokenType Minus;

        /// <summary>
        ///     Indicates that the token is a '.'.
        /// </summary>
        public static readonly TokenType Dot;

        /// <summary>
        ///     Indicates that the token is a '/'.
        /// </summary>
        public static readonly TokenType Slash;

        /// <summary>
        ///     Indicates that the token is a ':'.
        /// </summary>
        public static readonly TokenType Colon;

        /// <summary>
        ///     Indicates that the token is a '=>'.
        /// </summary>
        public static readonly TokenType Lambda;

        /// <summary>
        ///     Indicates that the token is a '<='.
        /// </summary>
        public static readonly TokenType LessThanEqual;

        /// <summary>
        ///     Indicates that the token is a '<'.
        /// </summary>
        public static readonly TokenType LessThan;

        /// <summary>
        ///     Indicates that the token is a '=='.
        /// </summary>
        public static readonly TokenType DoubleEqual;

        /// <summary>
        ///     Indicates that the token is a '='.
        /// </summary>
        public static readonly TokenType Equal;

        /// <summary>
        ///     Indicates that the token is a '>='.
        /// </summary>
        public static readonly TokenType GreaterThanEqual;

        /// <summary>
        ///     Indicates that the token is a '>'.
        /// </summary>
        public static readonly TokenType GreaterThan;

        /// <summary>
        ///     Indicates that the token is a '?'.
        /// </summary>
        public static readonly TokenType Question;

        /// <summary>
        ///     Indicates that the token is a '?.'.
        /// </summary>
        public static readonly TokenType QuestionDot;

        /// <summary>
        ///     Indicates that the token is a '??'.
        /// </summary>
        public static readonly TokenType DoubleQuestion;

        /// <summary>
        ///     Indicates that the token is a '['.
        /// </summary>
        public static readonly TokenType OpenBracket;

        /// <summary>
        ///     Indicates that the token is a ']'.
        /// </summary>
        public static readonly TokenType CloseBracket;

        /// <summary>
        ///     Indicates that the token is a '||'.
        /// </summary>
        public static readonly TokenType DoubleBar;

        /// <summary>
        ///     Indicates that the token is a '|'.
        /// </summary>
        public static readonly TokenType Bar;

        /// <summary>
        ///     Indicates that the token is a '~'.
        /// </summary>
        public static readonly TokenType Tilde;

        /// <summary>
        ///     Indicates that the token is a string literal.
        /// </summary>
        public static readonly TokenType StringLiteral;

        /// <summary>
        ///     Indicates that the token is an identifier.
        /// </summary>
        public static readonly TokenType Identifier;

        /// <summary>
        ///     Indicates that the token is an integer literal.
        /// </summary>
        public static readonly TokenType IntegerLiteral;

        /// <summary>
        ///     Indicates that the token is an real literal.
        /// </summary>
        public static readonly TokenType RealLiteral;

        /// <summary>
        ///     Indicates that the token is an open brace '{'.
        /// </summary>
        public static readonly TokenType OpenBrace;

        /// <summary>
        ///     Indicates that the token is a close brace '}'.
        /// </summary>
        public static readonly TokenType CloseBrace;

        /// <summary>
        ///     Indicates that the end of the input stream has been reached.
        /// </summary>
        public static readonly TokenType Eof;

        /// <summary>
        /// Indicates that the token was not recognized.
        /// </summary>
        public static readonly TokenType Unknown;

        /// <summary>
        /// Gets the token value.
        /// </summary>
        public readonly string Value;

        #endregion

        #region Constructors

        static TokenType()
        {
            Whitespace = new TokenType("Whitespace", " ");
            ExclamationEqual = new TokenType("ExclamationEqual", "!=");
            Exclamation = new TokenType("Exclamation", "!");
            Percent = new TokenType("Percent", "%");
            Dollar = new TokenType("Dollar", "$");
            DoubleDollar = new TokenType("DoubleDollar", "$$");
            DoubleAmphersand = new TokenType("DoubleAmphersand", "&&");
            Amphersand = new TokenType("Amphersand", "&");
            OpenParen = new TokenType("OpenParen", "(");
            CloseParen = new TokenType("CloseParen", ")");
            EmptyParen = new TokenType("EmptyParen", "()");
            Asterisk = new TokenType("Asterisk", "*");
            Plus = new TokenType("Plus", "+");
            Comma = new TokenType("Comma", ",");
            Semicolon = new TokenType("Semicolon", ";");
            Minus = new TokenType("Minus", "-");
            Dot = new TokenType("Dot", ".");
            Slash = new TokenType("Slash", "/");
            Colon = new TokenType("Colon", ":");
            Lambda = new TokenType("Lambda", "=>");
            LessThanEqual = new TokenType("LessThanEqual", "<=");
            LessThan = new TokenType("LessThan", "<");
            DoubleEqual = new TokenType("DoubleEqual", "==");
            Equal = new TokenType("Equal", "=");
            GreaterThanEqual = new TokenType("GreaterThanEqual", ">=");
            GreaterThan = new TokenType("GreaterThan", ">");
            Question = new TokenType("Question", "?");
            QuestionDot = new TokenType("QuestionDot", "?.");
            DoubleQuestion = new TokenType("DoubleQuestion", "??");
            OpenBracket = new TokenType("OpenBracket", "[");
            CloseBracket = new TokenType("CloseBracket", "]");
            DoubleBar = new TokenType("DoubleBar", "||");
            Bar = new TokenType("Bar", "|");
            Tilde = new TokenType("Tilde", "~");
            StringLiteral = new TokenType("StringLiteral", string.Empty);
            Identifier = new TokenType("Identifier", string.Empty);
            IntegerLiteral = new TokenType("IntegerLiteral", string.Empty);
            RealLiteral = new TokenType("RealLiteral", string.Empty);
            OpenBrace = new TokenType("OpenBrace", "{");
            CloseBrace = new TokenType("CloseBrace", "}");
            Eof = new TokenType("Eof", string.Empty);
            Unknown = new TokenType("Unknown", string.Empty);
        }

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