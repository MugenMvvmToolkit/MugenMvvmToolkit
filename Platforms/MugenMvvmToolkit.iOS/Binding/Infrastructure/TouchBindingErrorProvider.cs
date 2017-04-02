#region Copyright

// ****************************************************************************
// <copyright file="TouchBindingErrorProvider.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.iOS.Interfaces;
using MugenMvvmToolkit.iOS.Views;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public class TouchBindingErrorProvider : BindingErrorProviderBase
    {
        #region Nested types

        private sealed class LayoutInfo
        {
            #region Fields

            private const string Key = "@#layoutState";

            private readonly nfloat _borderWidth;
            private readonly nfloat _cornerRadius;
            private readonly CGColor _borderColor;
            private readonly bool _clipsToBounds;
            private readonly bool _isEmpty;

            #endregion

            #region Constructors

            private LayoutInfo(UIView view)
            {
                var layer = view.Layer;
                if (layer == null)
                {
                    _isEmpty = true;
                    return;
                }
                _borderWidth = layer.BorderWidth;
                _cornerRadius = layer.CornerRadius;
                _borderColor = layer.BorderColor;
                _clipsToBounds = view.ClipsToBounds;
            }

            #endregion

            #region Methods

            public static void Save(UIView view, object target)
            {
                ServiceProvider.AttachedValueProvider.GetOrAdd(target, Key, (t, o) => new LayoutInfo((UIView)o), view);
            }

            public static void Restore(UIView view, object target)
            {
                var info = ServiceProvider.AttachedValueProvider.GetValue<LayoutInfo>(target, Key, false);
                ServiceProvider.AttachedValueProvider.Clear(target, Key);
                if (info == null || info._isEmpty)
                    return;
                var layer = view.Layer;
                if (layer == null)
                    return;
                layer.BorderColor = info._borderColor;
                layer.BorderWidth = info._borderWidth;
                layer.CornerRadius = info._cornerRadius;
                view.ClipsToBounds = info._clipsToBounds;
            }

            #endregion
        }

        private sealed class ErrorButton : UIButton, IOrientationChangeListener
        {
            #region Fields

            private ValidationPopup _popup;
            private readonly TouchBindingErrorProvider _errorProvider;
            private IntPtr _textFieldHandle;
            private NSString _message;

            #endregion

            #region Constructors

            public ErrorButton(IntPtr handle)
                : base(handle)
            {
            }

            public ErrorButton(TouchBindingErrorProvider errorProvider, UITextField textField)
                            : base(new CGRect(0, 0, 25, 25))
            {
                _errorProvider = errorProvider;
                _textFieldHandle = textField.Handle;
                TouchUpInside += OnTouchUpInside;
                TouchToolkitExtensions.AddOrientationChangeListener(this);
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
                UITextField textField;
                var superview = GetTextFieldSuperview(out textField);
                if (superview == null)
                {
                    Tracer.Warn("Cannot get superview for " + textField);
                    return;
                }

                if (_popup == null)
                {
                    var showOnRect = textField.ConvertRectToView(Frame, superview);
                    var fieldFrame = superview.ConvertRectToView(textField.Frame, superview);
                    _popup = _errorProvider.CreateValidationPopup(showOnRect, fieldFrame);
                    _popup.TranslatesAutoresizingMaskIntoConstraints = false;
                    _popup.Message = _message;

                    _message = null;
                }
                if (_popup.Superview == null)
                {
                    superview.AddSubview(_popup);

                    var dict = new NSDictionary("popup", _popup);
                    _popup.Superview?.AddConstraints(NSLayoutConstraint.FromVisualFormat(@"H:|-0-[popup]-0-|",
                            NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
                    _popup.Superview?.AddConstraints(NSLayoutConstraint.FromVisualFormat(@"V:|-0-[popup]-0-|",
                        NSLayoutFormatOptions.DirectionLeadingToTrailing, null, dict));
                }
            }

            private void HidePopup()
            {
                if (_popup == null)
                    return;
                _popup.RemoveFromSuperview();
                _popup.ClearBindingsRecursively(true, true);
                _popup.DisposeEx();
                _popup = null;
            }

            private UIView GetTextFieldSuperview(out UITextField textField)
            {
                var fieldHandle = _textFieldHandle;
                if (fieldHandle == IntPtr.Zero)
                {
                    textField = null;
                    return null;
                }

                textField = Runtime.GetNSObject<UITextField>(_textFieldHandle);
                if (textField == null)
                    return null;
                var current = textField.Superview;
                UIView result = null;
                while (current != null && !(current is UIWindow))
                {
                    result = current;
                    current = current.Superview;
                }
                return result;
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

            #region Overrides of UIButton

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    HidePopup();
                    _textFieldHandle = IntPtr.Zero;
                }
                base.Dispose(disposing);
            }

            #endregion
        }

        #endregion

        #region Fields

        private static UIImage _defaultErrorImage;

        #endregion

        #region Constructors

        public TouchBindingErrorProvider()
        {
            if (_defaultErrorImage == null)
                _defaultErrorImage = UIImage.FromFile("error.png");
            ErrorBorderColor = ValidationPopup.ValidationColor;
            ErrorBorderWidth = 1f;
            CornerRadius = 5f;
            RightErrorImagePosition = true;
            ErrorImage = _defaultErrorImage;
        }

        #endregion

        #region Properties

        public float ErrorBorderWidth { get; set; }

        public float CornerRadius { get; set; }

        public UIColor ErrorBorderColor { get; set; }

        public UIImage ErrorImage { get; set; }

        public bool RightErrorImagePosition { get; set; }

        internal static Func<object, object> TryGetEntryField { get; set; }

        #endregion

        #region Methods

        protected virtual ValidationPopup CreateValidationPopup(CGRect showOnRect, CGRect fieldFrame)
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

        private void SetErrors(object target, IList<object> errors, bool isClear)
        {
            var hasErrors = errors.Count != 0;
            if (TryGetEntryField != null)
                target = TryGetEntryField(target);

            var nativeObject = target as INativeObject;
            if (!nativeObject.IsAlive())
                return;
            var uiView = target as UIView;
            if (uiView != null && ErrorBorderWidth > 0)
            {
                if (hasErrors)
                {
                    LayoutInfo.Save(uiView, target);
                    uiView.Layer.BorderColor = ErrorBorderColor.CGColor;
                    uiView.Layer.BorderWidth = ErrorBorderWidth;
                    uiView.Layer.CornerRadius = CornerRadius;
                    uiView.ClipsToBounds = true;
                }
                else
                    LayoutInfo.Restore(uiView, target);
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
                    if (isClear)
                        textField.RightView = null;
                    else if (errorButton == null)
                    {
                        errorButton = CreateErrorButton(textField);
                        textField.RightView = errorButton;
                    }
                }
                else
                {
                    textField.LeftViewMode = mode;
                    errorButton = textField.LeftView as ErrorButton;
                    if (isClear)
                        textField.LeftView = null;
                    else if (errorButton == null)
                    {
                        errorButton = CreateErrorButton(textField);
                        textField.LeftView = errorButton;
                    }
                }

                if (isClear)
                {
                    if (errorButton != null)
                    {
                        errorButton.ClearBindingsRecursively(true, true);
                        errorButton.DisposeEx();
                    }
                }
                else
                    errorButton.SetErrors(errors);
            }
        }

        #endregion

        #region Overrides of BindingErrorProviderBase

        protected override void SetErrors(object target, IList<object> errors, IDataContext context)
        {
            SetErrors(target, errors, false);
        }

        protected override void ClearErrors(object target, IDataContext context)
        {
            SetErrors(target, Empty.Array<object>(), true);
        }

        #endregion
    }
}
