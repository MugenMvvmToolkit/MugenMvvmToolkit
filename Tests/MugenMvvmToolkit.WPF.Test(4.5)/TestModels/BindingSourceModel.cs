using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class BindingSourceEventNotifierModel
    {
        #region Properties

        public object ObjectProperty { get; set; }

        public BindingSourceEventNotifierModel NestedModel { get; set; }

        #endregion

        #region Events

        public event EventHandler ObjectPropertyChanged;

        public event EventHandler NestedModelChanged;

        #endregion

        #region Methods

        public void RaiseNestedModelChanged()
        {
            var handler = NestedModelChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void RaiseObjectPropertyChanged()
        {
            EventHandler handler = ObjectPropertyChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }

    public class BindingSourceModel : NotifyPropertyChangedBase
    {
        #region Fields

        public static readonly PropertyInfo IntPropertyInfo = typeof(BindingSourceModel).GetProperty("IntProperty");

        public const string EventName = "Event";
        public const string InvalidEventName = "ActionEvent";
        public const string VoidMethodName = "VoidMethod()";

        public readonly Dictionary<string, Dictionary<int, string>> DoubleIndexerValues =
            new Dictionary<string, Dictionary<int, string>>();

        public readonly Dictionary<string, string> IndexerValues = new Dictionary<string, string>();

        private int _intProperty;
        private BindingSourceNestedModel _nestedModel;
        private string _stringProperty;

        public string PublicField;
        private object _objectProperty;

        #endregion

        #region Properties

        public string WriteOnly { set { } }

        public int IntProperty
        {
            get { return _intProperty; }
            set
            {
                if (value == _intProperty) return;
                _intProperty = value;
                OnPropertyChanged();
            }
        }

        public string StringProperty
        {
            get { return _stringProperty; }
            set
            {
                if (value == _stringProperty) return;
                _stringProperty = value;
                OnPropertyChanged();
            }
        }

        public object ObjectProperty
        {
            get { return _objectProperty; }
            set
            {
                if (Equals(value, _objectProperty)) return;
                _objectProperty = value;
                OnPropertyChanged();
            }
        }

        public BindingSourceNestedModel NestedModel
        {
            get { return _nestedModel; }
            set
            {
                if (Equals(value, _nestedModel)) return;
                _nestedModel = value;
                OnPropertyChanged();
            }
        }

        public string this[string index]
        {
            get
            {
                string value;
                IndexerValues.TryGetValue(index, out value);
                return value;
            }
            set { IndexerValues[index] = value; }
        }

        public string this[string index, int intValue]
        {
            get
            {
                Dictionary<int, string> value;
                if (!DoubleIndexerValues.TryGetValue(index, out value))
                    return null;
                string s;
                value.TryGetValue(intValue, out s);
                return s;
            }
            set
            {
                Dictionary<int, string> dictionary;
                if (!DoubleIndexerValues.TryGetValue(index, out dictionary))
                {
                    dictionary = new Dictionary<int, string>();
                    DoubleIndexerValues[index] = dictionary;
                }
                dictionary[intValue] = value;
            }
        }

        public bool IsEnabled { get; set; }

        public bool IsFocused { get; set; }

        #endregion

        #region Events

        public event EventHandler Event;

        public event Action ActionEvent;

        #endregion

        #region Methods

        public void RaiseEvent()
        {
            EventHandler handler = Event;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void RaiseActionEvent()
        {
            Action handler = ActionEvent;
            if (handler != null) handler();
        }

        public void VoidMethod()
        {

        }

        public void MethodWithArgs(object arg)
        {
        }

        #endregion
    }

    public class BindingSourceNestedModel : BindingSourceModel
    {
    }
}