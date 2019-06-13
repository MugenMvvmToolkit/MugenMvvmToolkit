namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IBindingPath
    {
        string Path { get; }

        string[] Parts { get; }
    }
}