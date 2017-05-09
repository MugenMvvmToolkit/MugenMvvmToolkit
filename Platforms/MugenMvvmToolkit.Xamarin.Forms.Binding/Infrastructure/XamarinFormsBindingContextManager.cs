#region Copyright

// ****************************************************************************
// <copyright file="XamarinFormsBindingContextManager.cs">
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Infrastructure
{
    public class XamarinFormsBindingContextManager : BindingContextManager
    {
        #region Nested types

        public sealed class BindableObjectBindingContext : IBindingContext
        {
            #region Fields

            private static bool? _isWindows;
            private readonly WeakReference _sourceReference;
            private bool _changed;

            #endregion

            #region Constructors

            public BindableObjectBindingContext(BindableObject element)
            {
                _sourceReference = ServiceProvider.WeakReferenceFactory(element);
                if (IsWindows())
                    element.BindingContextChanged += RaiseDataContextChangedWindows;
                else
                    element.BindingContextChanged += RaiseDataContextChanged;
            }

            #endregion

            #region Implementation of IBindingContext

            public object Source => _sourceReference.Target;

            public bool IsAlive => true;

            public object Value
            {
                get { return ((BindableObject)Source)?.BindingContext; }
                set
                {
                    var target = (BindableObject)Source;
                    if (target == null)
                        return;
                    if (ReferenceEquals(value, BindingConstants.UnsetValue))
                        target.ClearValue(BindableObject.BindingContextProperty);
                    else
                        target.BindingContext = value;
                }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            private static bool IsWindows()
            {
                if (_isWindows == null)
                {
                    var platform = ServiceProvider.Application?.PlatformInfo.Platform;
                    _isWindows = platform != null && (platform == PlatformType.XamarinFormsUWP || platform == PlatformType.XamarinFormsWinPhone ||
                                                      platform == PlatformType.XamarinFormsWinRT);

                }
                return _isWindows.Value;
            }

            private void RaiseDataContextChangedWindows(object sender, EventArgs args)
            {
                //note hack for uwp cell binding context for first time it contains root binding context instead of item binding context
                if (!_changed)
                {
                    _changed = true;
                    var element = sender as Element;
                    if (element != null)
                    {
                        var cell = element;
                        while (cell != null)
                        {
                            if (cell is Cell)
                                break;
                            cell = cell.Parent;
                        }
                        var bindingContext = cell?.BindingContext;
                        if (bindingContext != null && ReferenceEquals(bindingContext, cell.Parent?.BindingContext) && ReferenceEquals(bindingContext, element.BindingContext))
                            return;
                    }
                }

                ValueChanged?.Invoke(this, EventArgs.Empty);
            }

            private void RaiseDataContextChanged(object sender, EventArgs args)
            {
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }

            #endregion
        }

        #endregion

        #region Overrides of BindingContextManager

        protected override IBindingContext CreateBindingContext(object item)
        {
            var bindableObject = item as BindableObject;
            if (bindableObject == null)
                return base.CreateBindingContext(item);
            return new BindableObjectBindingContext(bindableObject);
        }

        #endregion        
    }
}
