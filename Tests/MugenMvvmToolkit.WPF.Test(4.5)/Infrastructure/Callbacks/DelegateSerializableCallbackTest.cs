using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.UWP.Infrastructure.Callbacks;
using MugenMvvmToolkit.WPF.Infrastructure.Callbacks;
using MugenMvvmToolkit.Silverlight.Infrastructure.Callbacks;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Callbacks
{
    [TestClass]
    public class DelegateSerializableCallbackTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void OperationShouldSerializeStaticMethods()
        {
            ServiceProvider.OperationCallbackFactory = new SerializableOperationCallbackFactory(Serializer);
            var operation = new AsyncOperation<bool>();
            var result = OperationResult.CreateResult(OperationType.PageNavigation, this, true);

            operation.ContinueWith(operationResult => { ResultStatic = operationResult; });
            ResultStatic.ShouldBeNull();
            var callback = operation.ToOperationCallback();
            var serialize = Serializer.Serialize(callback);
            serialize.Position = 0;
            callback = (IOperationCallback)Serializer.Deserialize(serialize);

            ResultStatic.ShouldBeNull();
            callback.Invoke(result);
            ResultStatic.ShouldEqual(result);
        }

        [TestMethod]
        public void OperationShouldSerializeInstanceMethods()
        {
            ServiceProvider.OperationCallbackFactory = new SerializableOperationCallbackFactory(Serializer);
            var operation = new AsyncOperation<bool>();
            var result = OperationResult.CreateResult(OperationType.PageNavigation, this, true);

            operation.ContinueWith(CallbackMethod);
            Result.ShouldBeNull();
            var callback = operation.ToOperationCallback();
            var serialize = Serializer.Serialize(callback);
            serialize.Position = 0;
            callback = (IOperationCallback)Serializer.Deserialize(serialize);

            IocContainer.GetFunc = (type, s, arg3) =>
            {
                type.ShouldEqual(GetType());
                return this;
            };
            Result.ShouldBeNull();
            callback.Invoke(result);
            Result.ShouldEqual(result);
        }

        [TestMethod]
        public void OperationShouldSerializeAnonymousClassMethods()
        {
            var vmMock = new NavigableViewModelMock();
            ServiceProvider.OperationCallbackFactory = new SerializableOperationCallbackFactory(Serializer);
            var operation = new AsyncOperation<bool>();
            var result = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                new NavigationContext(NavigationType.Page, NavigationMode.Back, vmMock, vmMock, this));

            operation.ContinueWith(operationResult => CallbackAnonMethod(operationResult, vmMock));
            ResultAnon.ShouldBeNull();
            ViewModel.ShouldBeNull();

            var callback = operation.ToOperationCallback();
            var serialize = Serializer.Serialize(callback);
            serialize.Position = 0;
            callback = (IOperationCallback)Serializer.Deserialize(serialize);

            IocContainer.GetFunc = (type, s, arg3) =>
            {
                if (type == GetType())
                    return this;
                return Activator.CreateInstance(type);
            };

            ResultAnon.ShouldBeNull();
            ViewModel.ShouldBeNull();

            callback.Invoke(result);
            ResultAnon.ShouldEqual(result);
            ViewModel.ShouldEqual(vmMock);
        }

        private void CallbackMethod(IOperationResult<bool> result)
        {
            Result = result;
        }

        public void CallbackAnonMethod(IOperationResult<bool> result, NavigableViewModelMock vm)
        {
            ResultAnon = result;
            ViewModel = vm;
        }

        #endregion

        #region Properties

        protected static IOperationResult ResultStatic { get; set; }

        protected IOperationResult Result { get; set; }

        protected IOperationResult ResultAnon { get; set; }

        protected IViewModel ViewModel { get; set; }

        protected ISerializer Serializer { get; set; }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            Serializer = new Serializer(new[]
                {
                    GetType().GetAssembly(), typeof (ApplicationSettings).GetAssembly(),
                    typeof (SerializableOperationCallbackFactory).GetAssembly()
                });
            base.OnInit();
        }

        #endregion
    }
}
