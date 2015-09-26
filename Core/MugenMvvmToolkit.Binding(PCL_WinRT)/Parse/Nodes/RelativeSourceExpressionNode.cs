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

        private RelativeSourceExpressionNode()
            : base(ExpressionNodeType.RelativeSource)
        {
        }

        #endregion

        #region Methods

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

        public static RelativeSourceExpressionNode CreateElementSource(string elementName, string path)
        {
            return new RelativeSourceExpressionNode
            {
                _type = ElementSourceType,
                _elementName = elementName,
                _path = path
            };
        }

        public static RelativeSourceExpressionNode CreateSelfSource(string path)
        {
            return new RelativeSourceExpressionNode { _type = SelfType, _path = path };
        }

        public static RelativeSourceExpressionNode CreateBindingContextSource(string path)
        {
            return new RelativeSourceExpressionNode { _type = ContextSourceType, _path = path };
        }

        #endregion

        #region Overrides of ExpressionNode

        protected override void AcceptInternal(IExpressionVisitor visitor)
        {
        }

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

        public string Type
        {
            get { return _type; }
        }

        public string ElementName
        {
            get { return _elementName; }
        }

        public string Path
        {
            get { return _path; }
        }

        public uint Level
        {
            get { return _level; }
        }

        public void MergePath(string path)
        {
            _path = BindingExtensions.MergePath(_path, path);
        }

        #endregion
    }
}
