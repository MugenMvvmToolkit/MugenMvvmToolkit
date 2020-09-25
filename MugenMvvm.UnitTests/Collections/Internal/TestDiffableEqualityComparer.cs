using System;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.UnitTests.Collections.Internal
{
    public class TestDiffableEqualityComparer : IContentDiffableEqualityComparer
    {
        #region Properties

        public Func<object?, object?, bool>? AreItemsTheSame { get; set; }

        public Func<object?, object?, bool>? AreContentsTheSame { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IDiffableEqualityComparer.AreItemsTheSame(object? x, object? y) => AreItemsTheSame?.Invoke(x, y) ?? Equals(x, y);

        bool IContentDiffableEqualityComparer.AreContentsTheSame(object? x, object? y) => AreContentsTheSame?.Invoke(x, y) ?? true;

        #endregion
    }
}