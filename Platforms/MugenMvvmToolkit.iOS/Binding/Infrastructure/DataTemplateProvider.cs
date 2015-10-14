#region Copyright

// ****************************************************************************
// <copyright file="DataTemplateProvider.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.iOS.Binding.Interfaces;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public sealed class DataTemplateProvider : IEventListener
    {
        #region Fields

        private readonly object _container;

        private readonly IBindingMemberInfo _templateSelectorMember;
        private ICollectionCellTemplateSelector _collectionCellTemplateSelector;
        private ITableCellTemplateSelector _tableCellTemplateSelector;

        #endregion

        #region Constructors

        public DataTemplateProvider([NotNull] object container, [NotNull] string templateSelectorMember)
        {
            Should.NotBeNull(container, "container");
            Should.NotBeNull(templateSelectorMember, "templateSelectorMember");
            var type = container.GetType();
            _container = container;
            _templateSelectorMember = BindingServiceProvider.MemberProvider.GetBindingMember(type, templateSelectorMember, false, false);
            if (_templateSelectorMember != null)
                _templateSelectorMember.TryObserve(container, this);
            UpdateValues();
        }

        #endregion

        #region Properties

        public ITableCellTemplateSelector TableCellTemplateSelector
        {
            get { return _tableCellTemplateSelector; }
        }

        public ICollectionCellTemplateSelector CollectionCellTemplateSelector
        {
            get { return _collectionCellTemplateSelector; }
        }

        bool IEventListener.IsAlive
        {
            get { return true; }
        }

        bool IEventListener.IsWeak
        {
            get { return false; }
        }

        #endregion

        #region Methods

        private void UpdateValues()
        {
            if (_templateSelectorMember != null)
            {
                var value = _templateSelectorMember.GetValue(_container, Empty.Array<object>());
                _collectionCellTemplateSelector = value as ICollectionCellTemplateSelector;
                _tableCellTemplateSelector = value as ITableCellTemplateSelector;
            }
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