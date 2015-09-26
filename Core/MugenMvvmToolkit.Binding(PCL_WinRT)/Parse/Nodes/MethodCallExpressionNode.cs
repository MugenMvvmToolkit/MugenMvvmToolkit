#region Copyright

// ****************************************************************************
// <copyright file="MethodCallExpressionNode.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public class MethodCallExpressionNode : ExpressionNode, IMethodCallExpressionNode
    {
        #region Fields

        private readonly IList<IExpressionNode> _arguments;
        private readonly string _method;
        private readonly IList<string> _typeArgs;
        private IExpressionNode _target;

        #endregion

        #region Constructors

        public MethodCallExpressionNode(IExpressionNode target, [NotNull] string methodName,
            [CanBeNull] IList<IExpressionNode> args, [CanBeNull] IList<string> typeArgs)
            : base(ExpressionNodeType.MethodCall)
        {
            Should.NotBeNull(methodName, "methodName");
            _target = target;
            _method = methodName;
            _typeArgs = typeArgs == null
                ? Empty.Array<string>()
                : typeArgs.ToArrayEx();
            _arguments = args == null ? Empty.Array<IExpressionNode>() : args.ToArrayEx();
        }

        #endregion

        #region Implementation of IMethodCallExpressionNode

        public IList<string> TypeArgs
        {
            get { return _typeArgs; }
        }

        public string Method
        {
            get { return _method; }
        }

        public IExpressionNode Target
        {
            get { return _target; }
        }

        public IList<IExpressionNode> Arguments
        {
            get { return _arguments; }
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (Target != null)
                _target = AcceptWithCheck(visitor, Target, false);
            for (int index = 0; index < _arguments.Count; index++)
            {
                _arguments[index] = AcceptWithCheck(visitor, _arguments[index], true);
            }
        }

        protected override IExpressionNode CloneInternal()
        {
            return new MethodCallExpressionNode(Target == null ? null : Target.Clone(), Method,
                Arguments.ToArrayEx(node => node.Clone()), TypeArgs);
        }

        public override string ToString()
        {
            string typeArgs = null;
            if (TypeArgs.Count != 0)
                typeArgs = string.Format("<{0}>", string.Join(", ", TypeArgs));
            string @join = string.Join(",", Arguments);
            if (Target == null)
                return string.Format("{0}{1}({2})", Method, typeArgs, join);
            return string.Format("{0}.{1}{2}({3})", Target, Method, typeArgs, join);
        }

        #endregion
    }
}
