#region Copyright

// ****************************************************************************
// <copyright file="EntityStateManagerTest.cs">
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
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    [TestClass]
    public class EntityStateManagerTest : TestBase
    {
        #region Fields

        private static readonly Guid GuidValue = Guid.NewGuid();
        private const int IntValue = 100;
        private const string StringValue = "test";

        #endregion

        #region Nested types

        public sealed class EntityStateModel
        {
            public string String { get; set; }

            public Guid Guid { get; set; }

            public int Int { get; set; }
        }

        #endregion

        #region Methods

        [TestMethod]
        public void ProviderShouldSupportChangeDetection()
        {
            var provider = GetStateManager();
            provider.CreateSnapshot(GetModel()).SupportChangeDetection.ShouldBeTrue();
        }

        [TestMethod]
        public void ProviderShouldSaveAndRestoreState()
        {
            IEntityStateManager manager = GetStateManager();
            var stateModel = GetModel();
            var entitySnapshot = manager.CreateSnapshot(stateModel);

            stateModel.Int = int.MaxValue;
            stateModel.Int.ShouldEqual(int.MaxValue);
            stateModel.String = null;
            stateModel.String.ShouldBeNull();
            stateModel.Guid = Guid.Empty;
            stateModel.Guid.ShouldEqual(Guid.Empty);

            entitySnapshot.Restore(stateModel);
            AssertModel(stateModel);
        }

        [TestMethod]
        public void ProviderShouldSaveAndApplyState()
        {
            IEntityStateManager manager = GetStateManager();
            var stateModel = GetModel();
            manager.CreateSnapshot(stateModel);

            stateModel.Int = int.MaxValue;
            stateModel.String = null;
            stateModel.Guid = Guid.Empty;

            stateModel.Guid.ShouldEqual(Guid.Empty);
            stateModel.String.ShouldBeNull();
            stateModel.Int.ShouldEqual(int.MaxValue);
        }

        [TestMethod]
        public void ProviderShouldTrackPropertyChanges()
        {
            IEntityStateManager manager = GetStateManager();
            var stateModel = GetModel();
            var snapshot = manager.CreateSnapshot(stateModel);

            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.Guid)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.String)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.Int)).ShouldBeFalse();

            stateModel.Int = int.MaxValue;
            stateModel.String = null;
            stateModel.Guid = Guid.Empty;

            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.Guid)).ShouldBeTrue();
            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.String)).ShouldBeTrue();
            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.Int)).ShouldBeTrue();

            stateModel.Int = IntValue;
            stateModel.String = StringValue;
            stateModel.Guid = GuidValue;

            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.Guid)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.String)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, ToolkitExtensions.GetMemberName<EntityStateModel>(() => model => model.Int)).ShouldBeFalse();
        }

        [TestMethod]
        public void ProviderShouldTrackObjectChanges()
        {
            IEntityStateManager manager = GetStateManager();
            var stateModel = GetModel();
            var snapshot = manager.CreateSnapshot(stateModel);

            snapshot.HasChanges(stateModel).ShouldBeFalse();

            stateModel.Int = int.MaxValue;
            snapshot.HasChanges(stateModel).ShouldBeTrue();
            stateModel.Int = IntValue;
            snapshot.HasChanges(stateModel).ShouldBeFalse();

            stateModel.String = null;
            snapshot.HasChanges(stateModel).ShouldBeTrue();
            stateModel.String = StringValue;
            snapshot.HasChanges(stateModel).ShouldBeFalse();

            stateModel.Guid = Guid.Empty;
            snapshot.HasChanges(stateModel).ShouldBeTrue();
            stateModel.Guid = GuidValue;
            snapshot.HasChanges(stateModel).ShouldBeFalse();
        }

        protected virtual IEntityStateManager GetStateManager()
        {
            return new EntityStateManager();
        }

        private static EntityStateModel GetModel()
        {
            return new EntityStateModel { Guid = GuidValue, Int = IntValue, String = StringValue };
        }

        private static void AssertModel(EntityStateModel model)
        {
            model.Guid.ShouldEqual(GuidValue);
            model.Int.ShouldEqual(IntValue);
            model.String.ShouldEqual(StringValue);
        }

        #endregion
    }
}
