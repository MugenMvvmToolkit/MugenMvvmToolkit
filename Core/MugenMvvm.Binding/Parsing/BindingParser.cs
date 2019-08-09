using System;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Parsing
{
    public class BindingParser : IBindingParser
    {
        #region Implementation of interfaces

        int IComponent.GetPriority(object source)
        {
            return int.MaxValue;
        }

        public IBindingParserResult[] Parse(string expression, IReadOnlyMetadataContext? metadata)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}