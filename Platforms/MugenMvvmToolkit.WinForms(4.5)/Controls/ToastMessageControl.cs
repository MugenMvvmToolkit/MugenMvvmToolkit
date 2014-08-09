#region Copyright
// ****************************************************************************
// <copyright file="ToastMessageControl.cs">
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Controls
{
    public class ToastMessageControl : Control
    {
        #region Fields

        private readonly Color _backgroundColor;
        private readonly Color _foregroundColor;
        private readonly Color? _glowColor;
        private readonly string _message;
        private bool _isTransparent;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastMessageControl" /> class with default settings.
        /// </summary>
        public ToastMessageControl([NotNull] string message, float duration, Color backgroundColor,
            Color foregroundColor, Color? glowColor, [NotNull] TaskCompletionSource<object> tcs)
        {
            Should.NotBeNull(message, "message");
            Should.NotBeNull(tcs, "tcs");
            _message = message;
            _backgroundColor = backgroundColor;
            _foregroundColor = foregroundColor;
            _glowColor = glowColor;
            _isTransparent = false;
            Duration = duration;
            TaskCompletionSource = tcs;
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        #endregion

        #region Properties

        public bool IsTransparent
        {
            get { return _isTransparent; }
            set
            {
                _isTransparent = value;
                if (value)
                    BackColor = Color.Transparent;
                Invalidate();
            }
        }

        public float AlphaValue { get; set; }

        public float Duration { get; set; }

        public TaskCompletionSource<object> TaskCompletionSource { get; private set; }

        #endregion

        #region Overrides of Control

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data. </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Parent == null)
                return;
            try
            {
                if (IsTransparent)
                {
                    int childIndex = Parent.Controls.GetChildIndex(this);
                    int count = Parent.Controls.Count - 1;
                    int num = childIndex + 1;
                    for (int i = count; i >= num; i -= 1)
                    {
                        Control ctrl = Parent.Controls[i];
                        if (!ctrl.Bounds.IntersectsWith(Bounds) || !ctrl.Visible)
                            continue;
                        using (var bmp = new Bitmap(ctrl.Width, ctrl.Height))
                        {
                            ctrl.DrawToBitmap(bmp, ctrl.ClientRectangle);
                            e.Graphics.TranslateTransform(ctrl.Left - Left, ctrl.Top - Top);
                            e.Graphics.DrawImage(bmp, Point.Empty);
                            e.Graphics.TranslateTransform(Left - ctrl.Left, Top - ctrl.Top);
                        }
                    }
                }
                PaintInternal(e);
            }
            catch (Exception exception)
            {
                Tracer.Error(exception.Flatten());
            }
        }

        #endregion

        #region Methods

        private void PaintInternal(PaintEventArgs pe)
        {
            using (var img = new Bitmap(Width, Height))
            using (Graphics e = Graphics.FromImage(img))
            using (var gp = new GraphicsPath())
            {
                e.SmoothingMode = SmoothingMode.AntiAlias;
                if (_glowColor.HasValue)
                {
                    using (Brush bru = new SolidBrush(Color.FromArgb(50, _glowColor.Value)))
                    using (var pn = new Pen(bru, 6f))
                    {
                        pn.LineJoin = LineJoin.Round;

                        var rectangle = new Rectangle(3, 3, Width - 10, Height - 10);
                        gp.AddRectangle(rectangle);
                        e.DrawPath(pn, gp);
                        gp.Reset();

                        rectangle = new Rectangle(5, 5, Width - 14, Height - 14);
                        gp.AddRectangle(rectangle);
                        e.DrawPath(pn, gp);
                        gp.Reset();

                        rectangle = new Rectangle(7, 7, Width - 18, Height - 18);
                        gp.AddRectangle(rectangle);
                        e.DrawPath(pn, gp);
                        gp.Reset();

                        rectangle = new Rectangle(9, 9, Width - 22, Height - 22);
                        gp.AddRectangle(rectangle);
                        e.DrawPath(pn, gp);
                        gp.Reset();
                    }
                }

                using (var backBrush = new SolidBrush(_backgroundColor))
                using (var blackPn = new Pen(backBrush, 5f) { LineJoin = LineJoin.Round })
                {
                    var rectangle = new Rectangle(8, 8, Width - 20, Height - 20);
                    gp.AddRectangle(rectangle);
                    e.DrawPath(blackPn, gp);

                    rectangle = new Rectangle(9, 9, Width - 21, Height - 21);
                    e.FillRectangle(backBrush, rectangle);
                    var cma = new ColorMatrix { Matrix33 = AlphaValue };
                    var imga = new ImageAttributes();
                    imga.SetColorMatrix(cma);
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter
                    };

                    using (var foregroundBrush = new SolidBrush(_foregroundColor))
                    {
                        rectangle = new Rectangle(9, 9, Width - 21, Height - 21);
                        e.DrawString(_message, Parent.Font, foregroundBrush, rectangle, sf);
                        rectangle = new Rectangle(0, 0, Width, Height);
                        pe.Graphics.DrawImage(img, rectangle, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imga);
                    }
                }
            }
        }

        #endregion
    }
}