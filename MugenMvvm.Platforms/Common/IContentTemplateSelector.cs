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
        bool TrySelectTemplate(object container, object? item, out object? template);
    }
}