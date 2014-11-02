#region Copyright
// ****************************************************************************
// <copyright file="RelativeSourcePathMergerVisitor.cs">
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
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Parse.Nodes;

namespace MugenMvvmToolkit.Binding.Parse
{
    /// <summary>
    ///    Represents the expression visitor that allows to merge relative source path: 
    ///    {Relative ControlType}.Test.Value to {Relative ControlType, Path=Test.Value}, $Relative(Type, Level).Value to {Relative ControlType, Level=Level, Path=Value}
    ///    {Element Name}.Value to {Element Name, Path=Value}, $Element(Name).Value to {Element Name, Path=Value}
    /// </summary>
    public class RelativeSourcePathMergerVisitor : IExpressionVisitor
    {
        #region Fields

        /// <summary>
        ///     Gets an instance of <see cref="RelativeSourcePathMergerVisitor" />.
        /// </summary>
        public static RelativeSourcePathMergerVisitor Instance;

        private static readonly ICollection<string> DefaultElementSourceAliases;

        private static readonly ICollection<string> DefaultRelativeSourceAliases;

        #endregion

        #region Constructors

        static RelativeSourcePathMergerVisitor()
        {
            Instance = new RelativeSourcePathMergerVisitor();
            DefaultElementSourceAliases = new[]
            {
                RelativeSourceExpressionNode.ElementSourceType,
                "Element",
                "El"
            };
            DefaultRelativeSourceAliases = new[]
            {
                RelativeSourceExpressionNode.RelativeSourceType,
                "Relative",
                "Rel"
            };
        }

        private RelativeSourcePathMergerVisitor()
        {
        }

        #endregion

        #region Methods

        private static ICollection<string> RelativeSourceAliases
        {
            get
            {
                var bindingParser = BindingServiceProvider.BindingProvider.Parser as BindingParser;
                if (bindingParser == null)
                    return DefaultRelativeSourceAliases;
                return bindingParser.RelativeSourceAliases;
            }
        }

        private static ICollection<string> ElementSourceAliases
        {
            get
            {
                var bindingParser = BindingServiceProvider.BindingProvider.Parser as BindingParser;
                if (bindingParser == null)
                    return DefaultElementSourceAliases;
                return bindingParser.ElementSourceAliases;
            }
        }


        #endregion

        #region Implementation of IExpressionVisitor

        /// <summary>
        ///     Dispatches the expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public IExpressionNode Visit(IExpressionNode node)
        {
            var nodes = new List<IExpressionNode>();
            var members = new List<string>();
            string memberName = node.TryGetMemberName(true, true, nodes, members);
            if (memberName == null)
            {
                var relativeExp = nodes[0] as IRelativeSourceExpressionNode;
                if (relativeExp != null)
                {
                    relativeExp.MergePath(string.Join(".", members));
                    return relativeExp;
                }

                var methodCall = nodes[0] as MethodCallExpressionNode;
                if (methodCall != null && methodCall.Target is ResourceExpressionNode)
                {
                    if (RelativeSourceAliases.Contains(methodCall.Method))
                    {
                        if ((methodCall.Arguments.Count == 1 || methodCall.Arguments.Count == 2) && methodCall.Arguments[0] is IMemberExpressionNode)
                        {
                            int level = 1;
                            var relativeType = (IMemberExpressionNode)methodCall.Arguments[0];
                            if (methodCall.Arguments.Count == 2)
                                level = (int)((IConstantExpressionNode)methodCall.Arguments[1]).Value;
                            return new RelativeSourceExpressionNode(relativeType.Member, (uint)level, string.Join(".", members));
                        }
                    }

                    if (ElementSourceAliases.Contains(methodCall.Method))
                    {
                        if (methodCall.Arguments.Count == 1 && methodCall.Arguments[0] is IMemberExpressionNode)
                        {
                            var elementSource = (IMemberExpressionNode)methodCall.Arguments[0];
                            return new RelativeSourceExpressionNode(elementSource.Member, string.Join(".", members));
                        }
                    }
                }
            }
            return node;
        }

        #endregion
    }
}