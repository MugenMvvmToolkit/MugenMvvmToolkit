using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public abstract class MetadataOwnerTestBase : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void HasMetadataShouldReturnCorrectValue(bool emptyValue)
        {
            var context = emptyValue ? DefaultMetadata : new SingleValueMetadataContext(MetadataContextValue.Create(MetadataContextKey.FromKey<object>("test"), ""));
            var owner = GetMetadataOwner(context, null);
            owner.HasMetadata.ShouldEqual(!emptyValue);
        }

        [Fact]
        public virtual void MetadataShouldReturnInputValue()
        {
            var context = new MetadataContext();
            var owner = GetMetadataOwner(context, null);
            owner.Metadata.ShouldEqual(context);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void MetadataOwnerShouldUseMetadataContextProvider(bool globalValue)
        {
            IMetadataOwner<IMetadataContext>? owner = null;
            var context = new MetadataContext();
            var component = new TestMetadataContextProviderComponent
            {
                TryGetMetadataContext = (o, list) =>
                {
                    o.ShouldEqual(owner);
                    list.List.ShouldBeNull();
                    list.Item.IsEmpty.ShouldBeTrue();
                    return context;
                }
            };

            using var subscriber = globalValue ? TestComponentSubscriber.Subscribe(component) : default;
            if (globalValue)
                owner = GetMetadataOwner(null, null);
            else
            {
                var provider = new MetadataContextProvider();
                provider.AddComponent(component);
                owner = GetMetadataOwner(null, provider);
            }

            owner.Metadata.ShouldEqual(context);
        }

        protected abstract IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider);

        #endregion
    }
}