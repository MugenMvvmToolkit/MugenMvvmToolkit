#region Copyright

// ****************************************************************************
// <copyright file="MacrosExpressionVisitor.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public class MacrosExpressionVisitor : IExpressionVisitor
    {
        #region Fields

        public static readonly MacrosExpressionVisitor Instance;

        #endregion

        #region Constructors

        static MacrosExpressionVisitor()
        {
            Instance = new MacrosExpressionVisitor();
        }

        private MacrosExpressionVisitor()
        {
        }

        #endregion

        #region Properties

        private static ICollection<string> RelativeSourceAliases => BindingServiceProvider.BindingProvider.Parser.RelativeSourceAliases;

        private static ICollection<string> ElementSourceAliases => BindingServiceProvider.BindingProvider.Parser.ElementSourceAliases;

        public bool IsPostOrder => false;

        #endregion

        #region Implementation of IExpressionVisitor

        public IExpressionNode Visit(IExpressionNode node)
        {
            var member = node as IMemberExpressionNode;
            if (member != null && member.Target is ResourceExpressionNode)
            {
                //$self, $this --> $BindingServiceProvider.ResourceResolver.SelfResourceName
                if (member.Member == "self" || member.Member == "this")
                    return new MemberExpressionNode(member.Target, BindingServiceProvider.ResourceResolver.SelfResourceName);
                //$context --> $BindingServiceProvider.ResourceResolver.DataContextResourceName
                if (member.Member == "context")
                    return new MemberExpressionNode(member.Target, BindingServiceProvider.ResourceResolver.DataContextResourceName);
                //$args, $arg --> $GetEventArgs()
                if (member.Member == "args" || member.Member == "arg")
                    return new MethodCallExpressionNode(member.Target, DefaultBindingParserHandler.GetEventArgsMethod, null, null);
                //$binding --> $GetBinding()
                if (member.Member == "binding")
                    return new MethodCallExpressionNode(member.Target, DefaultBindingParserHandler.GetBindingMethod, null, null);
            }

            var methodCallExp = node as IMethodCallExpressionNode;
            if (methodCallExp != null && methodCallExp.Target is ResourceExpressionNode)
            {
                //$OneTime(Expression) --> oneTimeImpl.GetValue(GetBinding(), () => Expression)
                if (methodCallExp.Method == "OneTime" && methodCallExp.Arguments.Count == 1)
                {
                    DataConstant<object> constant = Guid.NewGuid().ToString("n");
                    var idEx = new ConstantExpressionNode(constant);
                    var getBindEx = new MethodCallExpressionNode(ResourceExpressionNode.DynamicInstance, DefaultBindingParserHandler.GetBindingMethod, null, null);
                    IExpressionNode getValueEx = new LambdaExpressionNode(methodCallExp.Arguments[0], null);
                    return new MethodCallExpressionNode(new ConstantExpressionNode(typeof(BindingExtensions)), "GetOrAddValue", new[]
                    {
                        getBindEx, idEx, getValueEx
                    }, null).Accept(this);
                }

                //Alias ($Format(), $MethodName, etc) --> type.Format()
                Type type;
                string method;
                if (BindingServiceProvider.ResourceResolver.TryGetMethodAlias(methodCallExp.Method, out type, out method))
                    return new MethodCallExpressionNode(new ConstantExpressionNode(type), method, methodCallExp.Arguments, methodCallExp.TypeArgs).Accept(this);
            }

            var nodes = new List<IExpressionNode>();
            var members = new List<string>();
            string memberName = node.TryGetMemberName(true, true, nodes, members);
            if (memberName == null)
            {
                var relativeExp = nodes[0] as IRelativeSourceExpressionNode;
                if (relativeExp != null)
                {
                    relativeExp.MergePath(BindingExtensions.MergePath(members));
                    return relativeExp;
                }

                var methodCall = nodes[0] as IMethodCallExpressionNode;
                if (methodCall != null && methodCall.Target is ResourceExpressionNode)
                {
                    if (RelativeSourceAliases.Contains(methodCall.Method))
                    {
                        if ((methodCall.Arguments.Count == 1 || methodCall.Arguments.Count == 2) &&
                            methodCall.Arguments[0] is IMemberExpressionNode)
                        {
                            int level = 1;
                            var relativeType = (IMemberExpressionNode)methodCall.Arguments[0];
                            if (methodCall.Arguments.Count == 2)
                                level = (int)((IConstantExpressionNode)methodCall.Arguments[1]).Value;
                            return RelativeSourceExpressionNode.CreateRelativeSource(relativeType.Member, (uint)level,
                                BindingExtensions.MergePath(members));
                        }
                    }

                    if (ElementSourceAliases.Contains(methodCall.Method))
                    {
                        if (methodCall.Arguments.Count == 1 && methodCall.Arguments[0] is IMemberExpressionNode)
                        {
                            var elementSource = (IMemberExpressionNode)methodCall.Arguments[0];
                            return RelativeSourceExpressionNode.CreateElementSource(elementSource.Member,
                                BindingExtensions.MergePath(members));
                        }
                    }
                }
            }
            return node;
        }

        #endregion
    }
}
