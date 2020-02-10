using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Serialization;
using MugenMvvm.UnitTest.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Serialization
{
    public class SerializationContextTest : MetadataOwnerTestBase
    {
        #region Methods

        [Fact]
        public void BeginShouldThrowDoubleInitialization()
        {
            var ctx = new SerializationContext();
            using var t = SerializationContext.Begin(ctx);
            ShouldThrow<InvalidOperationException>(() => SerializationContext.Begin(ctx));
        }

        [Fact]
        public void BeginShouldInitializeContextForThread()
        {
            var ctx = new SerializationContext();
            SerializationContext.Current.ShouldBeNull();
            var actionToken = SerializationContext.Begin(ctx);
            SerializationContext.Current.ShouldEqual(ctx);

            Task.Run(() => { SerializationContext.Current.ShouldBeNull(); }).Wait();

            actionToken.Dispose();
            SerializationContext.Current.ShouldBeNull();
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new SerializationContext(metadata, metadataContextProvider);
        }

        #endregion
    }
}