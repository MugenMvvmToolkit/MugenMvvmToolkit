namespace MugenMvvm.Android.Interfaces
{
    public interface IContentTemplateSelector
    {
        object? SelectTemplate(object container, object item);
    }
}