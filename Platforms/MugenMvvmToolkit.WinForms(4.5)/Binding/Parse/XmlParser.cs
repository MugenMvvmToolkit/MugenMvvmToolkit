#region Copyright

// ****************************************************************************
// <copyright file="XmlParser.cs">
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

using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Parse
{
    internal sealed class XmlParser
    {
        #region Fields

        private static readonly HashSet<TokenType> EndElementTokens;

        private readonly XmlTokenizer _tokenizer;

        #endregion

        #region Constructors

        static XmlParser()
        {
            EndElementTokens = new HashSet<TokenType>
            {
                XmlTokens.CloseElement,
                TokenType.GreaterThan
            };
        }

        public XmlParser()
        {
            _tokenizer = new XmlTokenizer();
        }

        #endregion

        #region Methods

        public List<XmlExpressionNode> Parse(string xmlText)
        {
            _tokenizer.SetSource(xmlText);
            _tokenizer.NextToken(true);
            var xmlExpressionNodes = new List<XmlExpressionNode>();
            while (_tokenizer.Token != TokenType.Eof)
            {
                XmlExpressionNode parse = TryParse();
                xmlExpressionNodes.Add(parse);
                if (_tokenizer.Token != TokenType.LessThan && _tokenizer.Token != XmlTokens.StartComment)
                    _tokenizer.NextToken(true);
            }
            return xmlExpressionNodes;
        }

        private XmlExpressionNode TryParse()
        {
            if (_tokenizer.Token == TokenType.Whitespace)
                _tokenizer.NextToken(true);
            if (_tokenizer.Token == XmlTokens.StartComment)
                return TryParseComment();
            if (_tokenizer.Token == TokenType.LessThan)
                return TryParseElement();


            int startPosition = _tokenizer.FirstCharPosition;
            while ((_tokenizer.Token != XmlTokens.StartComment || _tokenizer.Token != TokenType.LessThan) &&
                   _tokenizer.Token != TokenType.Eof)
                _tokenizer.NextToken(true);
            return InvalidExpression(XmlInvalidExpressionType.Unknown, startPosition);
        }

        private XmlExpressionNode TryParseElement()
        {
            int startPosition = _tokenizer.FirstCharPosition;
            var tagEnd = _tokenizer.Position;
            _tokenizer.NextToken(false);
            if (!IsXmlIdentifier())
            {
                var node = new XmlValueExpressionNode(XmlValueExpressionType.ElementStartTag, startPosition, tagEnd);
                return InvalidExpression(XmlInvalidExpressionType.Element, startPosition, node);
            }

            var startTag = new XmlValueExpressionNode(XmlValueExpressionType.ElementStartTag, startPosition,
                _tokenizer.Position);
            string nameString = _tokenizer.Value;
            _tokenizer.NextToken(true);
            var elementNode = new XmlElementExpressionNode(startTag, nameString, startPosition, -1);
            while (_tokenizer.Token != TokenType.Eof)
            {
                if (IsXmlIdentifier())
                {
                    XmlExpressionNode attribute = TryParseAttribute(elementNode);
                    elementNode.AddAttribute(attribute);
                    continue;
                }

                if (_tokenizer.Token == XmlTokens.CloseElement)
                {
                    elementNode.UpdateCloseTag(
                        new XmlValueExpressionNode(XmlValueExpressionType.ElementEndTag, _tokenizer.FirstCharPosition,
                            _tokenizer.Position),
                        _tokenizer.Position);
                    _tokenizer.NextToken(false);
                    return elementNode;
                }

                if (_tokenizer.Token == TokenType.GreaterThan)
                {
                    elementNode.UpdateStartTagEnd(new XmlValueExpressionNode(XmlValueExpressionType.ElementStartTagEnd,
                        _tokenizer.FirstCharPosition,
                        _tokenizer.Position));
                    _tokenizer.NextToken(false);
                    IList<XmlExpressionNode> nodes = TryParseElementValue();
                    for (int i = 0; i < nodes.Count; i++)
                        elementNode.AddElement(nodes[i]);

                    int closeElementPosition = _tokenizer.FirstCharPosition;
                    if (_tokenizer.Token != XmlTokens.ComplexCloseElement)
                        return InvalidExpression(XmlInvalidExpressionType.Element, startPosition, elementNode);
                    _tokenizer.NextToken(false);
                    if (_tokenizer.Value != nameString)
                        return InvalidExpression(XmlInvalidExpressionType.Element, startPosition, elementNode);
                    if (_tokenizer.NextToken(false) != TokenType.GreaterThan)
                        return InvalidExpression(XmlInvalidExpressionType.Element, startPosition, elementNode);
                    elementNode.UpdateCloseTag(
                        new XmlValueExpressionNode(XmlValueExpressionType.ElementEndTag, closeElementPosition,
                            _tokenizer.Position),
                        _tokenizer.Position);
                    _tokenizer.NextToken(false);
                    return elementNode;
                }
                if (_tokenizer.Token == TokenType.Whitespace)
                    _tokenizer.NextToken(true);
                return InvalidExpression(XmlInvalidExpressionType.Element, startPosition, elementNode);
            }
            return InvalidExpression(XmlInvalidExpressionType.Element, startPosition, elementNode);
        }

        private IList<XmlExpressionNode> TryParseElementValue()
        {
            var nodes = new List<XmlExpressionNode>();
            while (_tokenizer.Token != TokenType.Eof)
            {
                if (_tokenizer.Token == XmlTokens.ComplexCloseElement)
                    return nodes;

                if (_tokenizer.Token == TokenType.LessThan)
                {
                    nodes.Add(TryParseElement());
                    continue;
                }

                if (_tokenizer.Token == XmlTokens.StartComment)
                {
                    nodes.Add(TryParseComment());
                    continue;
                }

                if (_tokenizer.Token == TokenType.Whitespace || _tokenizer.Token == TokenType.Unknown)
                {
                    _tokenizer.NextToken(true);
                    continue;
                }

                int position = _tokenizer.FirstCharPosition;
                while (_tokenizer.Token != XmlTokens.ComplexCloseElement && _tokenizer.Token != TokenType.LessThan &&
                       _tokenizer.Token != TokenType.Eof)
                    _tokenizer.NextToken(true);
                if (nodes.Count != 0 || _tokenizer.Token == TokenType.Eof)
                    nodes.Add(InvalidExpression(XmlInvalidExpressionType.ElementValue, position));
                else
                    nodes.Add(new XmlValueExpressionNode(XmlValueExpressionType.ElementValue, position,
                        _tokenizer.Position));
            }
            return nodes;
        }

        private XmlExpressionNode TryParseAttribute(XmlElementExpressionNode element)
        {
            int startPosition = _tokenizer.FirstCharPosition;
            var name = new XmlValueExpressionNode(element, XmlValueExpressionType.AttributeName, startPosition,
                _tokenizer.Position);

            if (_tokenizer.NextToken(true) != TokenType.Equal)
                return InvalidExpression(XmlInvalidExpressionType.Attribute, startPosition, EndElementTokens, name);
            var equal = new XmlValueExpressionNode(element, XmlValueExpressionType.AttributeEqual, name.End, _tokenizer.Position);

            if (_tokenizer.NextToken(true) != TokenType.StringLiteral)
                return InvalidExpression(XmlInvalidExpressionType.Attribute, startPosition, EndElementTokens, name,
                    equal);
            var value = new XmlValueExpressionNode(XmlValueExpressionType.AttributeValue, _tokenizer.FirstCharPosition,
                _tokenizer.Position);
            int position = _tokenizer.Position;
            _tokenizer.NextToken(true);
            return new XmlAttributeExpressionNode(name, equal, value, startPosition, position);
        }

        private XmlExpressionNode TryParseComment()
        {
            int startPosition = _tokenizer.FirstCharPosition;
            while (_tokenizer.Token != XmlTokens.EndComment && _tokenizer.Token != TokenType.Eof)
                _tokenizer.NextToken(true);
            if (_tokenizer.Token == TokenType.Eof)
                return InvalidExpression(XmlInvalidExpressionType.Comment, startPosition);
            int position = _tokenizer.Position;
            _tokenizer.NextToken(true);
            return new XmlCommentExpressionNode(startPosition, position);
        }

        private XmlInvalidExpressionNode InvalidExpression(XmlInvalidExpressionType type, int start)
        {
            return new XmlInvalidExpressionNode(type, start, _tokenizer.Position);
        }

        private XmlInvalidExpressionNode InvalidExpression(XmlInvalidExpressionType type, int start, params XmlExpressionNode[] nodes)
        {
            return new XmlInvalidExpressionNode(nodes, type, start, _tokenizer.Position);
        }

        private XmlInvalidExpressionNode InvalidExpression(XmlInvalidExpressionType type, int start, ICollection<TokenType> tokens, params XmlExpressionNode[] nodes)
        {
            while (!tokens.Contains(_tokenizer.Token) && _tokenizer.Token != TokenType.Eof)
                _tokenizer.NextToken(false);
            return new XmlInvalidExpressionNode(nodes, type, start, _tokenizer.Position);
        }

        private bool IsXmlIdentifier()
        {
            return _tokenizer.Token == TokenType.Identifier;
        }

        #endregion
    }
}