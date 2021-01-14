using System.Collections.Generic;
using MugenMvvm.Android.Interfaces;

namespace MugenMvvm.Android.Bindings
{
    public sealed class SingleResourceTemplateSelector : IResourceTemplateSelector
    {
        private static readonly Dictionary<int, SingleResourceTemplateSelector> Cache = new();

        private readonly int _templateId;

        private SingleResourceTemplateSelector(int templateId)
        {
            _templateId = templateId;
        }

        public int TemplateTypeCount => 1;

        public static SingleResourceTemplateSelector Get(int templateId)
        {
            if (!Cache.TryGetValue(templateId, out var value))
            {
                value = new SingleResourceTemplateSelector(templateId);
                Cache[templateId] = value;
            }

            return value;
        }

        public int SelectTemplate(object container, object? item) => _templateId;
    }
}