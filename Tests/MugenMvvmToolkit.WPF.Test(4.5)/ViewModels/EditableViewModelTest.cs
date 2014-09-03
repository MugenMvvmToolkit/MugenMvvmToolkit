using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.Test.TestViewModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class EditableViewModelTest : ValidatableViewModelTest
    {
        #region Properties

        public StateManagerMock StateManager { get; private set; }

        #endregion

        #region Test methods

        [TestMethod]
        public void EntityShouldBeNullBeforeInitializedIfFactoryNull()
        {
            ServiceProvider.DefaultEntityFactory = null;
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.Entity.ShouldBeNull();
        }

        [TestMethod]
        public void EntityShouldBeNotNullBeforeInitializedIfFactoryNotNull()
        {
            ServiceProvider.DefaultEntityFactory = Activator.CreateInstance;
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.Entity.ShouldNotBeNull();
        }

        [TestMethod]
        public void IsEntityInitializedShouldBeFalseBeforeInitialized()
        {
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.IsEntityInitialized.ShouldBeFalse();
        }

        [TestMethod]
        public void ApplyChangesShouldThrowExceptionIfVmNotInitialized()
        {
            var viewModel = GetViewModel<TestEditableViewModel>();
            ShouldThrow<InvalidOperationException>(() => viewModel.ApplyChanges());
        }

        [TestMethod]
        public void CancelChangesShouldThrowExceptionIfVmNotInitialized()
        {
            var testEditableViewModel = GetViewModel<TestEditableViewModel>();
            ShouldThrow<InvalidOperationException>(() => testEditableViewModel.CancelChanges());
        }

        [TestMethod]
        public void DoubleInitializeEntityShouldNotThrowException()
        {
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(new object(), true);
            viewModel.InitializeEntity(new object(), true);
        }

        [TestMethod]
        public void InitializeEntityShouldSaveStateUsingStateManager()
        {
            bool isInvoked = false;
            StateManager.CreateSnapshot = o =>
            {
                isInvoked = true;
                return new EntitySnapshotMock();
            };
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(new object(), true);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void CancelChangesShouldCancelStateUsingStateManager()
        {
            bool isInvoked = false;
            var mock = new EntitySnapshotMock
            {
                Restore = o => isInvoked = true
            };
            StateManager.CreateSnapshot = o => mock;
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(new object(), true);
            viewModel.CancelChanges();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void InitializeEntityShouldUseSameEntity()
        {
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, true);
            viewModel.Entity.ShouldEqual(entity);
        }

        [TestMethod]
        public void CancelChangesShouldShouldUseSameEntity()
        {
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, true);
            viewModel.CancelChanges().ShouldEqual(entity);
        }

        [TestMethod]
        public void ApplyChangesShouldUseSameEntity()
        {
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, true);
            viewModel.Entity.ShouldEqual(entity);
            viewModel.ApplyChanges();
            viewModel.Entity.ShouldEqual(entity);
        }

        [TestMethod]
        public void ApplyChangesShouldReturnEntityStateEntryWithEntityAdded()
        {
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, true);
            IList<IEntityStateEntry> entityStateEntries = viewModel.ApplyChanges();
            entityStateEntries.Single().Entity.ShouldEqual(entity);
            entityStateEntries.Single().State.ShouldEqual(EntityState.Added);
        }

        [TestMethod]
        public void ApplyChangesShouldReturnEntityStateEntryWithEntityModified()
        {
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, false);
            IList<IEntityStateEntry> entityStateEntries = viewModel.ApplyChanges();
            entityStateEntries.Single().Entity.ShouldEqual(entity);
            entityStateEntries.Single().State.ShouldEqual(EntityState.Modified);
        }

        [TestMethod]
        public void HasChangesShouldGetValueFromStateManagerIfItIsSupportChangeDetection()
        {
            bool stateValue = false;
            var entity = new object();
            var mock = new EntitySnapshotMock
            {
                HasChanges = o => stateValue,
                SupportChangeDetection = true
            };
            StateManager.CreateSnapshot = o => mock;

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, false);
            viewModel.HasChanges.ShouldBeFalse();
            stateValue = true;
            viewModel.HasChanges.ShouldBeTrue();
        }

        [TestMethod]
        public void HasChangesShouldNotGetValueFromStateManagerIfItIsNotSupportChangeDetection()
        {
            bool isInvoked = false;
            var entity = new object();
            var mock = new EntitySnapshotMock
            {
                HasChanges = o =>
                {
                    isInvoked = true;
                    return false;
                },
                SupportChangeDetection = false
            };
            StateManager.CreateSnapshot = o => mock;

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, false);
            viewModel.HasChanges.ShouldBeFalse();
            isInvoked.ShouldBeFalse();
        }

        [TestMethod]
        public void HasChangesShouldSetValue()
        {
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(entity, false);
            viewModel.HasChanges.ShouldBeFalse();
            viewModel.HasChanges = true;
            viewModel.HasChanges.ShouldBeTrue();
        }

        [TestMethod]
        public void EntityInitializedGenericShouldRaiseEvent()
        {
            bool isInvoked = false;
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.EntityInitialized += (model, o) =>
            {
                isInvoked = true;
                o.OriginalEntity.ShouldEqual(entity);
                o.Entity.ShouldEqual(entity);
                model.ShouldEqual(viewModel);
            };
            viewModel.InitializeEntity(entity, false);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void EntityInitializedNonGenericShouldRaiseEvent()
        {
            bool isInvoked = false;
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            IEditableViewModel viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.EntityInitialized += (model, o) =>
            {
                isInvoked = true;
                o.OriginalEntity.ShouldEqual(entity);
                o.Entity.ShouldEqual(entity);
                model.ShouldEqual(viewModel);
            };
            viewModel.InitializeEntity(entity, false);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ChangesAppliedGenericShouldRaiseEvent()
        {
            bool isInvoked = false;
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.ChangesApplied += (model, o) =>
            {
                isInvoked = true;
                o.Changes.Single().Entity.ShouldEqual(entity);
                model.ShouldEqual(viewModel);
            };
            viewModel.InitializeEntity(entity, false);
            viewModel.ApplyChanges();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ChangesCanceledGenericShouldRaiseEvent()
        {
            bool isInvoked = false;
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.ChangesCanceled += (model, o) =>
            {
                isInvoked = true;
                o.Entity.ShouldEqual(entity);
                model.ShouldEqual(viewModel);
            };
            viewModel.InitializeEntity(entity, false);
            viewModel.CancelChanges();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ChangesCanceledNonGenericShouldRaiseEvent()
        {
            bool isInvoked = false;
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            IEditableViewModel viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.ChangesCanceled += (model, o) =>
            {
                isInvoked = true;
                o.Entity.ShouldEqual(entity);
                model.ShouldEqual(viewModel);
            };
            viewModel.InitializeEntity(entity, false);
            viewModel.CancelChanges();
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void IsNewRecordShouldBeTrueIfIsNewEntity()
        {
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(new object(), true);
            viewModel.IsNewRecord.ShouldBeTrue();
        }

        [TestMethod]
        public void IsNewRecordShouldBeFalseIfIsNotNewEntity()
        {
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();
            var viewModel = GetViewModel<TestEditableViewModel>();
            viewModel.InitializeEntity(new object(), false);
            viewModel.IsNewRecord.ShouldBeFalse();
        }

        [TestMethod]
        public void VmShouldAddValidatorsForEntity()
        {
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();
            var viewModel = GetViewModel<TestEditableViewModel>();

            ValidatorProvider.Register(new SpyValidator() { CanValidate = context => context.Instance == entity });
            viewModel.InitializeEntity(entity, true);
            var validator =
                (SpyValidator)viewModel.GetValidators().Single(validator1 => validator1 != viewModel.Validator);
            validator.Context.Instance.ShouldEqual(entity);
        }

        [TestMethod]
        public void GetEntityStateManagerShouldReturnMainVmProvider()
        {
            bool isInvoked = false;
            var provider = new StateManagerMock
            {
                CreateSnapshot = o =>
                {
                    isInvoked = true;
                    return new EntitySnapshotMock();
                }
            };
            var entity = new object();
            var viewModel = GetViewModel<EditableViewModelMock>();
            viewModel.CustomStateManager = provider;
            viewModel.InitializeEntity(entity, true);
            isInvoked.ShouldBeTrue();
            viewModel.StateManager.ShouldEqual(provider);
        }

        [TestMethod]
        public void ChangesCanceledInternalShouldRaiseEvent()
        {
            bool isInvoked = false;
            var entity = new object();
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();

            var viewModel = GetViewModel<EditableViewModelMock>();
            viewModel.CancelChangesInternalDelegate = () => isInvoked = true;

            viewModel.InitializeEntity(entity, false);
            viewModel.CancelChanges();
            isInvoked.ShouldBeTrue();
            viewModel.IsChangesCanceledInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void GetChangesShouldBeUsedAsMainChanges()
        {
            bool isInvoked = false;
            bool isInvokedChangesApplied = false;
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();
            var entries = new IEntityStateEntry[0];
            var entity = new object();

            var viewModel = GetViewModel<EditableViewModelMock>();
            viewModel.GetChangesDelegate = o =>
            {
                isInvoked = true;
                o.ShouldEqual(entity);
                return entries;
            };
            viewModel.OnChangesAppliedDelegate = list =>
            {
                list.ShouldEqual(entries);
                isInvokedChangesApplied = true;
            };

            viewModel.InitializeEntity(entity, false);
            viewModel.ApplyChanges().ShouldEqual(entries);
            isInvoked.ShouldBeTrue();
            isInvokedChangesApplied.ShouldBeTrue();
        }

        [TestMethod]
        public void ApplyChangesInternalShouldBeUsedAsMainEntityAndChangesMethod()
        {
            bool isInvoked = false;
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();
            var entries = new IEntityStateEntry[0];
            var entity = new object();
            var viewModel = GetViewModel<EditableViewModelMock>();
            viewModel.ApplyChangesInternalDelegate = () =>
            {
                isInvoked = true;
                return new ApplyChangesClass
                {
                    Entity = entity,
                    EntityStateEntries = entries
                };
            };

            viewModel.InitializeEntity(new object(), true);
            viewModel.ApplyChanges().ShouldEqual(entries);
            viewModel.Entity.ShouldEqual(entity);
            isInvoked.ShouldBeTrue();
        }

        #endregion

        #region Overrides of TestBase

        protected override ValidatableViewModel GetValidatableViewModel()
        {
            StateManager.CreateSnapshot = o => new EntitySnapshotMock();
            var editableViewModel = GetViewModel<TestEditableViewModel>();
            editableViewModel.InitializeEntity(new object(), true);
            editableViewModel.RemoveInstance(editableViewModel.Entity);
            return editableViewModel;
        }

        protected override object GetFunc(Type type, string s, IIocParameter[] arg3)
        {
            if (type == typeof(IEntityStateManager))
                return StateManager;
            return base.GetFunc(type, s, arg3);
        }

        protected override void OnInit()
        {
            StateManager = new StateManagerMock();
            base.OnInit();
        }

        #endregion
    }
}