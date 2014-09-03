#region Copyright
// ****************************************************************************
// <copyright file="RelativeSourceExpressionNode.cs">
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
    /// <summary>
    ///     Represents accessing a relative source.
    /// </summary>
    public class RelativeSourceExpressionNode : ExpressionNode, IRelativeSourceExpressionNode
    {
        #region Fields

        public const string RelativeSourceType = "RelativeSource";
        public const string ElementSourceType = "ElementSource";
        public const string SelfType = "Self";
        public const string MemberSourceType = "MemberSource";

        private string _elementName;
        private uint _level;
        private string _path;
        private string _type;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelativeSourceExpressionNode" /> class.
        /// </summary>
        private RelativeSourceExpressionNode()
            : base(ExpressionNodeType.RelativeSource)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelativeSourceExpressionNode" /> class.
        /// </summary>
        public RelativeSourceExpressionNode(string path, bool isSelf)
            : base(ExpressionNodeType.RelativeSource)
        {
            _type = isSelf ? SelfType : MemberSourceType;
            _path = path;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelativeSourceExpressionNode" /> class.
        /// </summary>
        public RelativeSourceExpressionNode([NotNull] string elementName, string path)
            : base(ExpressionNodeType.RelativeSource)
        {
            Should.NotBeNullOrWhitespace(elementName, "elementName");
            _type = ElementSourceType;
            _elementName = elementName;
            _path = path;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelativeSourceExpressionNode" /> class.
        /// </summary>
        public RelativeSourceExpressionNode([NotNull] string type, uint level, string path)
            : base(ExpressionNodeType.RelativeSource)
        {
            Should.NotBeNullOrWhitespace(type, "type");
            _type = type;
            _level = level;
            _path = path;
        }

        #endregion

        #region Overrides of ExpressionNode

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        /// <param name="visitor">The visitor to visit this node with.</param>
        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        protected override IExpressionNode CloneInternal()
        {
            return new RelativeSourceExpressionNode
            {
                _type = Type,
                _path = Path,
                _elementName = ElementName,
                _level = Level
            };
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (Type == SelfType)
            {
                if (string.IsNullOrEmpty(Path))
                    return "{RelativeSource Self}";
                return string.Format("{{RelativeSource Self, Path={0}}}", Path);
            }
            if (Type == ElementSourceType)
            {
                if (string.IsNullOrEmpty(Path))
                    return string.Format("{{ElementSource {0}}}", ElementName);
                return string.Format("{{ElementSource {0}, Path={1}}}", ElementName, Path);
            }
            if (string.IsNullOrEmpty(Path))
                return string.Format("{{RelativeSource {0}, Level={1}}}", ElementName, Level.ToString());
            return string.Format("{{RelativeSource {0}, Path={1}, Level={2}}}", Type, Path, Level.ToString());
        }

        #endregion

        #region Implementation of IRelativeSourceExpressionNode

        /// <summary>
        ///     Gets the type of relative source.
        /// </summary>
        public string Type
        {
            get { return _type; }
        }

        /// <summary>
        ///     Gets the element name, if any.
        /// </summary>
        public string ElementName
        {
            get { return _elementName; }
        }

        /// <summary>
        ///     Gets the path, if any.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        ///     Gets the level.
        /// </summary>
        public uint Level
        {
            get { return _level; }
        }

        /// <summary>
        /// Merges the current path with specified.
        /// </summary>        
        public void MergePath(string path)
        {
            _path = BindingExtensions.MergePath(_path, path);
        }

        #endregion
    }
}