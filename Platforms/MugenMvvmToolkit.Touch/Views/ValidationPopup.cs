using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MugenMvvmToolkit.Views
{
    /// <summary>
    ///     Source code ObjectiveC https://github.com/dhawaldawar/TextFieldValidator/blob/master/TextFieldValidator/TextFieldValidator.m.
    /// </summary>
    public class ValidationPopup : UIView
    {
        #region Fields

        internal static readonly UIColor ValidationColor;
        private static readonly NSString EmptyNSString;

        private NSString _message;
        private UIColor _color;
        private UIColor _fontColor;
        private string _fontName;

        #endregion

        #region Constructors

        static ValidationPopup()
        {
            ValidationColor = new UIColor(0.7f, 0, 0, 1);
            EmptyNSString = new NSString(string.Empty);
        }

        public ValidationPopup(RectangleF showOnRect, RectangleF fieldFrame)
        {
            ShowOnRect = showOnRect;
            FieldFrame = fieldFrame;
            FontSize = 15;
            PaddingInErrorPopUp = 5;
        }

        #endregion

        #region Properties

        public RectangleF ShowOnRect { get; private set; }

        public RectangleF FieldFrame { get; private set; }

        public NSString Message
        {
            get { return _message ?? EmptyNSString; }
            set { _message = value; }
        }

        public UIColor Color
        {
            get { return _color ?? ValidationColor; }
            set { _color = value; }
        }

        public UIColor FontColor
        {
            get { return _fontColor ?? UIColor.White; }
            set { _fontColor = value; }
        }

        public string FontName
        {
            get { return _fontName ?? "Helvetica-Bold"; }
            set { _fontName = value; }
        }

        public float FontSize { get; set; }

        public float PaddingInErrorPopUp { get; set; }

        #endregion

        #region Overrides of UIView

        public override void Draw(RectangleF rect)
        {
            float[] color = Color.CGColor.Components;
            UIGraphics.BeginImageContext(new SizeF(30, 20));
            CGContext ctx = UIGraphics.GetCurrentContext();
            ctx.SetRGBFillColor(color[0], color[1], color[2], 1);
            ctx.SetShadowWithColor(SizeF.Empty, 7f, UIColor.Black.CGColor);

            ctx.AddLines(new[]
            {
                new PointF(15, 5),
                new PointF(25, 25),
                new PointF(5, 25)
            });
            ctx.ClosePath();
            ctx.FillPath();

            UIImage viewImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            var imgframe = new RectangleF((ShowOnRect.X + ((ShowOnRect.Width - 30) / 2)),
                ((ShowOnRect.Height / 2) + ShowOnRect.Y), 30, 13);

            var img = new UIImageView(viewImage);
            AddSubview(img);
            img.TranslatesAutoresizingMaskIntoConstraints = false;
            var dict = new NSDictionary("img", img);
            img.Superview.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(string.Format(@"H:|-{0}-[img({1})]", imgframe.X, imgframe.Width),
                    NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
            img.Superview.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(string.Format(@"V:|-{0}-[img({1})]", imgframe.Y, imgframe.Height),
                    NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));


            UIFont font = UIFont.FromName(FontName, FontSize);
            var message = new NSAttributedString(Message, font);
            SizeF size = message.GetBoundingRect(new SizeF(FieldFrame.Width - (PaddingInErrorPopUp) * 2, 1000),
                NSStringDrawingOptions.UsesLineFragmentOrigin, null).Size;
            size = new SizeF((float)Math.Ceiling(size.Width), (float)Math.Ceiling(size.Height));

            var view = new UIView(RectangleF.Empty);
            InsertSubviewBelow(view, img);
            view.BackgroundColor = Color;
            view.Layer.CornerRadius = 5f;
            view.Layer.ShadowColor = UIColor.Black.CGColor;
            view.Layer.ShadowRadius = 5f;
            view.Layer.Opacity = 1f;
            view.Layer.ShadowOffset = SizeF.Empty;
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            dict = new NSDictionary("view", view);
            view.Superview.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(
                    string.Format(@"H:|-{0}-[view({1})]",
                        FieldFrame.X + (FieldFrame.Width - (size.Width + (PaddingInErrorPopUp * 2))),
                        size.Width + (PaddingInErrorPopUp * 2)),
                    NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
            view.Superview.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(
                    string.Format(@"V:|-{0}-[view({1})]", imgframe.Y + imgframe.Height,
                        size.Height + (PaddingInErrorPopUp * 2)),
                    NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));

            var lbl = new UILabel(RectangleF.Empty)
            {
                Font = font,
                Lines = 0,
                BackgroundColor = UIColor.Clear,
                Text = Message,
                TextColor = FontColor
            };
            view.AddSubview(lbl);

            lbl.TranslatesAutoresizingMaskIntoConstraints = false;
            dict = new NSDictionary("lbl", lbl);
            lbl.Superview.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(
                    string.Format(@"H:|-{0}-[lbl({1})]", PaddingInErrorPopUp, size.Width),
                    NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
            lbl.Superview.AddConstraints(
                NSLayoutConstraint.FromVisualFormat(
                    string.Format(@"V:|-{0}-[lbl({1})]", PaddingInErrorPopUp, size.Height),
                    NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
        }

        public override bool PointInside(PointF point, UIEvent uievent)
        {
            RemoveFromSuperview();
            return false;
        }

        #endregion
    }
}