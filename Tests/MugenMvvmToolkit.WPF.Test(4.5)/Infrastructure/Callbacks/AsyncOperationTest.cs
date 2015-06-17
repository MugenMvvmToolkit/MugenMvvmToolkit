using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.WinRT.Infrastructure.Callbacks;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Callbacks
{
    [TestClass]
    public class AsyncOperationTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void IsCompletedTest()
        {
            var operation = new AsyncOperation<bool>();
            operation.IsCompleted.ShouldBeFalse();
            operation.SetResult(OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty));
            operation.IsCompleted.ShouldBeTrue();
        }

        [TestMethod]
        public void ResultTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);
            operation.SetResult(operationResult);
            operation.Result.ShouldEqual(operationResult);
        }
        
        [TestMethod]
        public void ContinueWithActionInterfaceTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            bool isInvoked = false;
            var continuationAction = new ActionContinuationMock
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    isInvoked = true;
                }
            };

            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeFalse();
            operation.SetResult(operationResult);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ContinueWithActionInterfaceToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            bool isInvoked = false;
            var continuationAction = new ActionContinuationMock
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    isInvoked = true;
                }
            };

            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ContinueWithActionInterfaceContinuationTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationAction = new ActionContinuationMock
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationAction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.SetResult(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }

        [TestMethod]
        public void ContinueWithActionInterfaceContinuationToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationAction = new ActionContinuationMock
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationAction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }





        [TestMethod]
        public void ContinueWithActionInterfaceGenericTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            bool isInvoked = false;
            var continuationAction = new ActionContinuationMock<bool>
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    isInvoked = true;
                }
            };

            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeFalse();
            operation.SetResult(operationResult);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ContinueWithActionInterfaceGenericToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            bool isInvoked = false;
            var continuationAction = new ActionContinuationMock<bool>
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    isInvoked = true;
                }
            };

            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            operation.ContinueWith(continuationAction);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ContinueWithActionInterfaceGenericContinuationTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationAction = new ActionContinuationMock<bool>
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationAction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.SetResult(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }

        [TestMethod]
        public void ContinueWithActionInterfaceGenericContinuationToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationAction = new ActionContinuationMock<bool>
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationAction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }





        [TestMethod]
        public void ContinueWithFunctionInterfaceTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            var funcResult = new object();
            var continuationFunction = new FunctionContinuationMock<object>
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    return funcResult;
                }
            };

            var op = operation.ContinueWith(continuationFunction);
            op.IsCompleted.ShouldBeFalse();
            operation.SetResult(operationResult);
            op.Result.Result.ShouldEqual(funcResult);

            operation.ContinueWith(continuationFunction);
            op.Result.Result.ShouldEqual(funcResult);
        }

        [TestMethod]
        public void ContinueWithFunctionInterfaceToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            var funcResult = new object();
            var continuationFunction = new FunctionContinuationMock<object>
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    return funcResult;
                }
            };

            var op = operation.ContinueWith(continuationFunction);
            op.IsCompleted.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);
            op.Result.Result.ShouldEqual(funcResult);

            operation.ContinueWith(continuationFunction);
            op.Result.Result.ShouldEqual(funcResult);
        }


        [TestMethod]
        public void ContinueWithFunctionInterfaceContinuationTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationFunction = new FunctionContinuationMock<object>
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationFunction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.SetResult(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }

        [TestMethod]
        public void ContinueWithFunctionInterfaceContinuationToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationFunction = new FunctionContinuationMock<object>
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationFunction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }





        [TestMethod]
        public void ContinueWithFunctionInterfaceGenericTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            var funcResult = new object();
            var continuationFunction = new FunctionContinuationMock<bool, object>
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    return funcResult;
                }
            };

            var op = operation.ContinueWith(continuationFunction);
            op.IsCompleted.ShouldBeFalse();
            operation.SetResult(operationResult);
            op.Result.Result.ShouldEqual(funcResult);

            operation.ContinueWith(continuationFunction);
            op.Result.Result.ShouldEqual(funcResult);
        }

        [TestMethod]
        public void ContinueWithFunctionInterfaceGenericToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult.CreateResult(OperationType.PageNavigation, this, true,
                DataContext.Empty);

            var funcResult = new object();
            var continuationFunction = new FunctionContinuationMock<bool, object>
            {
                Invoke = result =>
                {
                    result.ShouldEqual(operationResult);
                    return funcResult;
                }
            };

            var op = operation.ContinueWith(continuationFunction);
            op.IsCompleted.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);
            op.Result.Result.ShouldEqual(funcResult);

            operation.ContinueWith(continuationFunction);
            op.Result.Result.ShouldEqual(funcResult);
        }


        [TestMethod]
        public void ContinueWithFunctionInterfaceGenericContinuationTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationFunction = new FunctionContinuationMock<bool, object>
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationFunction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.SetResult(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }

        [TestMethod]
        public void ContinueWithFunctionInterfaceGenericContinuationToOperationCallbackTest()
        {
            var operation = new AsyncOperation<bool>();
            IOperationResult<bool> operationResult = OperationResult
                .CreateResult(OperationType.PageNavigation, this, true, DataContext.Empty);
            var exception = new TestException();
            var continuationFunction = new FunctionContinuationMock<bool, object>
            {
                Invoke = result =>
                {
                    throw exception;
                }
            };

            var continueWith = operation.ContinueWith(continuationFunction);
            continueWith.IsCompleted.ShouldBeFalse();
            operation.ToOperationCallback().Invoke(operationResult);

            continueWith.IsCompleted.ShouldBeTrue();
            continueWith.Result.IsFaulted.ShouldBeTrue();
            continueWith.Result.Exception.ShouldEqual(exception);
        }

        [TestMethod]
        public void OperationShouldUseDelegateToCreateMethodChain()
        {
            var operation = new AsyncOperation<bool>();
            bool isFuncInvoked = false;
            bool isFuncGenericInvoked = false;
            bool isActionGenericInvoked = false;
            bool isActionInvoked = false;

            var func = new Func<IOperationResult, object>(op =>
            {
                isFuncInvoked = true;
                return operation;
            });
            var funcGeneric = new Func<IOperationResult<object>, bool>(o =>
            {
                isFuncGenericInvoked = true;
                o.Result.ShouldEqual(operation);
                return true;
            });
            var actionGeneric = new Action<IOperationResult<bool>>(op =>
            {
                isActionGenericInvoked = true;
                op.Result.ShouldBeTrue();
            });
            var action = new Action<IOperationResult>(op =>
            {
                isActionInvoked = true;
            });

            operation.ContinueWith(continuationFunction: func)
                     .ContinueWith(funcGeneric)
                     .ContinueWith(actionGeneric)
                     .ContinueWith(action);

            operation.SetResult(OperationResult.CreateResult(OperationType.PageNavigation, this, true));

            isFuncInvoked.ShouldBeTrue();
            isFuncGenericInvoked.ShouldBeTrue();
            isActionGenericInvoked.ShouldBeTrue();
            isActionInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void OperationShouldUseDelegateToCreateMethodChainOperationCallback()
        {
            var operation = new AsyncOperation<bool>();
            bool isFuncInvoked = false;
            bool isFuncGenericInvoked = false;
            bool isActionGenericInvoked = false;
            bool isActionInvoked = false;

            var func = new Func<IOperationResult, object>(op =>
            {
                isFuncInvoked = true;
                return operation;
            });
            var funcGeneric = new Func<IOperationResult<object>, bool>(o =>
            {
                isFuncGenericInvoked = true;
                o.Result.ShouldEqual(operation);
                return true;
            });
            var actionGeneric = new Action<IOperationResult<bool>>(op =>
            {
                isActionGenericInvoked = true;
                op.Result.ShouldBeTrue();
            });
            var action = new Action<IOperationResult>(op =>
            {
                isActionInvoked = true;
            });

            operation.ContinueWith(continuationFunction: func)
                     .ContinueWith(funcGeneric)
                     .ContinueWith(actionGeneric)
                     .ContinueWith(action);

            operation.ToOperationCallback()
                     .Invoke(OperationResult.CreateResult(OperationType.PageNavigation, this, true));

            isFuncInvoked.ShouldBeTrue();
            isFuncGenericInvoked.ShouldBeTrue();
            isActionGenericInvoked.ShouldBeTrue();
            isActionInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void OperationShouldUseDelegateToCreateMethodChainSerializableCallback()
        {
            var operation = new AsyncOperation<bool>();
            bool isFuncInvoked = false;
            bool isFuncGenericInvoked = false;
            bool isActionGenericInvoked = false;
            bool isActionInvoked = false;

            var func = new Func<IOperationResult, object>(op =>
            {
                isFuncInvoked = true;
                return operation;
            });
            var funcGeneric = new Func<IOperationResult<object>, bool>(o =>
            {
                isFuncGenericInvoked = true;
                o.Result.ShouldEqual(operation);
                return true;
            });
            var actionGeneric = new Action<IOperationResult<bool>>(op =>
            {
                isActionGenericInvoked = true;
                op.Result.ShouldBeTrue();
            });
            var action = new Action<IOperationResult>(op =>
            {
                isActionInvoked = true;
            });

            operation.ContinueWith(continuationFunction: func)
                     .ContinueWith(funcGeneric)
                     .ContinueWith(actionGeneric)
                     .ContinueWith(action);

            OperationCallbackFactory.CreateSerializableCallback = @delegate =>
            {
                Func<IOperationResult, object> invoke = result => @delegate.DynamicInvoke(result);
                return new SerializableCallbackMock() { Invoke = invoke };
            };

            operation.ToSerializableCallback()
                     .Invoke(OperationResult.CreateResult(OperationType.PageNavigation, this, true));

            isFuncInvoked.ShouldBeTrue();
            isFuncGenericInvoked.ShouldBeTrue();
            isActionGenericInvoked.ShouldBeTrue();
            isActionInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void OperationShouldUseDelegateToConvertCallbackToSerializable()
        {
            var operation = new AsyncOperation<bool>();
            var action = new Action<IOperationResult>(op => { });
            var actionGeneric = new Action<IOperationResult<bool>>(op => { });
            var func = new Func<IOperationResult, object>(op => null);
            var funcGeneric = new Func<IOperationResult<object>, bool>(o => true);
            var delegates = new List<Delegate> { action, actionGeneric, func, funcGeneric };

            operation.ContinueWith(continuationFunction: func)
                     .ContinueWith(funcGeneric)
                     .ContinueWith(actionGeneric)
                     .ContinueWith(action);

            OperationCallbackFactory.CreateSerializableCallback = @delegate =>
            {
                delegates.Remove(@delegate).ShouldBeTrue();
                return new SerializableCallbackMock();
            };

            operation.ToSerializableCallback().ShouldNotBeNull();
            delegates.ShouldBeEmpty();
        }

        [TestMethod]
        public void OperationShouldUseDelegateToConvertCallbackToSerializableDuringSerialization()
        {
            var operation = new AsyncOperation<bool>();
            var action = new Action<IOperationResult>(op => { });
            var actionGeneric = new Action<IOperationResult<bool>>(op => { });
            var func = new Func<IOperationResult, object>(op => null);
            var funcGeneric = new Func<IOperationResult<object>, bool>(o => true);
            var delegates = new List<Delegate> { action, actionGeneric, func, funcGeneric };

            operation.ContinueWith(continuationFunction: func)
                     .ContinueWith(funcGeneric)
                     .ContinueWith(actionGeneric)
                     .ContinueWith(action);

            OperationCallbackFactory.CreateSerializableCallback = @delegate =>
            {
                delegates.Remove(@delegate).ShouldBeTrue();
                return new SerializableCallbackMock();
            };

            var operationCallback = operation.ToOperationCallback();
            Serializer.Serialize(operationCallback);
            delegates.ShouldBeEmpty();
        }

        #endregion

        #region Properties

        protected OperationCallbackFactoryMock OperationCallbackFactory { get; set; }

        protected ISerializer Serializer { get; set; }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            OperationCallbackFactory = new OperationCallbackFactoryMock();
            ServiceProvider.OperationCallbackFactory = OperationCallbackFactory;
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