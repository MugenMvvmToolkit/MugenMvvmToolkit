﻿using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestTargetValueSetterComponent : ITargetValueSetterComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public TrySetTargetValueDelegate? TrySetTargetValue { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ITargetValueSetterComponent.TrySetTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata) =>
            TrySetTargetValue?.Invoke(binding, targetMember, value, metadata) ?? false;

        #endregion

        #region Nested types

        public delegate bool TrySetTargetValueDelegate(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);

        #endregion
    }
}