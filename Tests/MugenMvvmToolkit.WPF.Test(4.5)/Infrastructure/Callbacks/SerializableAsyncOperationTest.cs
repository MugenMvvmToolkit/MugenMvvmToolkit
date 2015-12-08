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
using MugenMvvmToolkit.WinRT.Infrastructure.Callbacks;
using MugenMvvmToolkit.Silverlight.Infrastructure.Callbacks;
using MugenMvvmToolkit.WPF.Infrastructure.Callbacks;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Callbacks
{
    [TestClass]
    public class SerializableAsyncOperationTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void OperationShouldSerializeAsyncMethod()
        {
            ServiceProvider.OperationCallbackFactory = new SerializableOperationCallbackFactory(Serializer);
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> result = OperationResult.CreateResult(OperationType.PageNavigation, this, true);

            AsyncMethod(operation, true);
            var callback = operation.ToOperationCallback();
            var serialize = Serializer.Serialize(callback);
            serialize.Position = 0;
            callback = (IOperationCallback)Serializer.Deserialize(serialize);

            IocContainer.GetFunc = (type, s, arg3) =>
            {
                if (type == this.GetType())
                    return this;
                return Activator.CreateInstance(type);
            };
            AsyncMethodInvoked.ShouldBeFalse();
            callback.Invoke(result);
            AsyncMethodInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void OperationShouldSerializeAsyncMethodWithViewModel()
        {
            var vmMock = new NavigableViewModelMock();
            ServiceProvider.OperationCallbackFactory = new SerializableOperationCallbackFactory(Serializer);
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> result = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                new NavigationContext(NavigationType.Page, NavigationMode.Back, vmMock, vmMock, this));

            AsyncMethodWithViewModel(operation, true, vmMock);
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
            AsyncMethodInvoked.ShouldBeFalse();
            ViewModel.ShouldBeNull();
            callback.Invoke(result);
            AsyncMethodInvoked.ShouldBeTrue();
            ViewModel.ShouldEqual(vmMock);
        }

        private async void AsyncMethod(IAsyncOperation<bool> asyncOperation, bool result)
        {
            bool b = await asyncOperation;
            b.ShouldEqual(result);
            AsyncMethodInvoked = true;
        }

        private async void AsyncMethodWithViewModel(IAsyncOperation<bool> asyncOperation, bool result, IViewModel viewModel)
        {
            bool b = await asyncOperation;
            b.ShouldEqual(result);
            ViewModel = viewModel;
            AsyncMethodInvoked = true;
        }

        #endregion

        #region Properties

        protected bool AsyncMethodInvoked { get; set; }

        protected IViewModel ViewModel { get; set; }

        protected ISerializer Serializer { get; set; }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            AsyncMethodInvoked = false;
            ViewModel = null;
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
