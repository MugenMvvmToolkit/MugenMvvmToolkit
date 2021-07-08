using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.ViewModels;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Internal
{
    public sealed class MugenServiceProviderTest : UnitTestBase
    {
        private readonly MugenServiceProvider _serviceProvider;
        private readonly ViewModelManager _viewModelManager;

        public MugenServiceProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _serviceProvider = new MugenServiceProvider(_viewModelManager);
        }

        [Fact]
        public void ShouldCreateClassNoParameters() => _serviceProvider.GetService(typeof(TestClassNoParameters)).ShouldBeType<TestClassNoParameters>();

        [Fact]
        public void ShouldCreateClassWithMetadata()
        {
            var manager = (TestClassMetadata)_serviceProvider.GetService(typeof(TestClassMetadata), DefaultMetadata)!;
            manager.Metadata.ShouldEqual(DefaultMetadata);
        }

        [Fact]
        public void ShouldCreateClassWithViewModelManager()
        {
            var manager = (TestClassViewModelManager)_serviceProvider.GetService(typeof(TestClassViewModelManager))!;
            manager.ViewModelManager.ShouldEqual(_viewModelManager);
        }

        [Fact]
        public void ShouldIgnoreNonEmptyConstructor() => _serviceProvider.GetService(typeof(string)).ShouldBeNull();

        [Fact]
        public void ShouldUseDelegateFromConstructor()
        {
            _serviceProvider.RegisterSingleton(typeof(MugenServiceProviderTest), this);
            _serviceProvider.GetService(typeof(MugenServiceProviderTest)).ShouldEqual(this);

            _serviceProvider.RegisterFactory(typeof(MugenServiceProviderTest), (t, m) =>
            {
                t.ShouldEqual(typeof(MugenServiceProviderTest));
                m.ShouldEqual(DefaultMetadata);
                return this;
            });
            _serviceProvider.GetService(typeof(MugenServiceProviderTest), DefaultMetadata).ShouldEqual(this);
        }

        public sealed class TestClassNoParameters
        {
        }

        public sealed class TestClassViewModelManager
        {
            public readonly IViewModelManager? ViewModelManager;

            public TestClassViewModelManager(IViewModelManager? viewModelManager = null)
            {
                ViewModelManager = viewModelManager;
            }
        }

        public sealed class TestClassMetadata
        {
            public readonly IReadOnlyMetadataContext? Metadata;

            public TestClassMetadata(IReadOnlyMetadataContext? metadata = null)
            {
                Metadata = metadata;
            }
        }
    }
}