namespace MugenMvvm.Android.Interfaces
{
    public interface IResourceTemplateSelector
    {
        int TemplateTypeCount { get; }

        int SelectTemplate(object container, object? item);
    }
}