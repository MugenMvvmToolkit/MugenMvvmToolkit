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
    public class TokenType : StringConstantBase<TokenType>
    {
        #region Fields

        public static readonly TokenType Whitespace;

        public static readonly TokenType ExclamationEqual;

        public static readonly TokenType Exclamation;

        public static readonly TokenType Percent;

        public static readonly TokenType Dollar;

        public static readonly TokenType DoubleDollar;

        public static readonly TokenType DoubleAmphersand;

        public static readonly TokenType Amphersand;

        public static readonly TokenType OpenParen;

        public static readonly TokenType CloseParen;

        public static readonly TokenType EmptyParen;

        public static readonly TokenType Asterisk;

        public static readonly TokenType Plus;

        public static readonly TokenType Comma;

        public static readonly TokenType Semicolon;

        public static readonly TokenType Minus;

        public static readonly TokenType Dot;

        public static readonly TokenType Slash;

        public static readonly TokenType Colon;

        public static readonly TokenType Lambda;

        public static readonly TokenType LessThanEqual;

        public static readonly TokenType LessThan;

        public static readonly TokenType DoubleEqual;

        public static readonly TokenType Equal;

        public static readonly TokenType GreaterThanEqual;

        public static readonly TokenType GreaterThan;

        public static readonly TokenType Question;

        public static readonly TokenType QuestionDot;

        public static readonly TokenType DoubleQuestion;

        public static readonly TokenType OpenBracket;

        public static readonly TokenType CloseBracket;

        public static readonly TokenType DoubleBar;

        public static readonly TokenType Bar;

        public static readonly TokenType Tilde;

        public static readonly TokenType StringLiteral;

        public static readonly TokenType Identifier;

        public static readonly TokenType IntegerLiteral;

        public static readonly TokenType RealLiteral;

        public static readonly TokenType OpenBrace;

        public static readonly TokenType CloseBrace;

        public static readonly TokenType Eof;

        public static readonly TokenType Unknown;

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

        public TokenType([NotNull] string id, string value)
            : base(id)
        {
            Value = value;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Value))
                return Id;
            return $"{Id} '{Value}'";
        }

        #endregion
    }
}
