using Java.Lang;

namespace MugenMvvm.Android.Interfaces
{
    public interface ITitleTemplateSelector
    {
        ICharSequence? GetTitle(object container, object? item);
    }
}