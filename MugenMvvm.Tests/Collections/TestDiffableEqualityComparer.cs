using System;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Tests.Collections
{
    public class TestDiffableEqualityComparer : IContentDiffableEqualityComparer
    {
        public Func<object?, object?, bool>? AreItemsTheSame { get; set; }

        public Func<object?, object?, bool>? AreContentsTheSame { get; set; }

        bool IContentDiffableEqualityComparer.AreContentsTheSame(object? x, object? y) => AreContentsTheSame?.Invoke(x, y) ?? true;

        bool IDiffableEqualityComparer.AreItemsTheSame(object? x, object? y) => AreItemsTheSame?.Invoke(x, y) ?? Equals(x, y);
    }
}