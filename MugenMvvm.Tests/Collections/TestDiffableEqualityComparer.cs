using System;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Tests.Collections
{
    public class TestDiffableEqualityComparer : IDiffableEqualityComparer
    {
        public Func<object?, object?, bool>? AreItemsTheSame { get; set; }

        bool IDiffableEqualityComparer.AreItemsTheSame(object? x, object? y) => AreItemsTheSame?.Invoke(x, y) ?? Equals(x, y);
    }
}