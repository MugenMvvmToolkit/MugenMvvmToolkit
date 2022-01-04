﻿using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections;
using Xunit;

namespace MugenMvvm.UnitTests.Collections
{
    [Collection(SharedContext)]
    public class DecoratedReadOnlyObservableCollectionTest : ReadOnlyObservableCollectionTestBase
    {
        protected override IReadOnlyObservableCollection<T> GetCollection<T>(IReadOnlyObservableCollection<T> source, bool disposeSource, bool isWeak = true) =>
            new DecoratedReadOnlyObservableCollection<T>(source, 0, disposeSource, isWeak, false, ComponentCollectionManager);
    }
}