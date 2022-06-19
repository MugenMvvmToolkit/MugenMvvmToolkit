namespace MugenMvvm.Android.Interfaces
{
    public interface IResourceTemplateSelector//todo getcontext
    {
        public const int NoResult = int.MinValue;

        int TemplateTypeCount { get; }

        int TrySelectTemplate(object container, object? item);
    }
}