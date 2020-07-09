using MugenMvvm.Android.Interfaces;
using MugenMvvm.Collections;

namespace MugenMvvm.Android.Binding
{
    public sealed class SingleDataTemplateSelector : IDataTemplateSelector
    {
        #region Fields

        private readonly int _templateId;
        private static readonly TemplateDictionary Cache = new TemplateDictionary();

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

        #region Nested types

        private sealed class TemplateDictionary : LightDictionary<int, SingleDataTemplateSelector>
        {
            #region Constructors

            public TemplateDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(int key) => key;

            protected override bool Equals(int x, int y) => x == y;

            #endregion
        }

        #endregion
    }
}