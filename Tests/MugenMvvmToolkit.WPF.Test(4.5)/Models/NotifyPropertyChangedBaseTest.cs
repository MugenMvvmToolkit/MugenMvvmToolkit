#if WPF && NET45
extern alias core;
#endif
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.Infrastructure;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.Models
{
#if WPF
#if NET45
    [core::System.Serializable]
#else
    [Serializable]
#endif
#endif
    [DataContract(IsReference = true)]
    [TestClass]
    public class NotifyPropertyChangedBaseTest : NotifyPropertyChangedBase
    {
        #region Fields

#if WPF
#if NET45
        [core::System.NonSerialized]
#else
        [NonSerialized]
#endif
#endif
        [XmlIgnore]
        private ThreadManagerMock _threadManagerMock;
        private ExecutionMode _executionMode;
        private bool _useGlobalSetting;
        private string _setProperty;
        private string _property;
        private string _propertyExp;

        #endregion

        #region Constructors

        public NotifyPropertyChangedBaseTest()
        {
            _threadManagerMock = new ThreadManagerMock();
        }

        #endregion

        #region Properties

        public string SetPropertyName
        {
            get { return _setProperty; }
            set
            {
                if (_useGlobalSetting)
                {
                    SetProperty(ref _setProperty, value);
                    return;
                }
                SetProperty(ref _setProperty, value, "SetPropertyName", _executionMode);
            }
        }

        public string SetPropertyExpression
        {
            get { return _setProperty; }
            set
            {
                if (_useGlobalSetting)
                    this.SetProperty(ref _setProperty, value, () => vm => SetPropertyExpression);
                else
                    this.SetProperty(ref _setProperty, value, () => vm => SetPropertyExpression, _executionMode);
            }
        }

        public string PropertyString
        {
            get { return _property; }
            set
            {
                if (value == _property) return;
                _property = value;
                if (_useGlobalSetting)
                {
                    OnPropertyChanged();
                }
                OnPropertyChanged("PropertyString", _executionMode);
            }
        }

        public string PropertyExp
        {
            get { return _propertyExp; }
            set
            {
                if (value == _propertyExp) return;
                _propertyExp = value;
                if (_useGlobalSetting)
                    this.OnPropertyChanged(() => m => m.PropertyExp);
                else
                    this.OnPropertyChanged(() => m => m.PropertyExp, _executionMode);
            }
        }

        public ExecutionMode PropertyExecutionMode { get; set; }

        #endregion

        #region Test infrastructure

        [TestInitialize]
        public void InitializeTest()
        {
            PropertyExecutionMode = ExecutionMode.None;
        }

        #region Overrides of NotifyPropertyChangedBase

        protected override ExecutionMode PropertyChangeExecutionMode
        {
            get { return PropertyExecutionMode; }
        }

        #endregion

        [TestMethod]
        public void SetPropertyNameTest()
        {
            BasePropertyTest(() => SetPropertyName, s => SetPropertyName = s, "SetPropertyName");
        }

        [TestMethod]
        public void SetPropertyExpressionTest()
        {
            BasePropertyTest(() => SetPropertyExpression, s => SetPropertyExpression = s, "SetPropertyExpression");
        }

        [TestMethod]
        public void PropertyStringTest()
        {
            BasePropertyTest(() => PropertyString, s => PropertyString = s, "PropertyString");
        }

        [TestMethod]
        public void PropertyExpTest()
        {
            BasePropertyTest(() => PropertyExp, s => PropertyExp = s, "PropertyExp");
        }

        [TestMethod]
        public void TestClearEvents()
        {
            _executionMode = ExecutionMode.None;
            int count = 0;
            string lastProp = null;
            PropertyChanged += (sender, args) =>
                                   {
                                       lastProp = args.PropertyName;
                                       count++;
                                   };
            this.OnPropertyChanged(() => vm => vm.SetPropertyName);
            lastProp.ShouldEqual("SetPropertyName");
            count.ShouldEqual(1);

            //Clear events
            ClearPropertyChangedSubscribers();
            count = 0;
            lastProp = null;
            this.OnPropertyChanged(() => vm => vm.SetPropertyName);
            lastProp.ShouldBeNull();
            count.ShouldEqual(0);
        }

        [TestMethod]
        public void TestThreadManager()
        {
            _threadManagerMock = null;
            ServiceProvider.ThreadManager = new ThreadManagerMock();
            ThreadManager.ShouldEqual(ServiceProvider.ThreadManager);
            var localManager = new ThreadManagerMock();
            _threadManagerMock = localManager;
            ThreadManager.ShouldEqual(localManager);
        }

        protected void BasePropertyTest(Func<string> getProperty, Action<string> updateProperty, string propertyName)
        {
            _useGlobalSetting = false;
            updateProperty(null);
            BasePropertyTestInternal(getProperty, updateProperty, propertyName, b => _executionMode = b, false);
            ClearPropertyChangedSubscribers();
            updateProperty(null);
            _useGlobalSetting = true;

            using (SuspendNotifications())
            {
                BasePropertyTestInternal(getProperty, updateProperty, propertyName,
                                     b => PropertyExecutionMode = b, true);
            }
        }

        private void BasePropertyTestInternal(Func<string> getProperty, Action<string> updateProperty, string propertyName, Action<ExecutionMode> changeRaiseInUi, bool suspend)
        {
            changeRaiseInUi(ExecutionMode.None);
            int count = 0;
            string lastProp = null;
            const string value = "value";
            PropertyChanged += (sender, args) =>
                                   {
                                       lastProp = args.PropertyName;
                                       count++;
                                   };

            lastProp.ShouldBeNull();
            getProperty().ShouldBeNull();
            updateProperty(value);
            getProperty().ShouldEqual(value);
            if (!suspend)
            {
                lastProp.ShouldEqual(propertyName);
                count.ShouldEqual(1);
            }
            else
                count.ShouldEqual(0);

            //Invoke in ui thread
            updateProperty(null);
            changeRaiseInUi(ExecutionMode.AsynchronousOnUiThread);
            count = 0;
            lastProp = null;
            _threadManagerMock.InvokeOnUiThreadAsync = null;
            updateProperty(value);
            getProperty().ShouldEqual(value);
            lastProp.ShouldBeNull();
            if (!suspend)
            {
                _threadManagerMock.InvokeOnUiThreadAsync.ShouldNotBeNull();
                _threadManagerMock.InvokeOnUiThreadAsync.Invoke();
                lastProp.ShouldEqual(propertyName);
                count.ShouldEqual(1);
            }
            else
            {
                _threadManagerMock.InvokeOnUiThreadAsync.ShouldBeNull();
                count.ShouldEqual(0);
            }

            //Invoke in ui thread synchronous
            updateProperty(null);
            changeRaiseInUi(ExecutionMode.SynchronousOnUiThread);
            count = 0;
            lastProp = null;
            _threadManagerMock.InvokeOnUiThread = null;
            updateProperty(value);
            getProperty().ShouldEqual(value);
            lastProp.ShouldBeNull();
            if (!suspend)
            {
                _threadManagerMock.InvokeOnUiThread.ShouldNotBeNull();
                _threadManagerMock.InvokeOnUiThread.Invoke();
                lastProp.ShouldEqual(propertyName);
                count.ShouldEqual(1);
            }
            else
            {
                _threadManagerMock.InvokeOnUiThread.ShouldBeNull();
                count.ShouldEqual(0);
            }
        }

        #endregion

        #region Overrides of NotifyPropertyChangedBase

        protected override IThreadManager ThreadManager
        {
            get { return _threadManagerMock ?? base.ThreadManager; }
        }

        #endregion
    }

    [TestClass]
    public class NotifyPropertyChangedBaseSerializationTest : SerializationTestBase<NotifyPropertyChangedBaseTest>
    {
        #region Overrides of SerializationTestBase

        protected override NotifyPropertyChangedBaseTest GetObject()
        {
            return new NotifyPropertyChangedBaseTest { PropertyExecutionMode = ExecutionMode.None };
        }

        protected override void AssertObject(NotifyPropertyChangedBaseTest deserializedObj)
        {
        }

        #endregion
    }
}
