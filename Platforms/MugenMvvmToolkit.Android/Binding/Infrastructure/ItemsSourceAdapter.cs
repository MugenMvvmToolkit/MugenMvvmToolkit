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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Views;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class ItemsSourceAdapter : BaseAdapter, IItemsSourceAdapter
    {
        #region Fields

        private const string Key = "#ItemsSourceAdatapter";
        private static Func<object, Context, IDataContext, IItemsSourceAdapter> _factory;

        private IEnumerable _itemsSource;
        private readonly object _container;
        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private readonly LayoutInflater _layoutInflater;
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemsSourceAdapter" /> class.
        /// </summary>
        public ItemsSourceAdapter([NotNull] object container, Context context, bool listenCollectionChanges, string dropDownItemTemplateSelectorName = AttachedMemberNames.DropDownItemTemplateSelector,
            string itemTemplateSelectorName = AttachedMemberConstants.ItemTemplateSelector, string dropDownItemTemplateIdName = AttachedMemberNames.DropDownItemTemplate,
            string itemTemplateIdName = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(container, "container");
            _container = container;
            _itemTemplateProvider = new DataTemplateProvider(container, itemTemplateIdName, itemTemplateSelectorName);
            _dropDownTemplateProvider = new DataTemplateProvider(container, dropDownItemTemplateIdName,
                dropDownItemTemplateSelectorName);
            _layoutInflater = LayoutInflater.From(context);
            if (listenCollectionChanges)
                _weakHandler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this, (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
            var activityView = context.GetActivity() as IActivityView;
            if (activityView != null)
                activityView.Mediator.Destroyed += ActivityViewOnDestroyed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the factory that allows to create items source adapter.
        /// </summary>
        [NotNull]
        public static Func<object, Context, IDataContext, IItemsSourceAdapter> Factory
        {
            get { return _factory; }
            set
            {
                Should.PropertyBeNotNull(value);
                _factory = value;
            }
        }

        protected object Container
        {
            get { return _container; }
        }

        protected LayoutInflater LayoutInflater
        {
            get { return _layoutInflater; }
        }

        #endregion

        #region Implementation of IItemsSourceAdapter

        /// <summary>
        ///     Gets or sets the items source.
        /// </summary>
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
            return CreateView(GetRawItem(position), convertView, parent, _dropDownTemplateProvider, IsSpinner()
                ? Android.Resource.Layout.SimpleDropDownItem1Line
                : Android.Resource.Layout.SimpleSpinnerDropDownItem);
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
                Android.Resource.Layout.SimpleListItem1);
        }

        #endregion

        #region Methods

        public static IItemsSourceAdapter Get(object container)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<ItemsSourceAdapter>(container, Key, false);
        }

        public static void Set(object container, IItemsSourceAdapter adapter)
        {
            ServiceProvider.AttachedValueProvider.SetValue(container, Key, adapter);
        }

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
                            BindingServiceProvider.ContextManager
                                                  .GetBindingContext(valueView)
                                                  .Value = value;
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
            BindingServiceProvider.ContextManager.GetBindingContext(convertView).Value = value;
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
                var value = PlatformDataBindingModule.AdapterViewSelectedItemMember.GetValue(adapterView, null);
                if (value != null && GetPosition(value) < 0)
                    PlatformDataBindingModule.AdapterViewSelectedPositionMember.SetValue(adapterView, -1);
            }
            NotifyDataSetChanged();
        }

        private bool IsSpinner()
        {
            return Container is Spinner || PlatformExtensions.IsActionBar(Container);
        }

        private void ActivityViewOnDestroyed(Activity sender, EventArgs args)
        {
            ((IActivityView)sender).Mediator.Destroyed -= ActivityViewOnDestroyed;
            SetItemsSource(null, false);
            var adapterView = _container as AdapterView;
            if (adapterView != null && ReferenceEquals(PlatformDataBindingModule.GetAdapter(adapterView), this))
                PlatformDataBindingModule.SetAdapter(adapterView, null);
        }

        #endregion
    }
}