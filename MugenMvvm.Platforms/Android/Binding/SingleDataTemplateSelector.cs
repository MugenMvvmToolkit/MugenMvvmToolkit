using System.Collections.Generic;
using MugenMvvm.Android.Interfaces;

namespace MugenMvvm.Android.Binding
{
    public sealed class SingleDataTemplateSelector : IDataTemplateSelector
    {
        #region Fields

        private readonly int _templateId;
        private static readonly Dictionary<int, SingleDataTemplateSelector> Cache = new Dictionary<int, SingleDataTemplateSelector>();

        #endregion

        #region Constructors

        private SingleDataTemplateSelector(int templateId)
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

        public static SingleDataTemplateSelector Get(int templateId)
        {
            if (!Cache.TryGetValue(templateId, out var value))
            {
                value = new SingleDataTemplateSelector(templateId);
                Cache[templateId] = value;
            }

            return value;
        }

        #endregion
    }
}