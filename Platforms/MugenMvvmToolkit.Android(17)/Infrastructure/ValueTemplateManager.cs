#region Copyright
// ****************************************************************************
// <copyright file="ValueTemplateManager.cs">
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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Infrastructure
{
    public sealed class ValueTemplateManager
    {
        #region Fields

        private readonly object _container;
        private readonly string _templateIdMember;
        private readonly string _templateSelectorMember;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueTemplateManager" /> class.
        /// </summary>
        public ValueTemplateManager([NotNull] object container, [NotNull] string templateIdMember,
            [NotNull] string templateSelectorMember)
        {
            Should.NotBeNull(container, "container");
            Should.NotBeNull(templateIdMember, "templateIdMember");
            Should.NotBeNull(templateSelectorMember, "templateSelectorMember");
            _container = container;
            _templateIdMember = templateIdMember;
            _templateSelectorMember = templateSelectorMember;
        }

        #endregion

        #region Methods

        public object SelectTemplate(object value)
        {
            return SelectTemplate(_container, value, _templateSelectorMember);
        }

        public bool TrySelectTemplate(object value, out object template)
        {
            return TrySelectTemplate(_container, value, _templateSelectorMember, out template);
        }

        public int? GetTemplateId()
        {
            return GetTemplateId(_container, _templateIdMember);
        }

        public static object SelectTemplate(object container, object value, string templateSelectorMember)
        {
            TrySelectTemplate(container, value, templateSelectorMember, out value);
            return value;
        }

        public static bool TrySelectTemplate(object container, object value, string templateSelectorMember,
            out object template)
        {
            template = null;
            var selector = GetDataTemplateSelector(container, templateSelectorMember);
            if (selector == null)
                return false;
            template = selector.SelectTemplate(value, container);
            return true;
        }

        public static IDataTemplateSelector GetDataTemplateSelector(object container, string templateSelectorMember)
        {
            IBindingMemberInfo member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(container.GetType(), templateSelectorMember, false, false);
            if (member == null)
                return null;
            return (IDataTemplateSelector)member.GetValue(container, null);
        }

        public static int? GetTemplateId(object container, string templateIdMember)
        {
            IBindingMemberInfo member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(container.GetType(), templateIdMember, false, false);
            if (member == null)
                return null;
            return (int?)member.GetValue(container, null);
        }

        #endregion
    }
}