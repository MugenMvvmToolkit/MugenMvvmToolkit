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

        private static readonly Color KnownControlColor = ColorTranslator.FromHtml("#00008A");
        private static readonly Color UnknownControlColor = Color.Red;
        private static readonly Color CommentColor = ColorTranslator.FromHtml("#FF007E27");

        private static readonly Color ValueColor = ColorTranslator.FromHtml("#0000EA");
        private static readonly Color PropertyColor = ColorTranslator.FromHtml("#9E277E");
        private static readonly Color EventColor = ColorTranslator.FromHtml("#FE00FE");
        private static readonly Color AttachedMemberColor = ColorTranslator.FromHtml("#00889B");

        private const string ControlsName = "Controls";
        private const string ComponentsName = "Components";
        private readonly SortedDictionary<string, AutoCompleteItem> _controlsCompleteItems;
        private readonly SortedDictionary<string, SortedDictionary<string, AutoCompleteItem>> _controlsDictionary;
        private readonly AutoCompleteItem[] _attachedControlAutoCompleteItems;

        private readonly TreeNode _allControlsNode;
        private readonly TreeNode _sourceControls;
        private readonly Font _boldFont;
        private readonly Font _italicFont;

        private XmlElementExpressionNode _lastElement;
        private XmlValueExpressionNode _lastValueNode;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingEditorView" /> class.
        /// </summary>
        public BindingEditorView()
        {
            InitializeComponent();
            bindingEditor.Handler = this;
            _controlsDictionary = new SortedDictionary<string, SortedDictionary<string, AutoCompleteItem>>(StringComparer.CurrentCulture);
            _controlsCompleteItems = new SortedDictionary<string, AutoCompleteItem>(StringComparer.CurrentCulture);
            _allControlsNode = GetControls(CurrentControl);
            _sourceControls = GetComponents(CurrentControl);
            _attachedControlAutoCompleteItems = BindingServiceProvider
                .MemberProvider
                .GetAttachedMemberNames(typeof(Control))
                .ToArrayFast(s => new AutoCompleteItem(s, s, MemberTypes.Custom));
            typeControlComboBox.SelectedIndexChanged += TypeChanged;
            typeControlComboBox.Items.Add(ControlsName);
            typeControlComboBox.Items.Add(ComponentsName);
            typeControlComboBox.SelectedItem = ComponentsName;
            _boldFont = new Font(bindingEditor.Font, FontStyle.Bold);
            _italicFont = new Font(bindingEditor.Font, FontStyle.Italic);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingEditorView" /> class.
        /// </summary>
        public BindingEditorView(string xmlText)
            : this()
        {
            bindingEditor.Text = xmlText;
            bindingEditor.UpdateText();
        }

        #endregion

        #region Properties

        public static Control CurrentControl { get; set; }

        public string BindingText
        {
            get { return bindingEditor.Text; }
        }

        #endregion

        #region Methods

        private void TypeChanged(object sender, EventArgs eventArgs)
        {
            controlsTreeView.Nodes.Clear();
            var selectedItem = (string)typeControlComboBox.SelectedItem;
            switch (selectedItem)
            {
                case ControlsName:
                    controlsTreeView.Nodes.Add(_allControlsNode);
                    break;
                case ComponentsName:
                    controlsTreeView.Nodes.Add(_sourceControls);
                    break;
            }
            _sourceControls.ExpandAll();
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            DialogResult = ((Button)sender).DialogResult;
            Close();
        }

        private TreeNode GetControls(Control container)
        {
            if (container == null)
                return new TreeNode("Empty");
            var treeNodes = new List<TreeNode>();
            foreach (var control in container.Controls.OfType<Control>())
            {
                string name;
                Type type;
                if (AddCompleteItem(control, out name, out type))
                    treeNodes.Add(GetControls(control));
            }
            var node = new TreeNode(GetDisplayName(container, container.Name, container.GetType()), treeNodes.ToArray());
            node.Expand();
            return node;
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
                    treeNodes.Add(new TreeNode(GetDisplayName(component, name, type)));
            }
            return new TreeNode(GetDisplayName(container, container.Name, container.GetType()), treeNodes.ToArrayFast());
        }

        private bool AddCompleteItem(IComponent result, out string name, out Type type)
        {
            type = null;
            name = null;
            try
            {
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
                foreach (var member in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    completeItems.Add(new AutoCompleteItem(member));
                foreach (var member in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(info => info.CanWrite && info.GetIndexParameters().Length == 0))
                    completeItems.Add(new AutoCompleteItem(member));
                foreach (var member in type.GetEvents(BindingFlags.Public | BindingFlags.Instance))
                    completeItems.Add(new AutoCompleteItem(member));

                completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.DataContext, AttachedMemberConstants.DataContext, MemberTypes.Custom));
                completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.CommandParameter, AttachedMemberConstants.CommandParameter, MemberTypes.Custom));
                if (typeof(DataGridView).IsAssignableFrom(type) || typeof(TabControl).IsAssignableFrom(type))
                {
                    completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.SelectedItem, AttachedMemberConstants.SelectedItem, MemberTypes.Custom));
                }
                if (typeof(Control).IsAssignableFrom(type))
                {
                    completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.Content,
                        AttachedMemberConstants.Content, MemberTypes.Custom));
                    completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.ContentTemplate,
                        AttachedMemberConstants.ContentTemplate, MemberTypes.Custom));
                    completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.ItemsSource,
                        AttachedMemberConstants.ItemsSource, MemberTypes.Custom));
                    completeItems.Add(new AutoCompleteItem(AttachedMemberConstants.ItemTemplate,
                        AttachedMemberConstants.ItemTemplate, MemberTypes.Custom));
                    completeItems.Add(new AutoCompleteItem("CollectionViewManager", "CollectionViewManager",
                        MemberTypes.Custom));
                    completeItems.Add(new AutoCompleteItem("ContentViewManager", "ContentViewManager",
                        MemberTypes.Custom));
                }
                foreach (var attachedName in BindingServiceProvider.MemberProvider.GetAttachedMemberNames(type))
                {
                    if (!completeItems.ContainsKey(attachedName) && XmlTokenizer.IsValidName(attachedName))
                        completeItems.Add(new AutoCompleteItem(attachedName, attachedName, MemberTypes.Custom));
                }
                _controlsCompleteItems.Add(new AutoCompleteItem(string.Format("{0} ({1})", name, type.Name), name));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static string GetDisplayName(object instance, string name, Type type)
        {
            string text = instance == null ? null : PlatformExtensions.TryGetValue(instance, "Text");
            return string.Format("{0} ({1}{2})", name, type.Name, string.IsNullOrEmpty(text) ? "" : ", " + text);
        }

        private static ICollection<AutoCompleteItem> FindItems(SortedDictionary<string, AutoCompleteItem> items, string value)
        {
            if (value == null)
                return items.Values;
            return items.Values.Where(item => item.Value.SafeContains(value)).ToList();
        }

        #endregion

        #region Implementation of IXmlHandler

        bool IXmlHandler.CanAutoComplete(bool textChanged)
        {
            var node = bindingEditor.GetNodeAt(bindingEditor.SelectionStart - 1);
            _lastValueNode = node as XmlValueExpressionNode;
            if (_lastValueNode != null)
                return _lastValueNode.Type == XmlValueExpressionType.ElementStartTag ||
                       _lastValueNode.Type == XmlValueExpressionType.AttributeName;

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
                {
                    startIndexToReplace = _lastValueNode.Start + 1;
                    endIndexToReplace = _lastValueNode.End;

                    string selectedName = null;
                    var length = _lastValueNode.End - startIndexToReplace;
                    if (length != 0)
                        selectedName = bindingEditor.Text.Substring(startIndexToReplace, length);
                    return FindItems(_controlsCompleteItems, selectedName);
                }

                var node = _lastValueNode.Parent as XmlElementExpressionNode;
                if (node == null)
                    return null;
                SortedDictionary<string, AutoCompleteItem> list;
                if (!_controlsDictionary.TryGetValue(node.Name, out list))
                    return null;
                startIndexToReplace = _lastValueNode.Start;
                endIndexToReplace = _lastValueNode.End;
                return FindItems(list, _lastValueNode.GetValue(bindingEditor.Text));
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
                bindingEditor.Highlight(CommentColor, node, _italicFont);
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
                    var memberName = node.GetValue(bindingEditor.Text);
                    _controlsDictionary.TryGetValue(element.Name, out list);
                    AutoCompleteItem member = null;
                    if (list != null)
                        list.TryGetValue(memberName, out member);
                    if (member == null)
                    {
                        bindingEditor.Highlight(AttachedMemberColor, node, _boldFont);
                        return;
                    }
                    switch (member.Type)
                    {
                        case MemberTypes.Event:
                            bindingEditor.Highlight(EventColor, node);
                            break;
                        case MemberTypes.Field:
                        case MemberTypes.Property:
                            bindingEditor.Highlight(PropertyColor, node);
                            break;
                        case MemberTypes.Custom:
                            bindingEditor.Highlight(AttachedMemberColor, node);
                            break;
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