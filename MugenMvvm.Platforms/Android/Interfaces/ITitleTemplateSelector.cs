using Java.Lang;

namespace MugenMvvm.Android.Interfaces
{
    public interface ITitleTemplateSelector
    {
        ICharSequence? TryGetTitle(object container, object? item);
    }
}