using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Entities.Components;
using MugenMvvm.Enums;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Entities.Components
{
    public class EntityStateTransitionManagerTest : UnitTestBase
    {
        [Theory]
        [MemberData(nameof(GetData))]
        public void ShouldUpdateState(EntityState from, EntityState to, EntityState result)
        {
            EntityStateTransitionManager.Priority.ShouldEqual(EntityComponentPriority.StateTransitionManager);
            EntityStateTransitionManager.Instance.OnEntityStateChanging(null!, this, from, to, Metadata).ShouldEqual(result);
        }

        public static IEnumerable<object?[]> GetData() =>
            new[]
            {
                new[] { EntityState.Added, EntityState.Deleted, EntityState.Detached },
                new[] { EntityState.Added, EntityState.Modified, EntityState.Added },
                new[] { EntityState.Deleted, EntityState.Added, EntityState.Modified },
                new[] { EntityState.Modified, EntityState.Added, EntityState.Added }
            };
    }
}