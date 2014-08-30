#region Copyright
// ****************************************************************************
// <copyright file="MethodCallExpressionNode.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    /// <summary>
    ///     Represents a call to either static or an instance method.
    /// </summary>
    public class MethodCallExpressionNode : ExpressionNode, IMethodCallExpressionNode
    {
        #region Fields

        private readonly IList<IExpressionNode> _arguments;
        private readonly string _method;
        private readonly IList<string> _typeArgs;
        private IExpressionNode _target;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MethodCallExpressionNode" /> class.
        /// </summary>
        public MethodCallExpressionNode(IExpressionNode target, [NotNull] string methodName,
            [CanBeNull] IList<IExpressionNode> args, [CanBeNull] IList<string> typeArgs)
            : base(ExpressionNodeType.MethodCall)
        {
            Should.NotBeNull(methodName, "methodName");
            _target = target;
            _method = methodName;
            _typeArgs = typeArgs == null
                ? Empty.Array<string>()
                : typeArgs.ToArrayFast();
            _arguments = args == null ? Empty.Array<IExpressionNode>() : args.ToArrayFast();
        }

        #endregion

        #region Implementation of IMethodCallExpressionNode

        /// <summary>
        ///     Gets the type arguments for the method.
        /// </summary>
        public IList<string> TypeArgs
        {
            get { return _typeArgs; }
        }

        /// <summary>
        ///     Gets the method name for the method to be called.
        /// </summary>
        public string Method
        {
            get { return _method; }
        }

        /// <summary>
        ///     Gets the expression that represents the instance for instance method calls or null for static method calls.
        /// </summary>
        public IExpressionNode Target
        {
            get { return _target; }
        }

        /// <summary>
        ///     Gets a collection of expressions that represent arguments of the called method.
        /// </summary>
        public IList<IExpressionNode> Arguments
        {
            get { return _arguments; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Calls the visitor on the expression.
        /// </summary>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (Target != null)
                _target = AcceptWithCheck(visitor, Target, false);
            for (int index = 0; index < _arguments.Count; index++)
            {
                _arguments[index] = AcceptWithCheck(visitor, _arguments[index], true);
            }
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new MethodCallExpressionNode(Target == null ? null : Target.Clone(), Method,
                Arguments.ToArrayFast(node => node.Clone()), TypeArgs);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
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