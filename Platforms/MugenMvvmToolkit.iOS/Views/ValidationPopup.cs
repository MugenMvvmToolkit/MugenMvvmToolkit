#region Copyright

// ****************************************************************************
// <copyright file="ValidationPopup.cs">
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
using CoreGraphics;
using Foundation;
using UIKit;

#if XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.iOS.Views
#else
namespace MugenMvvmToolkit.iOS.Views
#endif
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

        protected ValidationPopup(IntPtr handle)
            : base(handle)
        {
        }

        public ValidationPopup(CGRect showOnRect, CGRect fieldFrame)
        {
            ShowOnRect = showOnRect;
            FieldFrame = fieldFrame;
            FontSize = 15;
            PaddingInErrorPopUp = 5;
            Layer.ZPosition = float.MaxValue;
        }

        #endregion

        #region Properties

        public CGRect ShowOnRect { get; private set; }

        public CGRect FieldFrame { get; private set; }

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

        public override void Draw(CGRect rect)
        {
            nfloat[] color = Color.CGColor.Components;
            UIGraphics.BeginImageContext(new CGSize(30, 20));
            CGContext ctx = UIGraphics.GetCurrentContext();
            ctx.SetFillColor(color[0], color[1], color[2], 1);
            ctx.SetShadow(CGSize.Empty, 7f, UIColor.Black.CGColor);

            ctx.AddLines(new[]
            {
                new CGPoint(15, 5), 
                new CGPoint(25, 25),
                new CGPoint(5, 25)
            });
            ctx.ClosePath();
            ctx.FillPath();

            UIImage viewImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            var imgframe = new CGRect((ShowOnRect.X + ((ShowOnRect.Width - 30) / 2)),
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
            var size = message.GetBoundingRect(new CGSize(FieldFrame.Width - (PaddingInErrorPopUp) * 2, 1000),
                NSStringDrawingOptions.UsesLineFragmentOrigin, null).Size;
            size = new CGSize((nfloat)Math.Ceiling(size.Width), (nfloat)Math.Ceiling(size.Height));

            var view = new UIView(CGRect.Empty);
            InsertSubviewBelow(view, img);
            view.BackgroundColor = Color;
            view.Layer.CornerRadius = 5f;
            view.Layer.ShadowColor = UIColor.Black.CGColor;
            view.Layer.ShadowRadius = 5f;
            view.Layer.Opacity = 1f;
            view.Layer.ShadowOffset = CGSize.Empty;
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

            var lbl = new UILabel(CGRect.Empty)
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

        public override bool PointInside(CGPoint point, UIEvent uievent)
        {
            RemoveFromSuperview();
            return false;
        }

        #endregion
    }
}