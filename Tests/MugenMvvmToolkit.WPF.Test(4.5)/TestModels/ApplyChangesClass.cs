using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ApplyChangesClass
    {
        public IList<IEntityStateEntry> EntityStateEntries { get; set; }

        public object Entity { get; set; }
    }
}