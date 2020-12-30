using System.Collections.Generic;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation.Paths
{
    public sealed class EmptyMemberPath : IMemberPath, IValueHolder<string>
    {
        #region Fields

        public static readonly EmptyMemberPath Instance = new();

        #endregion

        #region Constructors

        private EmptyMemberPath()
        {
        }

        #endregion

        #region Properties

        public string Path => "";

        public IReadOnlyList<string> Members => Default.Array<string>();

        string? IValueHolder<string>.Value { get; set; }

        #endregion
    }
}