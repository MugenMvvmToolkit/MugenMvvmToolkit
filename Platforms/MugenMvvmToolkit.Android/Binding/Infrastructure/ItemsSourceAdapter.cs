#region Copyright

// ****************************************************************************
// <copyright file="ItemsSourceAdapter.cs">
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
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Binding.Modules;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Android.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces.Models;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public class ItemsSourceAdapter : BaseAdapter, IItemsSourceAdapter
    {
        #region Fields

        private static Func<object, Context, IDataContext, IItemsSourceAdapter> _factory;

        private IEnumerable _itemsSource;
        private readonly object _container;
        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private readonly ReflectionExtensions.IWeakEventHandler<EventArgs> _listener;
        private readonly BindableLayoutInflater _layoutInflater;
        private readonly DataTemplateProvider _dropDownTemplateProvider;
        private readonly DataTemplateProvider _itemTemplateProvider;

        private Dictionary<int, int> _resourceTypeToItemType;
        private int _currentTypeIndex;

        #endregion

        #region Constructors

        static ItemsSourceAdapter()
        {
            _factory = (o, context, arg3) => new ItemsSourceAdapter(o, context, !ReferenceEquals(ViewGroupItemsSourceGenerator.Context, arg3));
        }

        public ItemsSourceAdapter([NotNull] object container, Context context, bool listenCollectionChanges, string dropDownItemTemplateSelectorName = null,
            string itemTemplateSelectorName = AttachedMemberConstants.ItemTemplateSelector, string dropDownItemTemplateIdName = null,
            string itemTemplateIdName = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(container, "container");
            _container = container;
            _itemTemplateProvider = new DataTemplateProvider(container, itemTemplateIdName, itemTemplateSelectorName);
            _dropDownTemplateProvider = new DataTemplateProvider(container,
                dropDownItemTemplateIdName ?? AttachedMembers.AdapterView.DropDownItemTemplate,
                dropDownItemTemplateSelectorName ?? AttachedMembers.AdapterView.DropDownItemTemplateSelector);
            _layoutInflater = context.GetBindableLayoutInflater();
            if (listenCollectionChanges)
                _weakHandler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this, (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
            var activityView = context.GetActivity() as IActivityView;
            if (activityView != null)
            {
                _listener = ReflectionExtensions.CreateWeakEventHandler<ItemsSourceAdapter, EventArgs>(this, (adapter, o, arg3) => adapter.ActivityViewOnDestroyed((Activity)o));
                activityView.Mediator.Destroyed += _listener.Handle;
            }
        }

        #endregion

        #region Properties

        [NotNull]
        public static Func<object, Context, IDataContext, IItemsSourceAdapter> Factory
        {
            get { return _factory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _factory = value;
            }
        }

        protected object Container
        {
            get { return _container; }
        }

        protected BindableLayoutInflater LayoutInflater
        {
            get { return _layoutInflater; }
        }

        #endregion

        #region Implementation of IItemsSourceAdapter

        public virtual IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set { SetItemsSource(value, true); }
        }

        public virtual object GetRawItem(int position)
        {
            if (position < 0)
                return null;
            return ItemsSource.ElementAtIndex(position);
        }

        public virtual int GetPosition(object value)
        {
            return ItemsSource.IndexOf(value);
        }

        #endregion

        #region Overrides of BaseAdapter

        public override int GetItemViewType(int position)
        {
            var selector = _itemTemplateProvider.GetDataTemplateSelector() as IResourceDataTemplateSelector;
            if (selector == null)
                return base.GetItemViewType(position);
            if (_resourceTypeToItemType == null)
                _resourceTypeToItemType = new Dictionary<int, int>();
            var id = selector.SelectTemplate(GetRawItem(position), _container);
            int type;
            if (!_resourceTypeToItemType.TryGetValue(id, out type))
            {
                type = _currentTypeIndex++;
                _resourceTypeToItemType[id] = type;
            }
            return type;
        }

        public override int ViewTypeCount
        {
            get
            {
                var resourceDataTemplateSelector = _itemTemplateProvider.GetDataTemplateSelector() as IResourceDataTemplateSelector;
                if (resourceDataTemplateSelector == null)
                    return base.ViewTypeCount;
                return resourceDataTemplateSelector.TemplateTypeCount;
            }
        }

        public override int Count
        {
            get
            {
                if (ItemsSource == null)
                    return 0;
                return ItemsSource.Count();
            }
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            if (ItemsSource == null)
                return null;
            var view = CreateView(GetRawItem(position), convertView, parent, _dropDownTemplateProvider, IsSpinner()
                ? global::Android.Resource.Layout.SimpleDropDownItem1Line
                : global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            if (view != null && !ReferenceEquals(view, convertView))
            {
                view.ListenParentChange();
                view.SetBindingMemberValue(AttachedMembers.Object.Parent, Container);
            }
            return view;
        }

        public override Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (ItemsSource == null)
                return null;
            return CreateView(GetRawItem(position), convertView, parent, _itemTemplateProvider,
                global::Android.Resource.Layout.SimpleListItem1);
        }

        #endregion

        #region Methods

        protected virtual void SetItemsSource(IEnumerable value, bool notifyDataSet)
        {
            if (ReferenceEquals(value, _itemsSource))
                return;
            if (_weakHandler == null)
                _itemsSource = value;
            else
            {
                var notifyCollectionChanged = _itemsSource as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged -= _weakHandler;
                _itemsSource = value;
                notifyCollectionChanged = _itemsSource as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged += _weakHandler;
            }
            if (notifyDataSet)
                NotifyDataSetChanged();
        }

        protected virtual View CreateView(object value, View convertView, ViewGroup parent, DataTemplateProvider templateProvider, int defaultTemplate)
        {
            var valueView = value as View;
            if (valueView != null)
                return valueView;

            int? templateId = null;
            int id;
            if (templateProvider.TrySelectResourceTemplate(value, out id))
                templateId = id;
            else
            {
                object template;
                if (templateProvider.TrySelectTemplate(value, out template))
                {
                    if (template != null)
                    {
                        valueView = template as View;
                        if (valueView != null)
                        {
                            valueView.SetDataContext(value);
                            return valueView;
                        }
                        if (template is int)
                            templateId = (int)template;
                        else
                            value = template;
                    }
                }
                else
                    templateId = templateProvider.GetTemplateId();
            }

            if (templateId == null)
            {
                if (!(convertView is TextView))
                    convertView = LayoutInflater.Inflate(defaultTemplate, null);
                var textView = convertView as TextView;
                if (textView != null)
                    textView.Text = value.ToStringSafe("(null)");
                return textView;
            }
            var itemView = convertView as ListItem;
            if (itemView == null || itemView.TemplateId != templateId.Value)
                convertView = CreateView(value, parent, templateId.Value);
            convertView.SetDataContext(value);
            return convertView;
        }

        protected virtual View CreateView(object value, ViewGroup parent, int templateId)
        {
            return new ListItem(templateId, LayoutInflater);
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var item = Container as Object;
            if (!item.IsAlive())
            {
                SetItemsSource(null, false);
                return;
            }
            var adapterView = Container as AdapterView;
            if (adapterView != null && args.Action != NotifyCollectionChangedAction.Add)
            {
                var value = adapterView.GetBindingMemberValue(AttachedMembers.AdapterView.SelectedItem);
                if (value != null && GetPosition(value) < 0)
                {
                    var index = args.OldStartingIndex;
                    var maxIndex = ItemsSource.Count() - 1;
                    while (index > maxIndex)
                        --index;
                    adapterView.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItem, GetRawItem(index));
                }
            }
            NotifyDataSetChanged();
        }

        private bool IsSpinner()
        {
            return Container is Spinner || PlatformExtensions.IsActionBar(Container);
        }

        private void ActivityViewOnDestroyed(Activity sender)
        {
            ((IActivityView)sender).Mediator.Destroyed -= _listener.Handle;
            SetItemsSource(null, false);
            var adapterView = _container as AdapterView;
            if (adapterView.IsAlive() && ReferenceEquals(PlatformDataBindingModule.GetAdapter(adapterView), this))
                PlatformDataBindingModule.SetAdapter(adapterView, null);
        }

        #endregion
    }
}
