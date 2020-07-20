﻿using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.Validation;

namespace MugenMvvm.UnitTest.Validation
{
    public class ValidatorMetadataTest : MetadataOwnerTestBase
    {
        #region Methods

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata)
        {
            return new Validator(metadata);
        }

        #endregion
    }
}