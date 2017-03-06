#region Copyright

// ****************************************************************************
// <copyright file="ResourceDataTemplateSelectorBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public abstract class ResourceDataTemplateSelectorBase<TSource> : IResourceDataTemplateSelector
    {
        #region Implementation of IDataTemplateSelector

        public abstract int TemplateTypeCount { get; }

        int IResourceDataTemplateSelector.SelectTemplate(object item, object container)
        {
            return SelectTemplate((TSource)item, container);
        }

        object IDataTemplateSelector.SelectTemplate(object item, object container)
        {
            return SelectTemplate((TSource)item, container);
        }

        #endregion

        #region Methods

        protected abstract int SelectTemplate(TSource item, object container);

        #endregion
    }
}
