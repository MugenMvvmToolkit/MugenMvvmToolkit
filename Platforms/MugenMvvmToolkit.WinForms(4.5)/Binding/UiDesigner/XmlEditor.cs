#region Copyright

// ****************************************************************************
// <copyright file="XmlEditor.cs">
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using MugenMvvmToolkit.WinForms.Binding.Interfaces;
using MugenMvvmToolkit.WinForms.Binding.Models;
using MugenMvvmToolkit.WinForms.Binding.Parse;
using MugenMvvmToolkit.WinForms.Binding.Parse.Nodes;
using Timer = System.Threading.Timer;

namespace MugenMvvmToolkit.WinForms.Binding.UiDesigner
{
    internal partial class XmlEditor : RichTextBox
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct UndoRedoInfo
        {
            #region Fields

            public readonly int CursorLocation;
            public readonly int Length;
            public readonly Point ScrollPos;
            public readonly string Text;

            #endregion

            #region Constructors

            private UndoRedoInfo(string text, Point scrollPos, int cursorLoc, int length)
            {
                Text = text;
                ScrollPos = scrollPos;
                CursorLocation = cursorLoc;
                Length = length;
            }

            #endregion

            #region Methods

            public static UndoRedoInfo Create(XmlEditor editor)
            {
                return new UndoRedoInfo(editor.Rtf, editor.GetScrollPos(), editor.SelectionStart, editor.SelectionLength);
            }

            public void Restore(XmlEditor editor, bool restoreText, bool restoreScroll = true)
            {
                if (restoreText)
                {
                    editor._highlighting = true;
                    editor.Rtf = Text;
                    editor._highlighting = false;
                }
                editor.Select(CursorLocation, Length);
                if (restoreScroll)
                    editor.SetScrollPos(ScrollPos);
                if (restoreText)
                    editor.OnTextChanged(EventArgs.Empty);
            }

            #endregion
        }

        #endregion

        #region Fields

        // ReSharper disable InconsistentNaming
        private const int WM_KEYDOWN = 0x100;
        private const int WM_SETREDRAW = 0x0b;
        private const int WM_CHAR = 0x102;
        private const int VK_CONTROL = 0x11;
        private const short KS_KEYDOWN = 0x80;

        const int WM_USER = 0x400;
        const int EM_GETEVENTMASK = WM_USER + 59;
        const int EM_SETEVENTMASK = WM_USER + 69;
        const int EM_GETSCROLLPOS = WM_USER + 221;
        const int EM_SETSCROLLPOS = WM_USER + 222;
        private const int MaxUndoRedoSteps = 100;
        // ReSharper restore InconsistentNaming

        private readonly Stack<UndoRedoInfo> _redoStack;
        private readonly List<UndoRedoInfo> _undoList;
        private readonly XmlParser _parser;
        private readonly XmlVisitor _visitor;
        private readonly ListBox _autoCompleteForm;
        private readonly Timer _highlightTimer;
        private readonly Action _highlightDelegate;

        private bool _autoCompleteShown;
        private bool _ignoreLostFocus;
        private bool _isUndo;
        private bool _highlighting;
        private bool _paintSuspended;

        private IntPtr _eventMask;
        private int _startIndexToReplace;
        private int _endIndexToReplace;
        private UndoRedoInfo _lastInfo;
        private UndoRedoInfo _painSuspendedInfo;

        #endregion

        #region Constructors

        public XmlEditor()
        {
            InitializeComponent();
            _redoStack = new Stack<UndoRedoInfo>();
            _undoList = new List<UndoRedoInfo>();
            _parser = new XmlParser();
            _visitor = new XmlVisitor();
            _visitor.VisitNode += VisitorOnVisitNode;
            _autoCompleteForm = new ListBox
            {
                Visible = false,
                MaximumSize = new Size(0, 200),
                AutoSize = true,
                MinimumSize = new Size(350, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                HorizontalScrollbar = true,
            };
            _highlightDelegate = Highlight;
            _highlightTimer = new Timer(HighlightTimerCallback);
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

        public new string Text
        {
            get { return base.Text; }
            set
            {
                if (Equals(value, base.Text))
                    return;
                SuspendPainting();
                base.Text = value;
                ResumePainting();
            }
        }

        internal IXmlHandler Handler { get; set; }

        private int Delay
        {
            get { return Math.Min(Lines.Length * 4, 600); }
        }

        #endregion

        #region Methods

        public string GetBindingText()
        {
            return Text;
        }

        public void SetBindingText(string text)
        {
            Text = text;
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
            _redoStack.Push(UndoRedoInfo.Create(this));
            UndoRedoInfo info = _undoList[0];
            _undoList.RemoveAt(0);
            info.Restore(this, true);
            _lastInfo = info;
            _isUndo = false;
        }

        public new void Redo()
        {
            if (!CanRedo)
                return;
            _isUndo = true;
            _undoList.Insert(0, UndoRedoInfo.Create(this));
            UpdateUndoLimit();
            UndoRedoInfo info = _redoStack.Pop();
            info.Restore(this, true);
            _isUndo = false;
        }

        public void Format()
        {
            if (_visitor.IsInvlalid)
                return;
            try
            {
                var text = XElement.Parse(GetBindingText(), LoadOptions.None).ToString();
                SetBindingText(text);
                Highlight();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
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

        internal void Highlight(Color color, int start, int length, Font font = null)
        {
            Select(start, length);
            SelectionColor = color;
            if (font != null)
                SelectionFont = font;
            SelectionLength = 0;
            SelectionColor = ForeColor;
            SelectionFont = Font;
        }

        internal void Highlight(Color color, XmlExpressionNode node, Font font = null)
        {
            Highlight(color, node.Start, node.Length, font);
        }

        private void UpdateUndoLimit()
        {
            while (_undoList.Count > MaxUndoRedoSteps)
                _undoList.RemoveAt(MaxUndoRedoSteps);
        }

        private Point GetScrollPos()
        {
            var p = new Point();
            SendMessage(Handle, EM_GETSCROLLPOS, 0, ref p);
            return p;
        }

        private void SetScrollPos(Point pos)
        {
            SendMessage(Handle, EM_SETSCROLLPOS, 0, ref pos);
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

        private void SuspendPainting()
        {
            if (_paintSuspended)
                return;

            _painSuspendedInfo = UndoRedoInfo.Create(this);
            SendMessage(Handle, WM_SETREDRAW, 0, IntPtr.Zero);
            _eventMask = SendMessage(Handle, EM_GETEVENTMASK, 0, IntPtr.Zero);
            _paintSuspended = true;
        }

        private void ResumePainting()
        {
            if (!_paintSuspended)
                return;

            _painSuspendedInfo.Restore(this, false, false);
            SendMessage(Handle, EM_SETEVENTMASK, 0, _eventMask);
            SetScrollPos(_painSuspendedInfo.ScrollPos);
            SendMessage(Handle, WM_SETREDRAW, 1, IntPtr.Zero);
            _paintSuspended = false;
            Invalidate();
            if (_autoCompleteShown)
                _autoCompleteForm.Invalidate();
        }

        private void HighlightTimerCallback(object state)
        {
            Invoke(_highlightDelegate);
        }

        private void Highlight()
        {
            _highlightTimer.Change(int.MaxValue, int.MaxValue);
            _highlighting = true;
            SuspendPainting();
            SelectAll();
            SelectionColor = ForeColor;
            SelectionFont = Font;
            _visitor.Raise();
            ResumePainting();
            _highlighting = false;
        }

        #endregion

        #region Overrides of TextBoxBase

        protected override void OnTextChanged(EventArgs e)
        {
            if (_highlighting)
                return;
            _highlightTimer.Change(Delay, int.MaxValue);
            HideAutoCompleteForm();
            _visitor.Clear();
            if (!string.IsNullOrEmpty(Text))
            {
                var nodes = _parser.Parse(Text);
                _visitor.Visit(nodes);
            }
            base.OnTextChanged(e);
            if (!_isUndo)
            {
                _redoStack.Clear();
                _undoList.Insert(0, _lastInfo);
                UpdateUndoLimit();
                _lastInfo = UndoRedoInfo.Create(this);
            }
            if (CanAutoComplete(true))
                ShowAutoComplete();
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            if (!_highlighting)
                HideAutoCompleteForm();
            base.OnSelectionChanged(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
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
                        SelectionLength = 0;
                        SelectedText = "\"\"";
                        SelectionStart = prevValue + 2;
                        e.Handled = true;
                    }
                }
            }
            else if (e.KeyChar == '/')
            {
                var node = GetSelectedNode();
                var selectedNode = node as XmlInvalidExpressionNode;
                if (selectedNode != null && selectedNode.Type == XmlInvalidExpressionType.Element)
                {
                    var nextChar = SelectionStart;
                    if (Text.Length <= nextChar || Text[nextChar] != '>')
                    {
                        var newIndex = SelectionStart + 3;
                        SelectionLength = 0;
                        SelectedText = " />";
                        SelectionStart = newIndex;
                        e.Handled = true;
                    }
                }
            }
            else if (e.KeyChar == '>')
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
                            SelectionLength = 0;
                            SelectedText = string.Format("></{0}>", element.Name);
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
        private static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);

        [DllImport("user32")]
        private static extern short GetKeyState(int nVirtKey);

        [DllImport("user32")]
        private extern static int GetCaretPos(out Point p);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

        #endregion
    }
}