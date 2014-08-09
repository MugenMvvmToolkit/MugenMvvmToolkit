#region Copyright
// ****************************************************************************
// <copyright file="XmlEditor.cs">
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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Binding.UiDesigner
{
    internal partial class XmlEditor : RichTextBox
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct UndoRedoInfo
        {
            #region Fields

            public readonly int CursorLocation;
            public readonly Point ScrollPos;
            public readonly string Text;

            #endregion

            #region Constructors

            public UndoRedoInfo(string text, Point scrollPos, int cursorLoc)
            {
                Text = text;
                ScrollPos = scrollPos;
                CursorLocation = cursorLoc;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Dictionary<string, string> ReplaceKeywords =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"&lt;", "<"},
                {"&gt;", ">"},
                {"&amp;", "&"},
                {"&quot;", "\""}
            };

        // ReSharper disable InconsistentNaming
        private const int WM_USER = 0x400;
        private const int WM_PAINT = 0xF;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SETREDRAW = 0x0b;
        private const int WM_CHAR = 0x102;
        private const int EM_GETSCROLLPOS = (WM_USER + 221);
        private const int EM_SETSCROLLPOS = (WM_USER + 222);
        private const int VK_CONTROL = 0x11;
        private const short KS_KEYDOWN = 0x80;
        // ReSharper restore InconsistentNaming

        private readonly Stack<UndoRedoInfo> _redoStack = new Stack<UndoRedoInfo>();
        private readonly List<UndoRedoInfo> _undoList = new List<UndoRedoInfo>();
        private readonly XmlParser _parser;
        private readonly XmlVisitor _visitor;

        private bool _autoCompleteShown;
        private bool _ignoreLostFocus;
        private readonly ListBox _autoCompleteForm;

        private bool _isUndo;
        private UndoRedoInfo _lastInfo = new UndoRedoInfo("", new Point(), 0);
        private const int MaxUndoRedoSteps = 100;
        private bool _parsing;
        private int _startIndexToReplace;
        private int _endIndexToReplace;

        #endregion

        #region Constructors

        public XmlEditor()
        {
            InitializeComponent();
            _parser = new XmlParser();
            _visitor = new XmlVisitor();
            _visitor.VisitNode += VisitorOnVisitNode;
            _autoCompleteForm = new ListBox
            {
                Visible = false,
                MaximumSize = new Size(0, 200),
                DisplayMember = "DisplayName",
                AutoSize = true,
                MinimumSize = new Size(250, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                HorizontalScrollbar = true,
            };
            Controls.Add(_autoCompleteForm);
        }

        #endregion

        #region Properties

        public new bool CanUndo
        {
            get { return _undoList.Count > 0; }
        }

        public new bool CanRedo
        {
            get { return _redoStack.Count > 0; }
        }

        internal IXmlHandler Handler { get; set; }

        #endregion

        #region Methods

        public void UpdateText()
        {
            OnTextChanged(EventArgs.Empty);
        }

        public void ShowAutoComplete()
        {
            var items = Handler.ProvideAutoCompleteInfo(out _startIndexToReplace, out _endIndexToReplace);
            if (items == null || items.Count == 0)
                return;
            _autoCompleteForm.Items.Clear();
            foreach (AutoCompleteItem item in items)
                _autoCompleteForm.Items.Add(item);
            SetBestSelectedAutoCompleteItem();
            InitializeAutoCompleteLayout(true);
            _ignoreLostFocus = true;
            _autoCompleteForm.Show();
            _autoCompleteShown = true;
            Focus();
            _ignoreLostFocus = false;
        }

        public new void Undo()
        {
            if (!CanUndo)
                return;
            _isUndo = true;
            _redoStack.Push(new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
            UndoRedoInfo info = _undoList[0];
            _undoList.RemoveAt(0);
            Text = info.Text;
            SelectionStart = info.CursorLocation;
            SetScrollPos(info.ScrollPos);
            _lastInfo = info;
            _isUndo = false;
        }

        public new void Redo()
        {
            if (!CanRedo)
                return;
            _isUndo = true;
            _undoList.Insert(0, new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
            UpdateUndoLimit();
            UndoRedoInfo info = _redoStack.Pop();
            Text = info.Text;
            SelectionStart = info.CursorLocation;
            SetScrollPos(info.ScrollPos);
            _isUndo = false;
        }

        public void Format()
        {
            if (_visitor.Nodes.Any(node => node is XmlInvalidExpressionNode))
                return;
            try
            {
                var selectionStart = SelectionStart;
                var text = XElement.Parse(Text, LoadOptions.None).ToString();
                foreach (var replaceKeyword in ReplaceKeywords)
                    text = text.Replace(replaceKeyword.Key, replaceKeyword.Value);
                Text = text;
                SelectionStart = selectionStart;
            }
            catch (Exception)
            {
            }
        }

        internal XmlExpressionNode GetSelectedNode()
        {
            var start = SelectionStart;
            return GetNodeAt(start, SelectionLength + start);
        }

        internal XmlExpressionNode GetNodeAt(int start)
        {
            return GetNodeAt(start, start);
        }

        internal XmlExpressionNode GetNodeAt(int start, int end)
        {
            XmlExpressionNode selectedNode = null;
            foreach (var node in _visitor.Nodes)
            {
                if (node.Start > start)
                    continue;
                if (selectedNode == null)
                {
                    if (node.End >= end)
                        selectedNode = node;
                }
                else
                {
                    if (node.End > end)
                    {
                        if (node.Start >= selectedNode.Start || node.End <= selectedNode.End)
                            selectedNode = node;
                    }
                }
            }
            return selectedNode;
        }

        internal void Highlight(Color color, XmlExpressionNode node, FontStyle? style = null)
        {
            Select(node.Start, node.Length);
            SelectionColor = color;
            if (style != null)
                SelectionFont = new Font(Font, style.Value);
        }

        private void UpdateUndoLimit()
        {
            while (_undoList.Count > MaxUndoRedoSteps)
                _undoList.RemoveAt(MaxUndoRedoSteps);
        }

        private Point GetScrollPos()
        {
            var res = new Point();
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(res));
            try
            {
                Marshal.StructureToPtr(res, pnt, true);
                SendMessage(Handle, EM_GETSCROLLPOS, 0, pnt);
                return res;
            }
            finally
            {
                Marshal.FreeHGlobal(pnt);
            }
        }

        private void SetScrollPos(Point point)
        {
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(point));
            try
            {
                Marshal.StructureToPtr(point, pnt, true);
                SendMessage(Handle, EM_SETSCROLLPOS, 0, pnt);
            }
            finally
            {
                Marshal.FreeHGlobal(pnt);
            }
        }

        private void VisitorOnVisitNode(XmlExpressionNode node)
        {
            if (Handler != null)
                Handler.HighlightNode(node);
        }

        private void AcceptAutoCompleteItem()
        {
            if (_autoCompleteForm.SelectedItem == null)
                return;
            SetAutoCompleteItem(((AutoCompleteItem)_autoCompleteForm.SelectedItem).Value);
            HideAutoCompleteForm();
        }

        private void InitializeAutoCompleteLayout(bool moveHorizontly)
        {
            _autoCompleteForm.Height = Math.Min(
                Math.Max(_autoCompleteForm.Items.Count, 1) * _autoCompleteForm.ItemHeight + 4,
                _autoCompleteForm.MaximumSize.Height);

            Point cursorLocation;
            GetCaretPos(out cursorLocation);
            var optimalLocation = new Point(cursorLocation.X + Left, cursorLocation.Y + Top + 20);
            var desiredPlace = new Rectangle(optimalLocation, _autoCompleteForm.Size) { Width = 152 };
            if (desiredPlace.Left < Bounds.Left)
                desiredPlace.X = Bounds.Left;
            if (desiredPlace.Right > Bounds.Right)
                desiredPlace.X -= (desiredPlace.Right - Bounds.Right);
            if (desiredPlace.Bottom > Bounds.Bottom)
                desiredPlace.Y = cursorLocation.Y - 2 - desiredPlace.Height;
            if (!moveHorizontly)
                desiredPlace.X = _autoCompleteForm.Left;
            _autoCompleteForm.Bounds = desiredPlace;
        }

        private void HideAutoCompleteForm()
        {
            if (!_autoCompleteShown)
                return;
            _autoCompleteForm.Hide();
            _autoCompleteShown = false;
        }

        private void SetAutoCompleteItem(string value)
        {
            SelectionStart = Math.Max(_startIndexToReplace, 0);
            SelectionLength = Math.Max(0, _endIndexToReplace - _startIndexToReplace);
            SelectedText = value;
            SelectionStart = SelectionStart + SelectionLength;
            SelectionLength = 0;
        }

        private void SetBestSelectedAutoCompleteItem()
        {
            var length = _endIndexToReplace - _startIndexToReplace;
            if (length > 0)
            {
                string curTokenString = Text.Substring(_startIndexToReplace, length);
                foreach (AutoCompleteItem item in _autoCompleteForm.Items)
                {
                    if (item.DisplayName.StartsWith(curTokenString, StringComparison.CurrentCultureIgnoreCase))
                    {
                        _autoCompleteForm.SelectedItem = item;
                        break;
                    }
                }
            }
            if (_autoCompleteForm.SelectedIndex < 0)
                _autoCompleteForm.SelectedIndex = 0;
        }

        private bool CanAutoComplete(bool textChaned)
        {
            return Handler != null && Handler.CanAutoComplete(textChaned);
        }

        #endregion

        #region Overrides of TextBoxBase

        protected override void OnTextChanged(EventArgs e)
        {
            if (_parsing) return;
            HideAutoCompleteForm();
            _parsing = true;
            try
            {
                SendMessage(Handle, WM_SETREDRAW, 0, IntPtr.Zero);
                base.OnTextChanged(e);

                Point scrollPos = GetScrollPos();
                int cursorLoc = SelectionStart;
                var length = SelectionLength;
                if (!_isUndo)
                {
                    _redoStack.Clear();
                    _undoList.Insert(0, _lastInfo);
                    UpdateUndoLimit();
                    _lastInfo = new UndoRedoInfo(Text, scrollPos, SelectionStart);
                }

                SelectAll();
                SelectionColor = ForeColor;
                SelectionFont = Font;
                if (!string.IsNullOrEmpty(Text))
                {
                    var nodes = _parser.Parse(Text);
                    _visitor.Visit(nodes);
                }

                SelectionStart = cursorLoc;
                SelectionLength = length;
                SetScrollPos(scrollPos);
            }
            finally
            {
                _parsing = false;
                SendMessage(Handle, WM_SETREDRAW, 1, IntPtr.Zero);
                Invalidate();
            }

            if (CanAutoComplete(true))
                ShowAutoComplete();
        }

        protected override void OnVScroll(EventArgs e)
        {
            if (_parsing) return;
            base.OnVScroll(e);
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            HideAutoCompleteForm();
            base.OnSelectionChanged(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_PAINT:
                    if (_parsing)
                        return;
                    break;

                case WM_KEYDOWN:
                    if (_autoCompleteShown)
                    {
                        switch ((Keys)(int)m.WParam)
                        {
                            case Keys.Down:
                                if (_autoCompleteForm.Items.Count != 0)
                                    _autoCompleteForm.SelectedIndex = (_autoCompleteForm.SelectedIndex + 1) %
                                                                      _autoCompleteForm.Items.Count;
                                return;
                            case Keys.Up:
                                if (_autoCompleteForm.Items.Count != 0)
                                {
                                    if (_autoCompleteForm.SelectedIndex < 1)
                                        _autoCompleteForm.SelectedIndex = _autoCompleteForm.Items.Count - 1;
                                    else
                                        _autoCompleteForm.SelectedIndex--;
                                }
                                return;
                            case Keys.Enter:
                            case Keys.Space:
                                AcceptAutoCompleteItem();
                                return;
                            case Keys.Escape:
                                HideAutoCompleteForm();
                                break;
                        }
                    }
                    else
                    {
                        if (((Keys)(int)m.WParam == Keys.Space) &&
                            ((GetKeyState(VK_CONTROL) & KS_KEYDOWN) != 0))
                        {
                            if (CanAutoComplete(false))
                                ShowAutoComplete();
                        }
                        else if (((Keys)(int)m.WParam == Keys.Z) &&
                                 ((GetKeyState(VK_CONTROL) & KS_KEYDOWN) != 0))
                        {
                            Undo();
                            return;
                        }
                        else if (((Keys)(int)m.WParam == Keys.Y) &&
                                 ((GetKeyState(VK_CONTROL) & KS_KEYDOWN) != 0))
                        {
                            Redo();
                            return;
                        }
                        else if (((Keys)(int)m.WParam == Keys.F) &&
                                 ((GetKeyState(VK_CONTROL) & KS_KEYDOWN) != 0))
                        {
                            Format();
                        }
                    }
                    break;
                case WM_CHAR:
                    switch ((Keys)(int)m.WParam)
                    {
                        case Keys.Space:
                            if ((GetKeyState(VK_CONTROL) & KS_KEYDOWN) != 0)
                                return;
                            break;
                        case Keys.Enter:
                            if (_autoCompleteShown)
                                return;
                            break;
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            if (!_ignoreLostFocus)
                HideAutoCompleteForm();
            base.OnLostFocus(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == '\"')
            {
                var selectedNode = GetNodeAt(SelectionStart - 1) as XmlValueExpressionNode;
                if (selectedNode != null && selectedNode.Type == XmlValueExpressionType.AttributeEqual)
                {
                    var prevValue = SelectionStart - 1;
                    if (prevValue >= 0 && Text[prevValue] == '=')
                    {
                        Text = Text.Insert(SelectionStart, "\"\"");
                        SelectionStart = prevValue + 2;
                        e.Handled = true;
                    }
                }
            }

            if (e.KeyChar == '/')
            {
                var node = GetSelectedNode();
                var selectedNode = node as XmlInvalidExpressionNode;
                if (selectedNode != null && selectedNode.Type == XmlInvalidExpressionType.Element)
                {
                    var nextChar = SelectionStart;
                    if (Text.Length <= nextChar || Text[nextChar] != '>')
                    {
                        var newIndex = SelectionStart + 2;
                        Text = Text.Insert(SelectionStart, "/>");
                        SelectionStart = newIndex;
                        e.Handled = true;
                    }
                }
            }

            if (e.KeyChar == '>')
            {
                var node = GetSelectedNode();
                var selectedNode = node as XmlInvalidExpressionNode;
                if (selectedNode != null && selectedNode.Type == XmlInvalidExpressionType.Element)
                {
                    var element = selectedNode.Nodes.OfType<XmlElementExpressionNode>().FirstOrDefault();
                    if (element != null)
                    {
                        var prevChar = SelectionStart - 2;
                        if (Text.Length > prevChar || Text[prevChar] != '/')
                        {
                            var newIndex = SelectionStart + 1;
                            Text = Text.Insert(SelectionStart, string.Format("></{0}>", element.Name));
                            SelectionStart = newIndex;
                            e.Handled = true;
                        }
                    }
                }
            }
            base.OnKeyPress(e);
        }

        #endregion

        #region Win32 methods

        [DllImport("user32")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32")]
        private extern static int GetCaretPos(out Point p);

        #endregion
    }
}