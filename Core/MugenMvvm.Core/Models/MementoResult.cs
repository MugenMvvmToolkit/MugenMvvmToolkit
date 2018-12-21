using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Serialization;

namespace MugenMvvm.Models
{
    public class MementoResult : IMementoResult
    {
        #region Fields

        public static readonly IMementoResult Unrestored;

        #endregion

        #region Constructors

        static MementoResult()
        {
            Unrestored = new MementoResult();
        }

        private MementoResult()
        {
            Context = Default.Context;
        }

        public MementoResult(object target, IReadOnlyContext? context = null)
        {
            IsRestored = true;
            Context = context ?? Default.Context;
            Target = target;
        }

        #endregion

        #region Properties

        public bool IsRestored { get; }

        public IReadOnlyContext Context { get; }

        public object? Target { get; }

        #endregion
    }
}