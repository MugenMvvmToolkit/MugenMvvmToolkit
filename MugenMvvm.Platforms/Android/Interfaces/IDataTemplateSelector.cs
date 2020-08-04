namespace MugenMvvm.Android.Interfaces
{
    public interface IDataTemplateSelector//todo add fragment support
    {
        int TemplateTypeCount { get; }

        int SelectTemplate(object container, object? item);
    }
}