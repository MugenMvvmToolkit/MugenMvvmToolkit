namespace MugenMvvm.Ios.Interfaces
{
    public interface IContentTemplateSelector
    {
        object? SelectTemplate(object container, object? item);
    }
}