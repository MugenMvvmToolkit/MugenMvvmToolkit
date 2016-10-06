#region Copyright

// ****************************************************************************
// <copyright file="DataTemplateProvider.cs">
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
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public sealed class DataTemplateProvider : IEventListener
    {
        #region Fields

        private readonly object _container;

        private readonly IBindingMemberInfo _templateMember;
        private readonly IBindingMemberInfo _templateSelectorMember;
        private IResourceDataTemplateSelector _resourceSelector;
        private int? _templateId;
        private IDataTemplateSelector _templateSelector;

        #endregion

        #region Constructors

        public DataTemplateProvider([NotNull] object container, [NotNull] string templateMember,
            [NotNull] string templateSelectorMember)
        {
            Should.NotBeNull(container, nameof(container));
            Should.NotBeNull(templateMember, nameof(templateMember));
            Should.NotBeNull(templateSelectorMember, nameof(templateSelectorMember));
            var type = container.GetType();
            _container = container;
            _templateMember = BindingServiceProvider.MemberProvider.GetBindingMember(type, templateMember, false, false);
            _templateSelectorMember = BindingServiceProvider.MemberProvider.GetBindingMember(type, templateSelectorMember, false, false);
            _templateMember?.TryObserve(container, this);
            _templateSelectorMember?.TryObserve(container, this);
            UpdateValues();
        }

        #endregion

        #region Properties

        bool IEventListener.IsAlive => true;

        bool IEventListener.IsWeak => false;

        #endregion

        #region Methods

        public bool TrySelectResourceTemplate(object value, out int templateId)
        {
            templateId = 0;
            if (_resourceSelector == null)
                return false;
            templateId = _resourceSelector.SelectTemplate(value, _container);
            return true;
        }

        public bool TrySelectTemplate(object value, out object template)
        {
            template = null;
            if (_templateSelector == null)
                return false;
            template = _templateSelector.SelectTemplate(value, _container);
            return true;
        }

        public IDataTemplateSelector GetDataTemplateSelector()
        {
            return _templateSelector;
        }

        public int? GetTemplateId()
        {
            return _templateId;
        }

        private void UpdateValues()
        {
            if (_templateSelectorMember != null)
            {
                var value = _templateSelectorMember.GetValue(_container, Empty.Array<object>());
                _resourceSelector = value as IResourceDataTemplateSelector;
                _templateSelector = value as IDataTemplateSelector;
            }
            if (_templateMember != null)
                _templateId = _templateMember.GetValue(_container, Empty.Array<object>()) as int?;
        }

        #endregion

        #region Implementation of interfaces

        bool IEventListener.TryHandle(object sender, object message)
        {
            UpdateValues();
            return true;
        }

        #endregion
    }
}