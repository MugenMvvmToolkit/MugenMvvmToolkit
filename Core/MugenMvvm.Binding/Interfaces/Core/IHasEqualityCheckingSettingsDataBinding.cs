namespace MugenMvvm.Binding.Interfaces.Core
{
    public interface IHasEqualityCheckingSettingsDataBinding : IDataBinding//todo move to metadata
    {
        bool DisableEqualityCheckingTarget { get; set; }

        bool DisableEqualityCheckingSource { get; set; }
    }
}