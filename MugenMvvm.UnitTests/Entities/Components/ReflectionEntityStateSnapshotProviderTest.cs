using System;
using System.Linq;
using System.Reflection;
using MugenMvvm.Entities;
using MugenMvvm.Entities.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Entities;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Entities.Components
{
    public class ReflectionEntityStateSnapshotProviderTest : UnitTestBase
    {
        private const int IntValue = 100;
        private const string StringValue = "test";
        private static readonly Guid GuidValue = Guid.NewGuid();

        private readonly ReflectionEntityStateSnapshotProvider _entityStateSnapshotProvider;

        public ReflectionEntityStateSnapshotProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _entityStateSnapshotProvider = new ReflectionEntityStateSnapshotProvider(ReflectionManager);
            EntityManager.AddComponent(_entityStateSnapshotProvider);
        }

        [Fact]
        public void ShouldDumpValues()
        {
            var stateModel = GetModel();
            var snapshot = EntityManager.TryGetSnapshot(stateModel, DefaultMetadata)!;

            var values = snapshot.Dump(stateModel, DefaultMetadata);
            values.Count.ShouldEqual(3);
            var v = values.Single(value => ((PropertyInfo)value.Member!).Name == nameof(stateModel.Guid));
            v.OldValue.ShouldEqual(GuidValue);
            v.NewValue.ShouldEqual(GuidValue);

            v = values.Single(value => ((PropertyInfo)value.Member!).Name == nameof(stateModel.Int));
            v.OldValue.ShouldEqual(IntValue);
            v.NewValue.ShouldEqual(IntValue);

            v = values.Single(value => ((PropertyInfo)value.Member!).Name == nameof(stateModel.String));
            v.OldValue.ShouldEqual(StringValue);
            v.NewValue.ShouldEqual(StringValue);

            stateModel.Int = int.MaxValue;
            stateModel.String = null;
            stateModel.Guid = Guid.Empty;

            values = snapshot.Dump(stateModel, DefaultMetadata);
            values.Count.ShouldEqual(3);
            v = values.Single(value => ((PropertyInfo)value.Member!).Name == nameof(stateModel.Guid));
            v.OldValue.ShouldEqual(GuidValue);
            v.NewValue.ShouldEqual(Guid.Empty);

            v = values.Single(value => ((PropertyInfo)value.Member!).Name == nameof(stateModel.Int));
            v.OldValue.ShouldEqual(IntValue);
            v.NewValue.ShouldEqual(int.MaxValue);

            v = values.Single(value => ((PropertyInfo)value.Member!).Name == nameof(stateModel.String));
            v.OldValue.ShouldEqual(StringValue);
            v.NewValue.ShouldEqual(null);
        }

        [Fact]
        public void ShouldSaveAndRestoreState()
        {
            var stateModel = GetModel();
            var entitySnapshot = EntityManager.TryGetSnapshot(stateModel, DefaultMetadata)!;

            stateModel.Int = int.MaxValue;
            stateModel.Int.ShouldEqual(int.MaxValue);
            stateModel.String = null;
            stateModel.String.ShouldBeNull();
            stateModel.Guid = Guid.Empty;
            stateModel.Guid.ShouldEqual(Guid.Empty);

            entitySnapshot.Restore(stateModel);
            AssertModel(stateModel);
        }

        [Fact]
        public void ShouldTrackEntityChanges()
        {
            var stateModel = GetModel();
            var snapshot = EntityManager.TryGetSnapshot(stateModel, DefaultMetadata)!;

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

        [Fact]
        public void ShouldTrackPropertyChanges()
        {
            var stateModel = GetModel();
            var snapshot = EntityManager.TryGetSnapshot(stateModel, DefaultMetadata)!;

            snapshot.HasChanges(stateModel, nameof(stateModel.Guid)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, nameof(stateModel.String)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, nameof(stateModel.Int)).ShouldBeFalse();

            stateModel.Int = int.MaxValue;
            snapshot.HasChanges(stateModel, nameof(stateModel.Guid)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, nameof(stateModel.String)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, nameof(stateModel.Int)).ShouldBeTrue();

            stateModel.String = null;
            snapshot.HasChanges(stateModel, nameof(stateModel.Guid)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, nameof(stateModel.String)).ShouldBeTrue();
            snapshot.HasChanges(stateModel, nameof(stateModel.Int)).ShouldBeTrue();

            stateModel.Guid = Guid.Empty;
            snapshot.HasChanges(stateModel, nameof(stateModel.Guid)).ShouldBeTrue();
            snapshot.HasChanges(stateModel, nameof(stateModel.String)).ShouldBeTrue();
            snapshot.HasChanges(stateModel, nameof(stateModel.Int)).ShouldBeTrue();

            stateModel.Int = IntValue;
            stateModel.String = StringValue;
            stateModel.Guid = GuidValue;

            snapshot.HasChanges(stateModel, nameof(stateModel.Guid)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, nameof(stateModel.String)).ShouldBeFalse();
            snapshot.HasChanges(stateModel, nameof(stateModel.Int)).ShouldBeFalse();
        }

        [Fact]
        public void ShouldUseFilter()
        {
            var stateModel = GetModel();
            _entityStateSnapshotProvider.MemberFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            _entityStateSnapshotProvider.MemberFilter = info => info.Name == nameof(stateModel.Guid);
            var snapshot = EntityManager.TryGetSnapshot(stateModel, DefaultMetadata)!;

            var values = snapshot.Dump(stateModel, DefaultMetadata);
            values.Count.ShouldEqual(1);
            var v = values.Single(value => ((PropertyInfo)value.Member!).Name == nameof(stateModel.Guid));
            v.OldValue.ShouldEqual(GuidValue);
            v.NewValue.ShouldEqual(GuidValue);
        }

        protected override IEntityManager GetEntityManager() => new EntityManager(ComponentCollectionManager);

        private static EntityStateModel GetModel() => new() { Guid = GuidValue, Int = IntValue, String = StringValue };

        private static void AssertModel(EntityStateModel model)
        {
            model.Guid.ShouldEqual(GuidValue);
            model.Int.ShouldEqual(IntValue);
            model.String.ShouldEqual(StringValue);
        }

        private sealed class EntityStateModel
        {
            public string? String { get; set; }

            public Guid Guid { get; set; }

            public int Int { get; set; }
        }
    }
}