using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestModels;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingSourceAccessorMock : DisposableObject, IBindingSourceAccessor, ISingleBindingSourceAccessor
    {
        #region Fields

        private IList<IBindingSource> _sources;

        #endregion

        #region Properties

        public Func<IBindingSourceAccessor, IDataContext, bool, bool> SetValue { get; set; }

        public Func<IBindingMemberInfo, IDataContext, bool, object> GetValue { get; set; }

        #endregion

        #region Implementation of IBindingSourceAccessor

        /// <summary>
        ///     Gets the underlying sources.
        /// </summary>
        public IList<IBindingSource> Sources
        {
            get
            {
                if (_sources == null)
                    return new[] { Source };
                return _sources;
            }
            set { _sources = value; }
        }

        /// <summary>
        ///     Occurs before the value changes.
        /// </summary>
        public event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        public event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;

        /// <summary>
        ///     Gets the underlying source.
        /// </summary>
        public IBindingSource Source { get; set; }

        /// <summary>
        ///     Sets the source value.
        /// </summary>
        /// <param name="targetAccessor">The specified accessor to get value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be set.
        /// </param>
        bool IBindingSourceAccessor.SetValue(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError)
        {
            return SetValue(targetAccessor, context, throwOnError);
        }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        /// <param name="targetMember">The specified member to set value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be obtained; false to return
        ///     <see cref="BindingConstants.InvalidValue" /> if the value cannot be obtained.
        /// </param>
        object IBindingSourceAccessor.GetValue(IBindingMemberInfo targetMember, IDataContext context, bool throwOnError)
        {
            return GetValue(targetMember, context, throwOnError);
        }

        #endregion

        #region Methods

        public void RaiseValueChanged(ValueAccessorChangedEventArgs e)
        {
            var handler = ValueChanged;
            if (handler != null) handler(this, e);
        }

        public void RaiseValueChanging(ValueAccessorChangingEventArgs e)
        {
            var handler = ValueChanging;
            if (handler != null) handler(this, e);
        }

        #endregion
    }
}