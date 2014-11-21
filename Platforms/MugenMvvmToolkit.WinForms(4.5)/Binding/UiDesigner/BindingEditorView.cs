#region Copyright
// ****************************************************************************
// <copyright file="BindingEditorView.cs">
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Binding.UiDesigner
{
    /// <summary>
    ///     Represents the data binding editor.
    /// </summary>
    public partial class BindingEditorView : Form, IXmlHandler
    {
        #region Fields

        private static readonly Color KnownControlColor;
        private static readonly Color UnknownControlColor;
        private static readonly Color CommentColor;

        private static readonly Color ValueColor;
        private static readonly Color PropertyColor;
        private static readonly Color EventColor;
        private static readonly Color AttachedMemberColor;
        private static readonly string[] DotSeparator;

        private readonly SortedDictionary<string, AutoCompleteItem> _controlsCompleteItems;
        private readonly SortedDictionary<string, SortedDictionary<string, AutoCompleteItem>> _controlsDictionary;
        private readonly AutoCompleteItem[] _attachedControlAutoCompleteItems;
        private readonly Dictionary<Type, SortedDictionary<string, AutoCompleteItem>> _typeCompleteItems;

        private readonly Font _boldFont;

        private XmlElementExpressionNode _lastElement;
        private XmlValueExpressionNode _lastValueNode;

        #endregion

        #region Constructors

        static BindingEditorView()
        {
            DotSeparator = new[] { "." };
            KnownControlColor = ColorTranslator.FromHtml("#00008A");
            UnknownControlColor = Color.Red;
            CommentColor = ColorTranslator.FromHtml("#FF007E27");
            ValueColor = ColorTranslator.FromHtml("#0000EA");
            PropertyColor = ColorTranslator.FromHtml("#9E277E");
            EventColor = ColorTranslator.FromHtml("#FE00FE");
            AttachedMemberColor = ColorTranslator.FromHtml("#00889B");
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingEditorView" /> class.
        /// </summary>
        public BindingEditorView()
        {
            ServiceProvider.InitializeDesignTimeManager();
            InitializeComponent();
            bindingEditor.Handler = this;
            _boldFont = new Font(bindingEditor.Font, FontStyle.Bold);
            _controlsDictionary = new SortedDictionary<string, SortedDictionary<string, AutoCompleteItem>>(StringComparer.CurrentCulture);
            _controlsCompleteItems = new SortedDictionary<string, AutoCompleteItem>(StringComparer.CurrentCulture);
            _typeCompleteItems = new Dictionary<Type, SortedDictionary<string, AutoCompleteItem>>();
            _attachedControlAutoCompleteItems = BindingServiceProvider
                .MemberProvider
                .GetAttachedMembers(typeof(Control))
                .ToArrayEx(s => new AutoCompleteItem(s.Key, s.Key, MemberTypes.Custom));
            controlsTreeView.Nodes.Add(GetComponents(CurrentControl));
            controlsTreeView.ExpandAll();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingEditorView" /> class.
        /// </summary>
        public BindingEditorView(string xmlText)
            : this()
        {
            bindingEditor.SetBindingText(xmlText);
        }

        #endregion

        #region Properties

        public static Control CurrentControl { get; set; }

        public string BindingText
        {
            get { return bindingEditor.GetBindingText(); }
        }

        #endregion

        #region Methods

        private void TreeView_DoubleClick(object sender, EventArgs e)
        {
            var node = controlsTreeView.SelectedNode;
            if (node == null || node.Tag == null)
                return;
            var name = "<" + node.Tag + "  />";
            var newIndex = bindingEditor.SelectionStart + name.Length - 3;
            bindingEditor.SelectionLength = 0;
            bindingEditor.SelectedText = name;
            bindingEditor.SelectionStart = newIndex;
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            DialogResult = ((Button)sender).DialogResult;
            Close();
        }

        private TreeNode GetComponents(Control container)
        {
            if (container.Site == null || container.Site.Container == null)
                return new TreeNode("Empty");
            var treeNodes = new List<TreeNode>();
            foreach (var component in container.Site.Container.Components.OfType<IComponent>())
            {
                string name;
                Type type;
                if (AddCompleteItem(component, out name, out type) && container != component)
                    treeNodes.Add(new TreeNode(GetDisplayName(component, name, type)) { Tag = name });
            }
            return new TreeNode(GetDisplayName(container, container.Name, container.GetType()), treeNodes.ToArrayEx())
            {
                Tag = container.Name
            };
        }

        private bool AddCompleteItem(IComponent result, out string name, out Type type)
        {
            type = null;
            name = null;
            try
            {
                if (result is Binder)
                    return false;
                name = PlatformExtensions.GetComponentName(result);
                if (string.IsNullOrWhiteSpace(name))
                    return false;
                type = result.GetType();
                SortedDictionary<string, AutoCompleteItem> completeItems;
                if (!_controlsDictionary.TryGetValue(name, out completeItems))
                {
                    completeItems = new SortedDictionary<string, AutoCompleteItem>(StringComparer.CurrentCulture);
                    _controlsDictionary[name] = completeItems;
                }
                AddCompleteItems(type, completeItems);
                _controlsCompleteItems.Add(new AutoCompleteItem(string.Format("{0} ({1})", name, type.Name), name));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void AddCompleteItems(Type type, SortedDictionary<string, AutoCompleteItem> completeItems)
        {
            foreach (var member in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                completeItems.Add(new AutoCompleteItem(member));
            foreach (var member in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(info => info.GetIndexParameters().Length == 0))
                completeItems.Add(new AutoCompleteItem(member));
            foreach (var member in type.GetEvents(BindingFlags.Public | BindingFlags.Instance))
                completeItems.Add(new AutoCompleteItem(member));

            if (!type.IsValueType && !typeof(Delegate).IsAssignableFrom(type))
                completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.DataContext,
                    AttachedMemberConstants.DataContext, MemberTypes.Custom));
            foreach (var attachedName in BindingServiceProvider.MemberProvider.GetAttachedMembers(type))
            {
                if (!completeItems.ContainsKey(attachedName.Key) && XmlTokenizer.IsValidName(attachedName.Key))
                    completeItems.Add(new AutoCompleteItem(attachedName.Key, attachedName.Key, MemberTypes.Custom,
                        attachedName.Value.Type));
            }
        }

        private static string GetDisplayName(object instance, string name, Type type)
        {
            string text = instance == null ? null : PlatformExtensions.TryGetValue(instance, "Text");
            return string.Format("{0} ({1}{2})", name, type.Name, string.IsNullOrEmpty(text) ? "" : ", " + text);
        }

        private ICollection<AutoCompleteItem> FindControlMemberItems(Type type, string value)
        {
            SortedDictionary<string, AutoCompleteItem> items;
            if (!_typeCompleteItems.TryGetValue(type, out items))
            {
                items = new SortedDictionary<string, AutoCompleteItem>();
                AddCompleteItems(type, items);
                _typeCompleteItems[type] = items;
            }
            return FindItems(items, value);
        }

        private static ICollection<AutoCompleteItem> FindItems(SortedDictionary<string, AutoCompleteItem> items, string value)
        {
            if (string.IsNullOrEmpty(value))
                return items.Values;
            List<AutoCompleteItem> result = null;
            foreach (var item in items)
            {
                if (!item.Value.Value.SafeContains(value))
                    continue;
                if (result == null)
                    result = new List<AutoCompleteItem>();
                result.Add(item.Value);
            }
            return result;
        }

        private void HighlightMember(MemberTypes memberType, int startIndex, int length)
        {
            switch (memberType)
            {
                case MemberTypes.Event:
                    bindingEditor.Highlight(EventColor, startIndex, length);
                    break;
                case MemberTypes.Field:
                case MemberTypes.Property:
                    bindingEditor.Highlight(PropertyColor, startIndex, length);
                    break;
                case MemberTypes.Custom:
                    bindingEditor.Highlight(AttachedMemberColor, startIndex, length);
                    break;
            }
        }

        private ICollection<AutoCompleteItem> ProvideElementAutoCompleteItems(out int startIndexToReplace, out int endIndexToReplace)
        {
            startIndexToReplace = _lastValueNode.Start + 1;
            endIndexToReplace = _lastValueNode.End;

            string selectedName = null;
            var length = _lastValueNode.End - startIndexToReplace;
            if (length != 0)
                selectedName = bindingEditor.Text.Substring(startIndexToReplace, length);
            return FindItems(_controlsCompleteItems, selectedName);
        }

        private ICollection<AutoCompleteItem> ProvideAttributeAutoCompleteItems(XmlElementExpressionNode parent, ref int startIndexToReplace, ref int endIndexToReplace)
        {
            SortedDictionary<string, AutoCompleteItem> list;
            if (!_controlsDictionary.TryGetValue(parent.Name, out list))
                return null;

            var attributeName = _lastValueNode.GetValue(bindingEditor.Text);
            var members = attributeName.Split(DotSeparator, StringSplitOptions.None);
            var path = members[0];
            var cursorIndex = bindingEditor.SelectionStart - _lastValueNode.Start;
            AutoCompleteItem member;
            if (members.Length == 1 || path.Length >= cursorIndex || !list.TryGetValue(path, out member))
            {
                startIndexToReplace = _lastValueNode.Start;
                endIndexToReplace = startIndexToReplace + path.Length;
                return FindItems(list, attributeName.Substring(0, cursorIndex));
            }

            Type type = member.Type;
            int currentLength = members[0].Length;
            int startIndex = currentLength + 1;
            for (int i = 1; i < members.Length; i++)
            {
                path = members[i];
                currentLength += path.Length;
                if (startIndex + path.Length >= cursorIndex)
                {
                    startIndexToReplace = startIndex + _lastValueNode.Start;
                    endIndexToReplace = startIndexToReplace + path.Length;
                    if (!string.IsNullOrEmpty(path) && currentLength > cursorIndex)
                        path = path.Substring(0, currentLength - cursorIndex);
                    return FindControlMemberItems(type, path);
                }
                var bindingMember = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(type, path, false, false);
                type = bindingMember == null ? typeof(object) : bindingMember.Type;
                startIndex += path.Length + 1;
            }
            return null;
        }

        #endregion

        #region Implementation of IXmlHandler

        bool IXmlHandler.CanAutoComplete(bool textChanged)
        {
            var node = bindingEditor.GetNodeAt(bindingEditor.SelectionStart - 1);
            _lastValueNode = node as XmlValueExpressionNode;
            if (_lastValueNode != null)
                return _lastValueNode.Type == XmlValueExpressionType.ElementStartTag ||
                       _lastValueNode.Type == XmlValueExpressionType.AttributeName ||
                       _lastValueNode.Type == XmlValueExpressionType.AttributeValue;

            if (textChanged)
                return false;
            var invalidExpressionNode = node as XmlInvalidExpressionNode;
            if (invalidExpressionNode != null && invalidExpressionNode.Nodes != null)
                _lastElement = invalidExpressionNode.Nodes.OfType<XmlElementExpressionNode>().FirstOrDefault();
            else
                _lastElement = node as XmlElementExpressionNode;
            return _lastElement != null;
        }

        ICollection<AutoCompleteItem> IXmlHandler.ProvideAutoCompleteInfo(out int startIndexToReplace, out int endIndexToReplace)
        {
            startIndexToReplace = bindingEditor.SelectionStart;
            endIndexToReplace = bindingEditor.SelectionStart;
            if (_lastValueNode != null)
            {
                if (_lastValueNode.Type == XmlValueExpressionType.ElementStartTag)
                    return ProvideElementAutoCompleteItems(out startIndexToReplace, out endIndexToReplace);

                XmlElementExpressionNode parent;
                var attributeExpressionNode = _lastValueNode.Parent as XmlAttributeExpressionNode;
                if (attributeExpressionNode == null)
                    parent = _lastValueNode.Parent as XmlElementExpressionNode;
                else
                    parent = attributeExpressionNode.Parent;

                if (parent != null)
                    return ProvideAttributeAutoCompleteItems(parent, ref startIndexToReplace, ref endIndexToReplace);
            }
            if (_lastElement != null)
            {
                SortedDictionary<string, AutoCompleteItem> items;
                if (_controlsDictionary.TryGetValue(_lastElement.Name, out items))
                    return items.Values;
                return _attachedControlAutoCompleteItems;
            }
            return null;
        }

        void IXmlHandler.HighlightNode(XmlExpressionNode node)
        {
            var commentExpressionNode = node as XmlCommentExpressionNode;
            if (commentExpressionNode != null)
            {
                bindingEditor.Highlight(CommentColor, node);
                return;
            }

            var expressionNode = node as XmlValueExpressionNode;
            if (expressionNode == null)
                return;
            SortedDictionary<string, AutoCompleteItem> list = null;
            XmlAttributeExpressionNode attr;
            switch (expressionNode.Type)
            {
                case XmlValueExpressionType.ElementStartTag:
                case XmlValueExpressionType.ElementStartTagEnd:
                case XmlValueExpressionType.ElementEndTag:
                    var element = node.Parent as XmlElementExpressionNode;
                    if (element == null)
                        return;
                    var elementColor = (element.Parent == null || _controlsDictionary.TryGetValue(element.Name, out list))
                        ? KnownControlColor
                        : UnknownControlColor;
                    var fontStyle = list == null ? _boldFont : null;
                    bindingEditor.Highlight(elementColor, node, fontStyle);
                    break;
                case XmlValueExpressionType.AttributeName:
                    element = node.Parent as XmlElementExpressionNode;
                    if (element == null)
                    {
                        attr = node.Parent as XmlAttributeExpressionNode;
                        if (attr != null)
                            element = attr.Parent;
                    }
                    if (element == null)
                        return;
                    _controlsDictionary.TryGetValue(element.Name, out list);
                    if (list == null)
                    {
                        bindingEditor.Highlight(AttachedMemberColor, node, _boldFont);
                        return;
                    }
                    var memberName = node.GetValue(bindingEditor.Text);
                    var members = memberName.Split(DotSeparator, StringSplitOptions.None);

                    AutoCompleteItem member;
                    if (members.Length == 0 || !list.TryGetValue(members[0], out member))
                    {
                        bindingEditor.Highlight(AttachedMemberColor, node, _boldFont);
                        return;
                    }

                    HighlightMember(member.MemberType, node.Start, members[0].Length);
                    //Highlight for complex path.
                    if (members.Length > 1)
                    {
                        Type type = member.Type;
                        int startIndex = node.Start + members[0].Length + 1;
                        for (int i = 1; i < members.Length; i++)
                        {
                            var path = members[i];
                            var memberType = MemberTypes.Custom;
                            if (type != null)
                            {
                                var bindingMember = BindingServiceProvider
                                    .MemberProvider
                                    .GetBindingMember(type, path, false, false);

                                if (bindingMember == null)
                                    type = null;
                                else
                                {
                                    type = bindingMember.Type;
                                    if (bindingMember.Member != null)
                                        memberType = bindingMember.Member.MemberType;
                                }
                            }
                            HighlightMember(memberType, startIndex, path.Length);
                            startIndex += path.Length + 1;
                        }
                    }
                    break;
                case XmlValueExpressionType.AttributeEqual:
                case XmlValueExpressionType.AttributeValue:
                    attr = node.Parent as XmlAttributeExpressionNode;
                    if (attr != null)
                        bindingEditor.Highlight(ValueColor, node);
                    break;
            }
        }

        #endregion
    }
}