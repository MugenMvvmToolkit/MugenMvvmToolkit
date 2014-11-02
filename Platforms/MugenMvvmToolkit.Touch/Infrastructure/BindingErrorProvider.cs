#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProvider.cs">
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
using System.Reflection;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated
    ///     with it.
    /// </summary>
    public class BindingErrorProvider : BindingErrorProviderBase
    {
        #region Nested types

        private sealed class LayoutInfo
        {
            #region Fields

            private const string Key = "@#layoutState";

            public float BorderWidth { get; set; }

            public float CornerRadius { get; set; }

            public CGColor BorderColor { get; set; }

            public bool ClipsToBounds { get; set; }

            #endregion

            #region Constructors

            private LayoutInfo(UIView view)
            {
                BorderWidth = view.Layer.BorderWidth;
                CornerRadius = view.Layer.CornerRadius;
                BorderColor = view.Layer.BorderColor;
                ClipsToBounds = view.ClipsToBounds;
            }

            #endregion

            #region Methods

            public static void Save(UIView view)
            {
                ServiceProvider.AttachedValueProvider.GetOrAdd(view, Key, (uiView, o) => new LayoutInfo(uiView), null);
            }

            public static void Restore(UIView view)
            {
                var info = ServiceProvider.AttachedValueProvider.GetValue<LayoutInfo>(view, Key, false);
                if (info == null)
                    return;
                ServiceProvider.AttachedValueProvider.Clear(view, Key);
                view.Layer.BorderColor = info.BorderColor;
                view.Layer.BorderWidth = info.BorderWidth;
                view.Layer.CornerRadius = info.CornerRadius;
                view.ClipsToBounds = info.ClipsToBounds;
            }

            #endregion
        }

        private sealed class ErrorButton : UIButton, IOrientationChangeListener
        {
            #region Fields

            private ValidationPopup _popup;
            private readonly BindingErrorProvider _errorProvider;
            private readonly UITextField _textField;
            private NSString _message;

            #endregion

            #region Constructors

            public ErrorButton(BindingErrorProvider errorProvider, UITextField textField)
                : base(new RectangleF(0, 0, 25, 25))
            {
                _errorProvider = errorProvider;
                _textField = textField;
                TouchUpInside += OnTouchUpInside;
                PlatformExtensions.AddOrientationChangeListener(this);
            }

            #endregion

            #region Methods

            public void SetErrors(IList<object> errors)
            {
                _message = errors.Count == 0 ? null : new NSString(string.Join(Environment.NewLine, errors));
                HidePopup();
            }

            private void OnTouchUpInside(object sender, EventArgs eventArgs)
            {
                var superview = GetTextFieldSuperview();
                if (_popup == null)
                {
                    var showOnRect = _textField.ConvertRectToView(Frame, superview);
                    var fieldFrame = superview.ConvertRectToView(_textField.Frame, superview);
                    _popup = _errorProvider.CreateValidationPopup(showOnRect, fieldFrame);
                    _popup.TranslatesAutoresizingMaskIntoConstraints = false;
                    _popup.Message = _message;

                    _message = null;
                }
                if (_popup.Superview == null)
                {
                    superview.AddSubview(_popup);

                    var dict = new NSDictionary("popup", _popup);
                    _popup.Superview.AddConstraints(NSLayoutConstraint.FromVisualFormat(@"H:|-0-[popup]-0-|",
                            NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
                    _popup.Superview.AddConstraints(NSLayoutConstraint.FromVisualFormat(@"V:|-0-[popup]-0-|",
                        NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
                }
            }

            private void HidePopup()
            {
                if (_popup == null)
                    return;
                _popup.RemoveFromSuperview();
                _popup.Dispose();
                _popup = null;
            }

            private UIView GetTextFieldSuperview()
            {
                var superview = _textField.Superview;
                while (superview is UIScrollView)
                    superview = superview.Superview;
                return superview;
            }

            #endregion

            #region Implementation of IOrientationChangeListener

            public void OnOrientationChanged()
            {
                if (_popup != null)
                    _message = _popup.Message;
                HidePopup();
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Func<object, UITextField> GetEntryField;
        private static readonly UIImage DefaultErrorImage;

        #endregion

        #region Constructors

        static BindingErrorProvider()
        {
            var field = typeof(EntryElement).GetField("entry", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(UITextField))
                GetEntryField = ServiceProvider.ReflectionManager.GetMemberGetter<UITextField>(field);
            DefaultErrorImage = UIImage.FromFile("error.png");
        }

        public BindingErrorProvider()
        {
            ErrorBorderColor = ValidationPopup.ValidationColor;
            ErrorBorderWidth = 1f;
            CornerRadius = 5f;
            RightErrorImagePosition = true;
            ErrorImage = DefaultErrorImage;
        }

        #endregion

        #region Properties

        public float ErrorBorderWidth { get; set; }

        public float CornerRadius { get; set; }

        public UIColor ErrorBorderColor { get; set; }

        public UIImage ErrorImage { get; set; }

        public bool RightErrorImagePosition { get; set; }

        #endregion

        #region Methods

        protected virtual ValidationPopup CreateValidationPopup(RectangleF showOnRect, RectangleF fieldFrame)
        {
            return new ValidationPopup(showOnRect, fieldFrame)
            {
                Color = ErrorBorderColor,
                BackgroundColor = UIColor.Clear,
                FontColor = UIColor.White
            };
        }

        private ErrorButton CreateErrorButton(UITextField textField)
        {
            var btn = new ErrorButton(this, textField);
            btn.SetBackgroundImage(ErrorImage, UIControlState.Normal);
            return btn;
        }

        #endregion

        #region Overrides of BindingErrorProviderBase

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        protected override void SetErrors(object target, IList<object> errors, IDataContext context)
        {
            var hasErrors = errors.Count != 0;
            var element = target as EntryElement;
            if (element != null && GetEntryField != null)
                target = GetEntryField(element);

            var uiView = target as UIView;
            if (uiView != null && ErrorBorderWidth > 0)
            {
                if (hasErrors)
                {
                    LayoutInfo.Save(uiView);
                    uiView.Layer.BorderColor = ErrorBorderColor.CGColor;
                    uiView.Layer.BorderWidth = ErrorBorderWidth;
                    uiView.Layer.CornerRadius = CornerRadius;
                    uiView.ClipsToBounds = true;
                }
                else
                    LayoutInfo.Restore(uiView);

            }

            var textField = target as UITextField;
            if (textField != null && ErrorImage != null && textField.Superview != null)
            {
                ErrorButton errorButton;
                UITextFieldViewMode mode = hasErrors ? UITextFieldViewMode.Always : UITextFieldViewMode.Never;
                if (RightErrorImagePosition)
                {
                    textField.RightViewMode = mode;
                    errorButton = textField.RightView as ErrorButton;
                    if (errorButton == null)
                    {
                        errorButton = CreateErrorButton(textField);
                        textField.RightView = errorButton;
                    }
                }
                else
                {
                    textField.LeftViewMode = mode;
                    errorButton = textField.LeftView as ErrorButton;
                    if (errorButton == null)
                    {
                        errorButton = CreateErrorButton(textField);
                        textField.LeftView = errorButton;
                    }
                }
                errorButton.SetErrors(errors);
            }
            base.SetErrors(target, errors, context);
        }

        #endregion
    }
}