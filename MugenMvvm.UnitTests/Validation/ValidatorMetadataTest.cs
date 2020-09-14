using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Metadata;
using MugenMvvm.Validation;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidatorMetadataTest : MetadataOwnerTestBase
    {
        #region Methods

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) => new Validator(metadata);

        #endregion
    }
}