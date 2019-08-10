namespace MugenMvvm.Interfaces.Components
{
    public interface IHasComponentPriority
    {
        int GetPriority(object owner);
    }
}