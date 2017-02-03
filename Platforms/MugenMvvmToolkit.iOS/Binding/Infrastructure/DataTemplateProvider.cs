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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public sealed class DataTemplateProvider<TItem> : IEventListener
        where TItem : class
    {
        #region Fields

        private readonly object _container;

        private readonly IBindingMemberInfo _templateSelectorMember;
        private TItem _templateSelector;

        #endregion

        #region Constructors

        public DataTemplateProvider([NotNull] object container, [NotNull] string templateSelectorMember)
        {
            Should.NotBeNull(container, nameof(container));
            Should.NotBeNull(templateSelectorMember, nameof(templateSelectorMember));
            var type = container.GetType();
            _container = container;
            _templateSelectorMember = BindingServiceProvider.MemberProvider.GetBindingMember(type, templateSelectorMember, false, false);
            _templateSelectorMember?.TryObserve(container, this);
            UpdateValues();
        }

        #endregion

        #region Properties

        public TItem TemplateSelector => _templateSelector;

        bool IEventListener.IsAlive => true;

        bool IEventListener.IsWeak => false;

        #endregion

        #region Methods

        private void UpdateValues()
        {
            if (_templateSelectorMember != null)
                _templateSelector = _templateSelectorMember.GetValue(_container, Empty.Array<object>()) as TItem;
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