#region Copyright

// ****************************************************************************
// <copyright file="DataTemplateSelectorBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public abstract class DataTemplateSelectorBase<TSource, TTemplate> : IDataTemplateSelector where TTemplate : class
    {
        #region Fields

        private BindingSet<TTemplate, TSource> _bindingSet;

        #endregion

        #region Implementation of IDataTemplateSelector

        object IDataTemplateSelector.SelectTemplate(object item, object container)
        {
            TTemplate template = SelectTemplate((TSource)item, container);
            if (template != null && CanInitialize(template, container))
            {
                if (_bindingSet == null)
                    _bindingSet = new BindingSet<TTemplate, TSource>(null);
                try
                {
                    _bindingSet.Target = template;
                    Initialize(template, _bindingSet);
                    _bindingSet.Apply();
                }
                finally
                {
                    _bindingSet.Target = null;
                }
            }
            return template;
        }

        #endregion

        #region Methods

        protected abstract TTemplate SelectTemplate(TSource item, object container);

        protected abstract void Initialize(TTemplate template, BindingSet<TTemplate, TSource> bindingSet);

        protected virtual bool CanInitialize([NotNull] TTemplate template, [NotNull] object container)
        {
            return true;
        }

        #endregion
    }
}
