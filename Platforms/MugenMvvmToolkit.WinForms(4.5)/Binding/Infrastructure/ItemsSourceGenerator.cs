#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourceGenerator.cs">
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

using System;
using System.Collections;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Modules;
#if WINFORMS
using System.ComponentModel;
using System.Windows.Forms;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Interfaces.Models;
#elif TOUCH
using ObjCRuntime;
#endif
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    internal class ItemsSourceGenerator : ItemsSourceGeneratorBase
    {
        #region Fields

        private readonly IBindingMemberInfo _itemTemplateMember;
        private readonly WeakReference _view;

#if WINFORMS
        private readonly bool _isTabControl;
#endif
        #endregion

        #region Constructors

        private ItemsSourceGenerator([NotNull] object view)
        {
            Should.NotBeNull(view, "view");
#if WINFORMS
            ListenDisposeEvent(view as IComponent);
            _isTabControl = view is TabControl;
#elif TOUCH
            TryListenController(view as INativeObject);
#endif
            _view = ServiceProvider.WeakReferenceFactory(view, true);
            _itemTemplateMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(view.GetType(), AttachedMemberConstants.ItemTemplate, false, false);
        }

        #endregion

        #region Overrides of ItemsSourceGeneratorBase

        protected override IEnumerable ItemsSource { get; set; }

        protected override bool IsTargetDisposed
        {
            get
            {
                var view = GetView();
                if (view == null)
                    return true;
#if WINFORMS
                var control = view as Control;
                if (control == null)
                    return false;
                return control.IsDisposed;
#elif TOUCH
                var nativeObject = view as INativeObject;
                if (nativeObject == null)
                    return false;
                return !nativeObject.IsAlive();
#endif
            }
        }

        protected override void Add(int insertionIndex, int count)
        {
            var view = GetView();
            if (view == null)
                return;
            ICollectionViewManager viewManager = GetCollectionViewManager(view);
            for (int i = 0; i < count; i++)
            {
                int index = insertionIndex + i;
                viewManager.Insert(view, index, GetItemFromTemplate(view, index));
            }
        }

        protected override void Remove(int removalIndex, int count)
        {
            var view = GetView();
            if (view == null)
                return;
            ICollectionViewManager viewManager = GetCollectionViewManager(view);
            for (int i = 0; i < count; i++)
                viewManager.RemoveAt(view, removalIndex + i);
        }

        protected override void Replace(int startIndex, int count)
        {
            var view = GetView();
            if (view == null)
                return;
            ICollectionViewManager viewManager = GetCollectionViewManager(view);
            for (int i = 0; i < count; i++)
            {
                int index = startIndex + i;
                viewManager.RemoveAt(view, index);
                viewManager.Insert(view, index, GetItemFromTemplate(view, index));
            }
        }

        protected override void Refresh()
        {
            var view = GetView();
            if (view == null)
                return;
            ICollectionViewManager viewManager = GetCollectionViewManager(view);
            viewManager.Clear(view);
            int count = ItemsSource.Count();
            for (int i = 0; i < count; i++)
                viewManager.Insert(view, i, GetItemFromTemplate(view, i));
        }

        #endregion

        #region Methods

        public static IItemsSourceGenerator GetOrAdd(object item)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(item, Key, (o, o1) => new ItemsSourceGenerator(o), null);
        }

        private ICollectionViewManager GetCollectionViewManager(object view)
        {
            return PlatformDataBindingModule
                .CollectionViewManagerMember
                .GetValue(view, null) ?? DefaultCollectionViewManager.Instance;
        }

        private object GetItemFromTemplate(object view, int index)
        {
            object item = GetItem(index);
            if (_itemTemplateMember == null)
            {
#if WINFORMS
                if (_isTabControl)
                    return CreateDefaultTabPage(item);
#endif
                return GetDefaultTemplate(item);
            }
            var selector = (IDataTemplateSelector)_itemTemplateMember.GetValue(view, null);
            if (selector == null)
            {
#if WINFORMS
                if (_isTabControl)
                    return CreateDefaultTabPage(item);
#endif
                return GetDefaultTemplate(item);
            }
            return selector.SelectTemplateWithContext(item, view);
        }

        private static object GetDefaultTemplate(object item)
        {
            if (item is IViewModel)
                return ViewModelToViewConverter.Instance.Convert(item);
            return item;
        }

        private object GetView()
        {
            return _view.Target;
        }

#if WINFORMS
        private static TabPage CreateDefaultTabPage(object item)
        {
            var viewModel = item as IViewModel;
            if (viewModel == null)
                return new TabPage(item == null ? "null" : item.ToString());
            var page = new TabPage();
            var set = new BindingSet<TabPage, IViewModel>(page);
            set.BindFromExpression("Content ;");
            if (viewModel is IHasDisplayName)
                set.BindFromExpression("Text DisplayName;");
            set.Apply();
            BindingServiceProvider.ContextManager.GetBindingContext(page).Value = item;
            return page;
        }
#endif
        #endregion
    }
}