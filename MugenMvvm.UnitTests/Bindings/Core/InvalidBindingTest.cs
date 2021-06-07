using System;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Core;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class InvalidBindingTest : UnitTestBase
    {
        private readonly InvalidBinding _binding;
        private readonly Exception _exception;

        public InvalidBindingTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _exception = new Exception();
            _binding = new InvalidBinding(_exception);
        }

        [Fact]
        public void BuildShouldReturnSelf() => ((IBindingBuilder)_binding).Build(this, null, DefaultMetadata).ShouldEqual(_binding);

        [Fact]
        public void StateShouldBeInvalid()
        {
            _binding.State.ShouldEqual(BindingState.Invalid);
            _binding.Exception.ShouldEqual(_exception);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateTargetShouldNotifyListeners(int count)
        {
            var updateFailed = 0;
            for (var i = 0; i < count; i++)
            {
                _binding.AddComponent(new TestBindingTargetListener
                {
                    OnTargetUpdateFailed = (b, e, m) =>
                    {
                        ++updateFailed;
                        b.ShouldEqual(_binding);
                        e.ShouldEqual(_exception);
                        m.ShouldEqual(_binding);
                    },
                    OnTargetUpdateCanceled = (b, m) => throw new NotSupportedException(),
                    OnTargetUpdated = (b, v, m) => throw new NotSupportedException()
                });
            }

            _binding.UpdateTarget();
            updateFailed.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void UpdateSourceShouldNotifyListeners(int count)
        {
            var updateFailed = 0;
            for (var i = 0; i < count; i++)
            {
                _binding.AddComponent(new TestBindingSourceListener
                {
                    OnSourceUpdateFailed = (b, e, m) =>
                    {
                        ++updateFailed;
                        b.ShouldEqual(_binding);
                        e.ShouldEqual(_exception);
                        m.ShouldEqual(_binding);
                    },
                    OnSourceUpdateCanceled = (b, m) => throw new NotSupportedException(),
                    OnSourceUpdated = (b, v, m) => throw new NotSupportedException()
                });
            }

            _binding.UpdateSource();
            updateFailed.ShouldEqual(count);
        }
    }
}