#if XAMARIN_IOS
// ReSharper disable once CheckNamespace
namespace MugenMvvm.Ios.Interfaces
#else
// ReSharper disable once CheckNamespace
namespace MugenMvvm.Android.Interfaces
#endif
{
    public interface IContentTemplateSelector
    {
        object? SelectTemplate(object container, object? item);
    }
}