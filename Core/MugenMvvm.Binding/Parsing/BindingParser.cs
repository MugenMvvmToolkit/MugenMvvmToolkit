using System;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing
{
    public class BindingParser : ComponentOwnerBase<IBindingParser>, IBindingParser
    {
        #region Constructors

        public BindingParser(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IBindingParserResult[] Parse(string expression, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}