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

            private readonly WeakReference _sourceReference;

            #endregion

            #region Constructors

            public BindableObjectBindingContext(BindableObject element)
            {
                _sourceReference = ServiceProvider.WeakReferenceFactory(element);
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
