namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IHasEqualityCheckingSettingsDataBinding : IDataBinding
    {
        bool DisableEqualityCheckingTarget { get; set; }

        bool DisableEqualityCheckingSource { get; set; }
    }
}