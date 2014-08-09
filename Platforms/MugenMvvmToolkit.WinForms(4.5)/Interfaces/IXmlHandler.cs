#region Copyright
// ****************************************************************************
// <copyright file="IXmlHandler.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;

namespace MugenMvvmToolkit.Interfaces
{
    internal interface IXmlHandler
    {
        bool CanAutoComplete(bool textChanged);

        ICollection<AutoCompleteItem> ProvideAutoCompleteInfo(out int startIndexToReplace, out int endIndexToReplace);

        void HighlightNode(XmlExpressionNode node);
    }
}