namespace MugenMvvm.Interfaces.Models
{
    public interface IHasService<out T> where T : class
    {
        T? GetService(bool optional);
    }
}