using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Accessors;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Core
{
    [TestClass]
    public class DataBindingTest : TestBase
    {
        #region Nested types

        public sealed class CycleItem : NotifyPropertyChangedBase
        {
            private int _property = 1;

            public int Property
            {
                get { return _property; }
                set
                {
                    _property += value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Methods

        [TestMethod]
        public virtual void BindingShouldBeRegisteredInBindingManager()
        {
            var target = new object();
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            DataBinding binding = CreateDataBinding(
                new BindingSourceAccessorMock { Source = new ObserverMock { GetActualSource = b => target, Path = path } },
                new BindingSourceAccessorMock(), bindingManager);
            bindingManager.Register(target, path.Path, binding);
            bindingManager.GetBindings(target).Single().ShouldEqual(binding);
            bindingManager.GetBindings(target, path.Path).Single().ShouldEqual(binding);
        }

        [TestMethod]
        public virtual void BindingShouldCorrectInitializeProperties()
        {
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();
            DataBinding binding = CreateDataBinding(target, source, bindingManager);

            binding.TargetAccessor.ShouldEqual(target);
            binding.SourceAccessor.ShouldEqual(source);
            binding.Behaviors.ShouldBeEmpty();
        }

        [TestMethod]
        public virtual void BindingShouldAddSelfToDataContext()
        {
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();
            DataBinding binding = CreateDataBinding(target, source, bindingManager);

            var dataContext = binding.Context;
            dataContext.Count.ShouldEqual(1);
            dataContext.GetData(BindingConstants.Binding).ShouldEqual(binding);
        }

        [TestMethod]
        public virtual void BindingShouldRaiseExceptionEventWhenUpdateTargetThrowException()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock
            {
                Source = new ObserverMock
                {
                    GetActualSource = b => new object(),
                    Path = path,
                    IsValid = b => true
                }
            };
            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            target.SetValue = (func, context, arg3) => { throw new TestException(); };
            binding.BindingException += (sender, args) =>
            {
                args.Action.ShouldEqual(BindingAction.UpdateTarget);
                args.Exception.InnerException.ShouldBeType<TestException>();
                isInvoked = true;
            };
            binding.UpdateTarget();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldRaiseExceptionEventWhenUpdateSourceThrowException()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock
                {
                    GetActualSource = b => new object(),
                    Path = path,
                    IsValid = b => true
                }
            };
            var source = new BindingSourceAccessorMock();

            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            source.SetValue = (func, context, arg3) => { throw new TestException(); };
            binding.BindingException += (sender, args) =>
            {
                args.Action.ShouldEqual(BindingAction.UpdateSource);
                args.Exception.InnerException.ShouldBeType<TestException>();
                isInvoked = true;
            };
            binding.UpdateSource();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldRaiseEventWhenUpdateTargetTrue()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock
            {
                Source = new ObserverMock
                {
                    IsValid = b => true
                }
            };
            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            target.SetValue = (func, context, arg3) => true;
            binding.BindingUpdated += (sender, args) =>
            {
                args.Action.ShouldEqual(BindingAction.UpdateTarget);
                args.Result.ShouldBeTrue();
                isInvoked = true;
            };
            binding.UpdateTarget();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldRaiseEventWhenUpdateTargetFalse()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();
            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            target.SetValue = (func, context, arg3) => false;
            binding.BindingUpdated += (sender, args) =>
            {
                args.Action.ShouldEqual(BindingAction.UpdateTarget);
                args.Result.ShouldBeFalse();
                isInvoked = true;
            };
            binding.UpdateTarget();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldRaiseEventWhenUpdateSourceTrue()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock
                {
                    GetActualSource = b => new object(),
                    Path = path,
                    IsValid = b => true
                }
            };
            var source = new BindingSourceAccessorMock();

            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            source.SetValue = (func, context, arg3) => true;
            binding.BindingUpdated += (sender, args) =>
            {
                args.Action.ShouldEqual(BindingAction.UpdateSource);
                args.Result.ShouldBeTrue();
                isInvoked = true;
            };
            binding.UpdateSource();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldRaiseEventWhenUpdateSourceFalse()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();

            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            source.SetValue = (func, context, arg3) => false;
            binding.BindingUpdated += (sender, args) =>
            {
                args.Action.ShouldEqual(BindingAction.UpdateSource);
                args.Result.ShouldBeFalse();
                isInvoked = true;
            };
            binding.UpdateSource();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldThrowExceptionDuplicateIdBehavior()
        {
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();
            DataBinding binding = CreateDataBinding(target, source, bindingManager);

            var first = new BindingBehaviorMock { Id = Guid.Empty, Attach = binding1 => true };
            var second = new BindingBehaviorMock { Id = Guid.Empty, Attach = binding1 => true };
            binding.Behaviors.Add(first);
            ShouldThrow(() => binding.Behaviors.Add(second));
        }

        [TestMethod]
        public virtual void BindingShouldNotAddSameBehavior()
        {
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();
            DataBinding binding = CreateDataBinding(target, source, bindingManager);

            var first = new BindingBehaviorMock
            {
                Id = Guid.Empty,
                Attach = binding1 => true
            };
            binding.Behaviors.Add(first);
            ShouldThrow(() => binding.Behaviors.Add(first));
        }

        [TestMethod]
        public virtual void BindingShouldNotAddBehaviorIfAttachReturnsFalse()
        {
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();
            DataBinding binding = CreateDataBinding(target, source, bindingManager);

            var first = new BindingBehaviorMock
            {
                Id = Guid.Empty,
                Attach = binding1 => false
            };
            binding.Behaviors.Add(first);
            binding.Behaviors.Count.ShouldEqual(0);
            binding.Behaviors.Contains(first).ShouldBeFalse();
        }

        [TestMethod]
        public virtual void BindingShouldCallAttachDetachMethodInBehavior()
        {
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock { GetActualSource = b => new object(), Path = path }
            };
            var source = new BindingSourceAccessorMock();
            DataBinding binding = CreateDataBinding(target, source, bindingManager);

            int countAttach = 0;
            int countDetach = 0;
            var first = new BindingBehaviorMock
            {
                Id = Guid.Empty,
                Attach = binding1 =>
                {
                    countAttach++;
                    return true;
                },
                Detach = binding1 => countDetach++
            };
            binding.Behaviors.Add(first);
            countAttach.ShouldEqual(1);
            countDetach.ShouldEqual(0);

            binding.Behaviors.Remove(first);
            countAttach.ShouldEqual(1);
            countDetach.ShouldEqual(1);
        }

        [TestMethod]
        public virtual void BindingShouldUpdateSourceWithBindingContext()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock
                {
                    GetActualSource = b => new object(),
                    Path = path,
                    IsValid = b => true
                }
            };
            var source = new BindingSourceAccessorMock();

            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            source.SetValue = (func, context, arg3) =>
            {
                context.ShouldEqual(binding.Context);
                arg3.ShouldBeTrue();
                isInvoked = true;
                return true;
            };
            binding.UpdateSource();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldUpdateTargetWithBindingContext()
        {
            bool isInvoked = false;
            IBindingPath path = BindingPath.Create("test");
            var bindingManager = new BindingManager();
            var target = new BindingSourceAccessorMock
            {
                Source = new ObserverMock
                {
                    GetActualSource = b => new object(),
                    Path = path
                }
            };
            var source = new BindingSourceAccessorMock
            {
                Source = new ObserverMock
                {
                    IsValid = b => true
                }
            };
            DataBinding binding = CreateDataBinding(target, source, bindingManager);
            target.SetValue = (func, context, arg3) =>
            {
                context.ShouldEqual(binding.Context);
                arg3.ShouldBeTrue();
                isInvoked = true;
                return true;
            };
            binding.UpdateTarget();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void BindingShouldSuppressCycle()
        {
            var cycleItem1 = new CycleItem();
            var cycleItem2 = new CycleItem();
            var dataBinding = CreateDataBinding(
                new BindingSourceAccessor(new SinglePathObserver(cycleItem1, BindingPath.Create("Property"), true),
                    DataContext.Empty, true),
                new BindingSourceAccessor(new SinglePathObserver(cycleItem2, BindingPath.Create("Property"), true),
                    DataContext.Empty, false));
            dataBinding.Behaviors.Add(new TwoWayBindingMode());
            cycleItem2.Property = 10;

            Tracer.Warn("Item1: {0}, Item2: {1}", cycleItem1.Property, cycleItem2.Property);
        }

        protected virtual DataBinding CreateDataBinding(ISingleBindingSourceAccessor target,
            IBindingSourceAccessor source, IBindingManager manager = null)
        {
            if (manager != null)
                BindingServiceProvider.BindingManager = manager;
            return new DataBinding(target, source);
        }

        #endregion

        #region Overrides of TestBase

        protected override void OnInit()
        {
            base.OnInit();
            ThreadManager.ImmediateInvokeAsync = true;
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ThreadManager.ImmediateInvokeOnUiThread = true;
        }

        #endregion
    }
}