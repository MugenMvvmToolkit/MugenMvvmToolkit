using System;
using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestModels;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class BindingSourceAccessorMock : DisposableObject, IBindingSourceAccessor, ISingleBindingSourceAccessor
    {
        #region Fields

        private IList<IObserver> _sources;

        #endregion

        #region Constructors

        public BindingSourceAccessorMock()
        {
            CanRead = true;
            CanWrite = true;
        }

        #endregion

        #region Properties

        public Func<IBindingSourceAccessor, IDataContext, bool, bool> SetValue { get; set; }

        public Func<IBindingMemberInfo, IDataContext, bool, object> GetValue { get; set; }

        #endregion

        #region Implementation of IBindingSourceAccessor

        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        public IList<IObserver> Sources
        {
            get
            {
                if (_sources == null)
                    return new[] { Source };
                return _sources;
            }
            set { _sources = value; }
        }

        public event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging;

        public event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged;

        public IObserver Source { get; set; }

        bool IBindingSourceAccessor.SetValue(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError)
        {
            return SetValue(targetAccessor, context, throwOnError);
        }

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
