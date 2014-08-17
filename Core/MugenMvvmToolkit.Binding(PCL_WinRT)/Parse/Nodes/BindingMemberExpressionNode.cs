#region Copyright
// ****************************************************************************
// <copyright file="BindingMemberExpressionNode.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    public class BindingMemberExpressionNode : ExpressionNode
    {
        #region Fields

        private readonly int _index;
        private readonly bool _isDynamic;
        private readonly string _parameterName;
        private readonly string _path;
        private readonly string _resourceName;
        private IRelativeSourceExpressionNode _relativeSourceExpression;

        #endregion

        #region Constructors

        private BindingMemberExpressionNode([NotNull] string paramName, int index)
            : base(ExpressionNodeType.BindingMember)
        {
            Should.NotBeNull(paramName, "paramName");
            _parameterName = paramName;
            _index = index;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberExpressionNode" /> class.
        /// </summary>
        public BindingMemberExpressionNode([NotNull] IRelativeSourceExpressionNode relativeSource,
            [NotNull] string paramName, int index)
            : this(paramName, index)
        {
            Should.NotBeNull(relativeSource, "relativeSource");
            _relativeSourceExpression = relativeSource;
            _path = relativeSource.Path;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberExpressionNode" /> class.
        /// </summary>
        public BindingMemberExpressionNode([NotNull] string path, [NotNull] string paramName, int index)
            : this(paramName, index)
        {
            Should.NotBeNull(path, "path");
            _path = path;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingMemberExpressionNode" /> class.
        /// </summary>
        public BindingMemberExpressionNode([NotNull] string resourceName, [NotNull] string path,
            [NotNull] string paramName, int index)
            : this(paramName, index)
        {
            Should.NotBeNull(resourceName, "resourceName");
            Should.NotBeNull(path, "path");
            _resourceName = resourceName;
            _path = path;
            _isDynamic = true;
        }

        #endregion

        #region Properties

        public bool IsRelativeSource
        {
            get { return RelativeSourceExpression != null; }
        }

        public IRelativeSourceExpressionNode RelativeSourceExpression
        {
            get { return _relativeSourceExpression; }
        }

        public string ParameterName
        {
            get { return _parameterName; }
        }

        public int Index
        {
            get { return _index; }
        }

        public string ResourceName
        {
            get { return _resourceName; }
        }

        public string Path
        {
            get { return _path; }
        }

        public bool IsDynamic
        {
            get { return _isDynamic; }
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (RelativeSourceExpression != null)
                _relativeSourceExpression = AcceptWithCheck(visitor, RelativeSourceExpression, true);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            if (IsRelativeSource)
                return new BindingMemberExpressionNode(RelativeSourceExpression, ParameterName, Index);
            return new BindingMemberExpressionNode(Path, ParameterName, Index);
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return ParameterName;
        }

        #endregion
    }
}