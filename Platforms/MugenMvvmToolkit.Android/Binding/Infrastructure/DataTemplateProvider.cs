#region Copyright
// ****************************************************************************
// <copyright file="DataTemplateProvider.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public sealed class DataTemplateProvider
    {
        #region Fields

        private readonly object _container;
        private readonly IBindingMemberInfo _templateIdMember;
        private readonly IBindingMemberInfo _templateSelectorMember;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataTemplateProvider" /> class.
        /// </summary>
        public DataTemplateProvider([NotNull] object container, [NotNull] string templateIdMember,
            [NotNull] string templateSelectorMember)
        {
            Should.NotBeNull(container, "container");
            Should.NotBeNull(templateIdMember, "templateIdMember");
            Should.NotBeNull(templateSelectorMember, "templateSelectorMember");
            var type = container.GetType();
            _container = container;
            _templateIdMember = BindingServiceProvider.MemberProvider.GetBindingMember(type, templateIdMember, false, false);
            _templateSelectorMember = BindingServiceProvider.MemberProvider.GetBindingMember(type, templateSelectorMember, false, false);
        }

        #endregion

        #region Methods

        public bool TrySelectResourceTemplate(object value, out int templateId)
        {
            templateId = 0;
            var selector = GetDataTemplateSelector() as IResourceDataTemplateSelector;
            if (selector == null)
                return false;
            templateId = selector.SelectTemplate(value, _container);
            return true;
        }

        public bool TrySelectTemplate(object value, out object template)
        {
            template = null;
            var selector = GetDataTemplateSelector();
            if (selector == null)
                return false;
            template = selector.SelectTemplate(value, _container);
            return true;
        }

        public IDataTemplateSelector GetDataTemplateSelector()
        {
            return _templateSelectorMember.TryGetValue<IDataTemplateSelector>(_container);
        }

        public int? GetTemplateId()
        {
            return _templateIdMember.TryGetValue<int?>(_container);
        }

        #endregion
    }
}