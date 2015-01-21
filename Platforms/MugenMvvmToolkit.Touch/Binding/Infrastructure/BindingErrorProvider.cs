#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProvider.cs">
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
using System.Reflection;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
#if XAMARIN_FORMS
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;
#else
using MonoTouch.Dialog;
#endif
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Binding.Infrastructure
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
            private readonly BindingErrorProvider _errorProvider;
            private UITextField _textField;
            private NSString _message;

            #endregion

            #region Constructors

            public ErrorButton(BindingErrorProvider errorProvider, UITextField textField)
                : base(new CGRect(0, 0, 25, 25))
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
                if (superview == null)
                {
                    Tracer.Warn("Cannot get superview for " + _textField);
                    return;
                }
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
                _popup.ClearBindingsHierarchically(true, true, true);
                _popup = null;
            }

            private UIView GetTextFieldSuperview()
            {
                if (_textField == null)
                    return null;
                var current = _textField.Superview;
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
                    _textField = null;
                }
                base.Dispose(disposing);
            }

            #endregion
        }

        #endregion

        #region Fields

#if XAMARIN_FORMS
        protected const string NativeViewKey = "##NativeView";
#else
        private static readonly Func<object, UITextField> GetEntryField;
#endif
        private static readonly UIImage DefaultErrorImage;

        #endregion

        #region Constructors

        static BindingErrorProvider()
        {
#if XAMARIN_FORMS
            Forms.ViewInitialized += FormsOnViewInitialized;
#else
            var field = typeof(EntryElement).GetField("entry", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(UITextField))
                GetEntryField = ServiceProvider.ReflectionManager.GetMemberGetter<UITextField>(field);
#endif
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
#if XAMARIN_FORMS
            var element = target as Element;
            if (element != null)
                target = GetNativeView(element);
#else
            var element = target as EntryElement;
            if (element != null && GetEntryField != null)
                target = GetEntryField(element);
#endif
            var nativeObject = target as INativeObject;
            if (nativeObject == null || !nativeObject.IsAlive())
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
                        errorButton.ClearBindingsHierarchically(true, true, true);
                }
                else
                    errorButton.SetErrors(errors);
            }
        }

#if XAMARIN_FORMS
        private static void FormsOnViewInitialized(object sender, ViewInitializedEventArgs args)
        {
            var view = args.View;
            if (view == null || args.NativeView == null)
                return;
            ServiceProvider.AttachedValueProvider.SetValue(view, NativeViewKey, args.NativeView);
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return;
            var dictionary = GetErrorsDictionary(view);
            if (dictionary != null)
            {
                foreach (var item in dictionary)
                    errorProvider.SetErrors(view, item.Key, item.Value, DataContext.Empty);
            }
        }

        [CanBeNull]
        protected virtual UIView GetNativeView([NotNull] Element element)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<UIView>(element, NativeViewKey, false);
        }
#endif
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
            base.SetErrors(target, errors, context);
            SetErrors(target, errors, false);
        }

        /// <summary>
        ///     Clears the errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="context">The specified context, if any.</param>
        protected override void ClearErrors(object target, IDataContext context)
        {
            base.SetErrors(target, Empty.Array<object>(), context);
            SetErrors(target, Empty.Array<object>(), true);
        }

        #endregion
    }
}