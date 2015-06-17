#region Copyright

// ****************************************************************************
// <copyright file="XmlTokens.cs">
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

namespace MugenMvvmToolkit.WinForms.Binding.Parse
{
    internal static class XmlTokens
    {
        #region Fields

        public static readonly TokenType StartComment;

        public static readonly TokenType EndComment;

        public static readonly TokenType CloseElement;

        public static readonly TokenType ComplexCloseElement;

        #endregion

        #region Constructors

        static XmlTokens()
        {
            StartComment = new TokenType("StartComment", "<!--");
            EndComment = new TokenType("EndComment", "-->");
            CloseElement = new TokenType("CloseElement", "/>");
            ComplexCloseElement = new TokenType("ComplexCloseElement", "</");
        }

        #endregion
    }
}