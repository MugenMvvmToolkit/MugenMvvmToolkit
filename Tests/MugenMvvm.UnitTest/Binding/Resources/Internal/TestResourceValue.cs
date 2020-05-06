using MugenMvvm.Binding.Interfaces.Resources;

namespace MugenMvvm.UnitTest.Binding.Resources.Internal
{
    public class TestResourceValue : IResourceValue
    {
        #region Properties

        public bool IsAlive { get; set; } = true;

        public bool IsStatic { get; set; }

        public object? Value { get; set; }

        #endregion
    }
}