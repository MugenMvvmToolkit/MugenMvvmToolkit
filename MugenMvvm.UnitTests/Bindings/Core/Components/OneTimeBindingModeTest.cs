﻿using System;
using System.Linq;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class OneTimeBindingModeTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnAttachingShouldUpdateBindingIfAvailable(bool dispose)
        {
            var disposeCount = 0;
            var updateCount = 0;
            var binding = new TestBinding
            {
                UpdateSource = () => throw new NotSupportedException(),
                UpdateTarget = () => ++updateCount,
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                },
                Source = new[]
                {
                    new TestMemberPathObserver
                    {
                        GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                    },
                    new TestMemberPathObserver
                    {
                        GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                    }
                },
                Dispose = () => ++disposeCount
            };
            var mode = dispose ? OneTimeBindingMode.Instance : OneTimeBindingMode.NonDisposeInstance;
            ((IAttachableComponent) mode).OnAttaching(binding, DefaultMetadata).ShouldBeFalse();
            disposeCount.ShouldEqual(dispose ? 1 : 0);
            updateCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void OnAttachingShouldUpdateBindingIfNotAvailable(bool dispose, bool lastMemberChanged)
        {
            var disposeCount = 0;
            var updateCount = 0;
            var isAvailableTarget = false;
            var isAvailableSource = false;
            var binding = new TestBinding
            {
                UpdateSource = () => throw new NotSupportedException(),
                UpdateTarget = () => ++updateCount,
                Target = new TestMemberPathObserver
                {
                    GetLastMember = metadata =>
                    {
                        if (isAvailableTarget)
                            return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                        return default;
                    }
                },
                Source = new[]
                {
                    new TestMemberPathObserver
                    {
                        GetLastMember = metadata =>
                        {
                            if (isAvailableSource)
                                return new MemberPathLastMember(this, ConstantMemberInfo.Target);
                            return default;
                        }
                    },
                    new TestMemberPathObserver
                    {
                        GetLastMember = metadata => new MemberPathLastMember(this, ConstantMemberInfo.Target)
                    }
                },
                Dispose = () => ++disposeCount
            };
            var mode = dispose ? OneTimeBindingMode.Instance : OneTimeBindingMode.NonDisposeInstance;
            var listener = (IBindingSourceObserverListener) mode;
            binding.AddComponent(mode);
            binding.GetComponents<object>().Single().ShouldEqual(mode);

            isAvailableSource = true;
            if (lastMemberChanged)
                listener.OnSourceLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                listener.OnSourcePathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            disposeCount.ShouldEqual(0);
            updateCount.ShouldEqual(0);

            isAvailableTarget = true;
            listener.OnSourceError(binding, EmptyPathObserver.Empty, new Exception(), DefaultMetadata);
            disposeCount.ShouldEqual(0);
            updateCount.ShouldEqual(0);

            if (lastMemberChanged)
                listener.OnSourceLastMemberChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            else
                listener.OnSourcePathMembersChanged(binding, EmptyPathObserver.Empty, DefaultMetadata);
            disposeCount.ShouldEqual(dispose ? 1 : 0);
            updateCount.ShouldEqual(1);
        }

        #endregion
    }
}