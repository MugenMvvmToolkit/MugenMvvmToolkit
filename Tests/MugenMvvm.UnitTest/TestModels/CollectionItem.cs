using System.Diagnostics;
using System.Threading;

namespace MugenMvvm.UnitTest.TestModels
{
    [DebuggerDisplay("Id = {Id}")]
    public class CollectionItem
    {
        #region Fields

        private static int _idGenerator = -1;

        #endregion

        #region Constructors

        public CollectionItem()
        {
            Id = Interlocked.Increment(ref _idGenerator);
        }

        #endregion

        #region Properties

        public bool Hidden { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Id: {Id}";
        }

        #endregion
    }
}