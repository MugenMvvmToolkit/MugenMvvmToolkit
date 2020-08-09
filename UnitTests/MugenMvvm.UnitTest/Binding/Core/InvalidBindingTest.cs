using System;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core
{
    public class InvalidBindingTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void StateShouldBeInvalid()
        {
            var ex = new Exception();
            var invalidBinding = new InvalidBinding(ex);
            invalidBinding.State.ShouldEqual(BindingState.Invalid);
            invalidBinding.Exception.ShouldEqual(ex);
        }

        [Fact]
        public void BuildShouldReturnSelf()
        {
            var invalidBinding = new InvalidBinding(new Exception());
            ((IBindingBuilder) invalidBinding).Build(this, null, DefaultMetadata).ShouldEqual(invalidBinding);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateTargetShouldNotifyListeners(int count)
        {
            var updateFailed = 0;
            var exception = new Exception();
            var binding = new InvalidBinding(exception);
            for (var i = 0; i < count; i++)
            {
                binding.AddComponent(new TestBindingTargetListener
                {
                    OnTargetUpdateFailed = (b, e, m) =>
                    {
                        ++updateFailed;
                        b.ShouldEqual(binding);
                        e.ShouldEqual(exception);
                        m.ShouldEqual(binding);
                    },
                    OnTargetUpdateCanceled = (b, m) => throw new NotSupportedException(),
                    OnTargetUpdated = (b, v, m) => throw new NotSupportedException()
                });
            }

            binding.UpdateTarget();
            updateFailed.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateSourceShouldNotifyListeners(int count)
        {
            var updateFailed = 0;
            var exception = new Exception();
            var binding = new InvalidBinding(exception);
            for (var i = 0; i < count; i++)
            {
                binding.AddComponent(new TestBindingSourceListener
                {
                    OnSourceUpdateFailed = (b, e, m) =>
                    {
                        ++updateFailed;
                        b.ShouldEqual(binding);
                        e.ShouldEqual(exception);
                        m.ShouldEqual(binding);
                    },
                    OnSourceUpdateCanceled = (b, m) => throw new NotSupportedException(),
                    OnSourceUpdated = (b, v, m) => throw new NotSupportedException()
                });
            }

            binding.UpdateSource();
            updateFailed.ShouldEqual(count);
        }

        #endregion
    }
}