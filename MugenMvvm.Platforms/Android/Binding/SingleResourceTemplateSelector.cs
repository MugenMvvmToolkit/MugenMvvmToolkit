using System.Collections.Generic;
using MugenMvvm.Android.Interfaces;

namespace MugenMvvm.Android.Binding
{
    public sealed class SingleResourceTemplateSelector : IResourceTemplateSelector
    {
        #region Fields

        private readonly int _templateId;
        private static readonly Dictionary<int, SingleResourceTemplateSelector> Cache = new Dictionary<int, SingleResourceTemplateSelector>();

        #endregion

        #region Constructors

        private SingleResourceTemplateSelector(int templateId)
        {
            _templateId = templateId;
        }

        #endregion

        #region Properties

        public int TemplateTypeCount => 1;

        #endregion

        #region Implementation of interfaces

        public int SelectTemplate(object container, object? item) => _templateId;

        #endregion

        #region Methods

        public static SingleResourceTemplateSelector Get(int templateId)
        {
            if (!Cache.TryGetValue(templateId, out var value))
            {
                value = new SingleResourceTemplateSelector(templateId);
                Cache[templateId] = value;
            }

            return value;
        }

        #endregion
    }
}