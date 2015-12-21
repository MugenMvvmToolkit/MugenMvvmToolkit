#region Copyright

// ****************************************************************************
// <copyright file="Tokenizer.cs">
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    [DebuggerDisplay("{Token} = {Value})")]
    public class Tokenizer : ITokenizer
    {
        #region Fields

        private static readonly HashSet<char> IntegerSuffixes;
        private static readonly HashSet<char> RealSuffixes;

        private string _source;
        private readonly bool _throwOnError;
        private readonly HashSet<char> _ignoreChars;

        #endregion

        #region Constructors

        static Tokenizer()
        {
            IntegerSuffixes = new HashSet<char>
            {
                'u',
                'l',
                'U',
                'L',
            };
            RealSuffixes = new HashSet<char>
            {
                'f',
                'd',
                'm',
                'F',
                'D',
                'M'
            };
        }

        public Tokenizer(bool throwOnError)
        {
            _throwOnError = throwOnError;
            _ignoreChars = new HashSet<char>();
        }

        public Tokenizer(bool throwOnError, [NotNull] string source)
            : this(throwOnError)
        {
            SetSource(source);
        }

        #endregion

        #region Properties

        public string Source
        {
            get { return _source; }
        }

        protected Char CurrentChar { get; set; }

        public HashSet<char> IgnoreChars
        {
            get { return _ignoreChars; }
        }

        public bool ThrowOnError
        {
            get { return _throwOnError; }
        }

        #endregion

        #region Implementation of ITokenizer

        public int FirstCharPosition { get; protected set; }

        public int Position { get; protected set; }

        public int Length
        {
            get { return Source.Length; }
        }

        public TokenType Token { get; protected set; }

        public string Value { get; protected set; }

        public TokenType NextToken(bool ignoreWhitespace)
        {
            return NextTokenInternal(ignoreWhitespace);
        }

        #endregion

        #region Methods

        public void SetSource(string source)
        {
            Should.NotBeNullOrEmpty(source, nameof(source));
            _source = source;
            CurrentChar = source[0];
            Position = 0;
            Token = null;
        }

        protected virtual void NextChar()
        {
            if (Position < Length)
                Position++;
            CurrentChar = Position < Length ? Source[Position] : '\0';
        }

        protected virtual TokenType NextTokenInternal(bool ignoreWhitespace)
        {
            FirstCharPosition = Position;
            if (ignoreWhitespace)
                while (Char.IsWhiteSpace(CurrentChar))
                    NextChar();
            TokenType t;
            int tokenPos = Position;
            if (_ignoreChars.Contains(CurrentChar))
            {
                NextChar();
                t = TokenType.Unknown;
            }
            else
                t = ReadToken();
            Token = t;
            Value = Source.Substring(tokenPos, Position - tokenPos);
            return t;
        }

        protected virtual TokenType ReadToken()
        {
            TokenType t;
            int tokenPos = Position;
            switch (CurrentChar)
            {
                case '!':
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        NextChar();
                        t = TokenType.ExclamationEqual;
                    }
                    else
                        t = TokenType.Exclamation;
                    break;
                case '%':
                    NextChar();
                    t = TokenType.Percent;
                    break;
                case '$':
                    NextChar();
                    if (CurrentChar == '$')
                    {
                        NextChar();
                        t = TokenType.DoubleDollar;
                    }
                    else
                        t = TokenType.Dollar;
                    break;
                case '&':
                    NextChar();
                    if (CurrentChar == '&')
                    {
                        NextChar();
                        t = TokenType.DoubleAmphersand;
                    }
                    else
                        t = TokenType.Amphersand;
                    break;
                case '(':
                    NextChar();
                    while (Position < Length && Char.IsWhiteSpace(CurrentChar))
                        NextChar();
                    if (CurrentChar == ')')
                    {
                        NextChar();
                        t = TokenType.EmptyParen;
                    }
                    else
                        t = TokenType.OpenParen;
                    break;
                case ')':
                    NextChar();
                    t = TokenType.CloseParen;
                    break;
                case '*':
                    NextChar();
                    t = TokenType.Asterisk;
                    break;
                case '+':
                    NextChar();
                    t = TokenType.Plus;
                    break;
                case ',':
                    NextChar();
                    t = TokenType.Comma;
                    break;
                case ';':
                    NextChar();
                    t = TokenType.Semicolon;
                    break;
                case '-':
                    NextChar();
                    t = TokenType.Minus;
                    break;
                case '.':
                    NextChar();
                    t = TokenType.Dot;
                    break;
                case '/':
                    NextChar();
                    t = TokenType.Slash;
                    break;
                case ':':
                    NextChar();
                    t = TokenType.Colon;
                    break;
                case '<':
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        NextChar();
                        t = TokenType.LessThanEqual;
                    }
                    else if (CurrentChar == '>')
                    {
                        NextChar();
                        t = TokenType.ExclamationEqual;
                    }
                    else
                        t = TokenType.LessThan;
                    break;
                case '=':
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        NextChar();
                        t = TokenType.DoubleEqual;
                    }
                    else if (CurrentChar == '>')
                    {
                        NextChar();
                        t = TokenType.Lambda;
                    }
                    else
                        t = TokenType.Equal;
                    break;
                case '>':
                    NextChar();
                    if (CurrentChar == '=')
                    {
                        NextChar();
                        t = TokenType.GreaterThanEqual;
                    }
                    else
                        t = TokenType.GreaterThan;
                    break;
                case '?':
                    NextChar();
                    if (CurrentChar == '?')
                    {
                        NextChar();
                        t = TokenType.DoubleQuestion;
                    }
                    else if (CurrentChar == '.')
                    {
                        NextChar();
                        t = TokenType.QuestionDot;
                    }
                    else if (CurrentChar == '[')
                        t = TokenType.QuestionDot;
                    else
                        t = TokenType.Question;
                    break;
                case '[':
                    NextChar();
                    t = TokenType.OpenBracket;
                    break;
                case ']':
                    NextChar();
                    t = TokenType.CloseBracket;
                    break;
                case '{':
                    NextChar();
                    t = TokenType.OpenBrace;
                    break;
                case '}':
                    NextChar();
                    t = TokenType.CloseBrace;
                    break;
                case '|':
                    NextChar();
                    if (CurrentChar == '|')
                    {
                        NextChar();
                        t = TokenType.DoubleBar;
                    }
                    else
                        t = TokenType.Bar;
                    break;
                case '~':
                    NextChar();
                    t = TokenType.Tilde;
                    break;
                case '"':
                case '\'':
                    bool isValid = true;
                    char quote = CurrentChar;
                    var position = Position;
                    do
                    {
                        NextChar();
                        while (Position < Length && (CurrentChar != quote || Source[Position - 1] == '\\'))
                            NextChar();
                        if (Position == Length)
                        {
                            if (_throwOnError)
                                throw BindingExceptionManager.UnterminatedStringLiteral(Source.Substring(tokenPos, Position - tokenPos), this, Source);
                            Position = position;
                            CurrentChar = quote;
                            isValid = false;
                            NextChar();
                            break;
                        }
                        NextChar();
                    } while (CurrentChar == quote);
                    t = isValid ? TokenType.StringLiteral : TokenType.Unknown;
                    break;
                default:
                    if (Char.IsWhiteSpace(CurrentChar))
                    {
                        NextChar();
                        t = TokenType.Whitespace;
                        break;
                    }
                    if (IsValidIdentifierSymbol(true, CurrentChar))
                    {
                        do
                        {
                            NextChar();
                        } while (IsValidIdentifierSymbol(false, CurrentChar));
                        t = TokenType.Identifier;
                        break;
                    }
                    if (Char.IsDigit(CurrentChar))
                    {
                        t = TokenType.IntegerLiteral;
                        do
                        {
                            NextChar();
                        } while (Char.IsDigit(CurrentChar));
                        if (CurrentChar == '.')
                        {
                            NextChar();
                            if (!Char.IsDigit(CurrentChar))
                            {
                                CurrentChar = '.';
                                --Position;
                                break;
                            }
                            t = TokenType.RealLiteral;
                            if (!ValidateDigit())
                            {
                                t = TokenType.Unknown;
                                break;
                            }
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(CurrentChar));
                        }
                        if (CurrentChar == 'E' || CurrentChar == 'e')
                        {
                            t = TokenType.RealLiteral;
                            NextChar();
                            if (CurrentChar == '+' || CurrentChar == '-')
                                NextChar();
                            if (!ValidateDigit())
                            {
                                t = TokenType.Unknown;
                                break;
                            }
                            do
                            {
                                NextChar();
                            } while (Char.IsDigit(CurrentChar));
                        }
                        if (RealSuffixes.Contains(CurrentChar))
                        {
                            t = TokenType.RealLiteral;
                            NextChar();
                        }
                        else if (IntegerSuffixes.Contains(CurrentChar))
                        {
                            if (t != TokenType.IntegerLiteral)
                            {
                                if (_throwOnError)
                                    throw BindingExceptionManager.UnexpectedCharacterParser(CurrentChar, this, Source);
                                t = TokenType.Unknown;
                                break;
                            }
                            char c = CurrentChar;
                            NextChar();
                            if ((c == 'U' || c == 'u') && (CurrentChar == 'L' || CurrentChar == 'l'))
                                NextChar();
                        }
                        break;
                    }
                    if (Position == Length)
                    {
                        t = TokenType.Eof;
                        break;
                    }
                    if (_throwOnError)
                        throw BindingExceptionManager.UnexpectedCharacterParser(CurrentChar, this, Source);
                    t = TokenType.Unknown;
                    NextChar();
                    break;
            }
            return t;
        }

        protected virtual bool IsValidIdentifierSymbol(bool firstSymbol, char symbol)
        {
            if (firstSymbol)
                return Char.IsLetter(symbol) || symbol == '@' || symbol == '_';
            return Char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_';
        }

        private bool ValidateDigit()
        {
            if (Char.IsDigit(CurrentChar))
                return true;
            if (_throwOnError)
                throw BindingExceptionManager.DigitExpected(CurrentChar, this, Source);
            return false;
        }

        #endregion
    }
}
