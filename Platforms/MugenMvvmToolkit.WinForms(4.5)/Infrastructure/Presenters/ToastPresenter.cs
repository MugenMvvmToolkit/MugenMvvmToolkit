#region Copyright

// ****************************************************************************
// <copyright file="ToastPresenter.cs">
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
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.WinForms.Binding;
using MugenMvvmToolkit.WinForms.Controls;

namespace MugenMvvmToolkit.WinForms.Infrastructure.Presenters
{
    public class ToastPresenter : IToastPresenter
    {
        #region Nested types

        private sealed class ToastImpl : IToast
        {
            #region Fields

            public readonly TaskCompletionSource<object> Tcs;
            public ToastMessageControl Control;

            #endregion

            #region Constructors

            public ToastImpl()
            {
                Tcs = new TaskCompletionSource<object>();
            }

            #endregion

            #region Properties

            public Task CompletionTask
            {
                get { return Tcs.Task; }
            }

            #endregion


            #region Methods

            public void Close()
            {
                var control = Control;
                if (control == null)
                    return;
                Control = null;
                ServiceProvider.ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, control, control, (messageControl, toastMessageControl) => ClearControl(messageControl));
            }

            #endregion
        }

        #endregion

        #region Fields

        private const int TimerInterval = 45;
        private static readonly string ControlName;
        private readonly IThreadManager _threadManager;

        #endregion

        #region Constructors

        static ToastPresenter()
        {
            ControlName = Guid.NewGuid().ToString("n");
        }

        public ToastPresenter([NotNull] IThreadManager threadManager)
        {
            Should.NotBeNull(threadManager, "threadManager");
            _threadManager = threadManager;
            Background = Color.FromArgb(255, 105, 105, 105);
            Foreground = Color.FromArgb(255, 247, 247, 247);
        }

        #endregion

        #region Properties

        public Color? Glow { get; set; }

        public Color Background { get; set; }

        public Color Foreground { get; set; }

        public Font Font { get; set; }

        #endregion

        #region Implementation of IToastPresenter

        public IToast ShowAsync(object content, float duration, ToastPosition position = ToastPosition.Bottom, IDataContext context = null)
        {
            var toastImpl = new ToastImpl();
            if (_threadManager.IsUiThread)
                toastImpl.Control = ShowInternal(content, duration, position, context, toastImpl.Tcs);
            else
                _threadManager.InvokeOnUiThreadAsync(() => toastImpl.Control = ShowInternal(content, duration, position, context, toastImpl.Tcs));
            return toastImpl;
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual ToastMessageControl ShowInternal(object content, float duration, ToastPosition position, IDataContext context, TaskCompletionSource<object> tcs)
        {
            Form activeForm = Form.ActiveForm;
            if (activeForm == null)
            {
                tcs.SetResult(null);
                return null;
            }
            foreach (var result in activeForm.Controls.Find(ControlName, false).OfType<ToastMessageControl>())
                ClearControl(result);

            ToastMessageControl control = null;
            var selector = activeForm.GetBindingMemberValue(AttachedMembers.Form.ToastTemplateSelector);
            if (selector != null)
                control = (ToastMessageControl)selector.SelectTemplate(content, activeForm);

            if (control == null)
                control = GetToastControl(activeForm, content);
            control.Duration = duration;
            control.TaskCompletionSource = tcs;
            control.Name = ControlName;
            activeForm.Controls.Add(control);
            SetPosition(activeForm, control, position);
            control.BringToFront();
            var timer = new Timer { Interval = TimerInterval, Tag = control };
            timer.Tick += TimerTick;
            timer.Start();
            control.Tag = timer;
            return control;
        }

        [NotNull]
        protected virtual ToastMessageControl GetToastControl(Form form, object content)
        {
            string msg = content == null ? "(null)" : content.ToString();
            var control = new ToastMessageControl(msg, Background, Foreground, Glow)
            {
                IsTransparent = true
            };
            Font font = Font;
            if (font == null)
                font = form.Font;
            else
                control.Font = font;
            using (Graphics gr = control.CreateGraphics())
            {
                SizeF textSize = gr.MeasureString(msg, font);
                control.Height = (int)textSize.Height + 25;
                control.Width = (int)textSize.Width + 35;
                if (textSize.Width > form.Width - 100)
                {
                    control.Width = form.Width - 100;
                    var hf = textSize.Width / control.Width;
                    control.Height += (int)(textSize.Height * hf);
                }
                if (control.Height > form.Height)
                    control.Height = form.Height;
            }
            return control;
        }

        private static void SetPosition(Control parent, Control control, ToastPosition position)
        {
            control.Left = (parent.ClientSize.Width - control.Width) / 2;
            switch (position)
            {
                case ToastPosition.Bottom:
                    control.Top = parent.ClientSize.Height - control.Height - 20;
                    control.Anchor = AnchorStyles.Bottom;
                    break;
                case ToastPosition.Center:
                    control.Top = (parent.ClientSize.Height - control.Height) / 2;
                    control.Anchor = AnchorStyles.None;
                    break;
                case ToastPosition.Top:
                    control.Top = 20;
                    control.Anchor = AnchorStyles.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("position");
            }
        }

        private static void TimerTick(object sender, EventArgs args)
        {
            var control = (ToastMessageControl)((Timer)sender).Tag;
            if (control.Duration > 0)
            {
                control.Duration -= TimerInterval;
                if (control.AlphaValue == 1f)
                    return;
                control.AlphaValue += 0.1f;
                if (control.AlphaValue > 1f)
                    control.AlphaValue = 1f;
                control.Refresh();
            }
            else
            {
                if (control.AlphaValue == 0f)
                {
                    ClearControl(control);
                    return;
                }
                control.AlphaValue -= 0.1f;
                if (control.AlphaValue < 0f)
                    control.AlphaValue = 0f;
                control.Refresh();
            }
        }

        private static void ClearControl(ToastMessageControl control)
        {
            control.TaskCompletionSource.TrySetResult(null);
            if (control.Parent != null)
                control.Parent.Controls.Remove(control);
            ((Timer)control.Tag).Dispose();
            control.Dispose();
        }

        #endregion
    }
}
