#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberExpressionNode.cs">
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
            Should.NotBeNull(paramName, nameof(paramName));
            _parameterName = paramName;
            _index = index;
        }

        public BindingMemberExpressionNode([NotNull] IRelativeSourceExpressionNode relativeSource,
            [NotNull] string paramName, int index)
            : this(paramName, index)
        {
            Should.NotBeNull(relativeSource, nameof(relativeSource));
            _relativeSourceExpression = relativeSource;
            _path = relativeSource.Path;
        }

        public BindingMemberExpressionNode([NotNull] string path, [NotNull] string paramName, int index)
            : this(paramName, index)
        {
            Should.NotBeNull(path, nameof(path));
            _path = path;
        }

        public BindingMemberExpressionNode([NotNull] string resourceName, [NotNull] string path,
            [NotNull] string paramName, int index)
            : this(paramName, index)
        {
            Should.NotBeNull(resourceName, nameof(resourceName));
            Should.NotBeNull(path, nameof(path));
            _resourceName = resourceName;
            _path = path;
            _isDynamic = true;
        }

        #endregion

        #region Properties

        public bool IsRelativeSource => RelativeSourceExpression != null;

        public IRelativeSourceExpressionNode RelativeSourceExpression => _relativeSourceExpression;

        public string ParameterName => _parameterName;

        public int Index => _index;

        public string ResourceName => _resourceName;

        public string Path => _path;

        public bool IsDynamic => _isDynamic;

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
            if (RelativeSourceExpression != null)
                _relativeSourceExpression = AcceptWithCheck(visitor, RelativeSourceExpression, true);
        }

        protected override IExpressionNode CloneInternal()
        {
            if (IsRelativeSource)
                return new BindingMemberExpressionNode(RelativeSourceExpression, ParameterName, Index);
            return new BindingMemberExpressionNode(Path, ParameterName, Index);
        }

        public override string ToString()
        {
            return ParameterName;
        }

        #endregion
    }
}
