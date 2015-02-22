#region Copyright

// ****************************************************************************
// <copyright file="RelativeSourceExpressionNode.cs">
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

using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Parse.Nodes
{
    /// <summary>
    ///     Represents accessing a relative source.
    /// </summary>
    public sealed class RelativeSourceExpressionNode : ExpressionNode, IRelativeSourceExpressionNode
    {
        #region Fields

        public const string RelativeSourceType = "RelativeSource";
        public const string ElementSourceType = "ElementSource";
        public const string SelfType = "Self";
        public const string ContextSourceType = "ContextSource";
        
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

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of <see cref="RelativeSourceExpressionNode" /> with value {ElementSource Element, Path=Value}
        /// </summary>
        public static RelativeSourceExpressionNode CreateRelativeSource(string type, uint level, string path)
        {
            Should.NotBeNull(type, "type");
            return new RelativeSourceExpressionNode
            {
                _type = type,
                _level = level,
                _path = path
            };
        }

        /// <summary>
        ///     Creates an instance of <see cref="RelativeSourceExpressionNode" /> with value {ElementSource Element, Path=Value}
        /// </summary>
        public static RelativeSourceExpressionNode CreateElementSource(string elementName, string path)
        {
            return new RelativeSourceExpressionNode
            {
                _type = ElementSourceType,
                _elementName = elementName,
                _path = path
            };
        }

        /// <summary>
        ///     Creates an instance of <see cref="RelativeSourceExpressionNode" /> with  value {RelativeSource Self, Path=Value}
        /// </summary>
        public static RelativeSourceExpressionNode CreateSelfSource(string path)
        {
            return new RelativeSourceExpressionNode { _type = SelfType, _path = path };
        }

        /// <summary>
        ///     Creates an instance of <see cref="RelativeSourceExpressionNode" /> with value {RelativeSource Self, Path=DataContext.Value}
        /// </summary>
        public static RelativeSourceExpressionNode CreateBindingContextSource(string path)
        {
            return new RelativeSourceExpressionNode { _type = ContextSourceType, _path = path };
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