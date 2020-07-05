namespace MugenMvvm.Android.Interfaces
{
    public interface IDataTemplateSelector
    {
        int TemplateTypeCount { get; }

        int SelectTemplate(object? item, object container);
    }
}