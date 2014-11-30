#region Copyright
// ****************************************************************************
// <copyright file="ItemsSourceAdapter.cs">
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

using System;
using System.Collections;
using System.Collections.Specialized;
using Android.Content;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Views;
using Object = Java.Lang.Object;
#if API8SUPPORT
using ActionBarEx = Android.Support.V7.App.ActionBar;
#elif API17
using ActionBarEx = Android.App.ActionBar;
#endif

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class ItemsSourceAdapter : BaseAdapter, IItemsSourceAdapter
    {
        #region Fields

        private const string Key = "#ItemsSourceAdatapter";
        private IEnumerable _itemsSource;
        private readonly object _container;
        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private readonly LayoutInflater _layoutInflater;
        private readonly DataTemplateProvider _dropDownTemplateProvider;
        private readonly DataTemplateProvider _itemTemplateProvider;
        private static Func<object, Context, IDataContext, IItemsSourceAdapter> _factory;

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
            set { SetItemsSource(value); }
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

        public override int Count
        {
            get { return ItemsSource.Count(); }
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
            return CreateView(GetRawItem(position), convertView, parent, _itemTemplateProvider, Android.Resource.Layout.SimpleListItem1);
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

        protected virtual void SetItemsSource(IEnumerable value)
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
            NotifyDataSetChanged();
        }

        protected virtual View CreateView(object value, View convertView, ViewGroup parent, DataTemplateProvider templateProvider, int defaultTemplate)
        {
            var valueView = value as View;
            if (valueView != null)
                return valueView;

            int? templateId = null;
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
            if (templateId == null)
            {
                if (!(convertView is TextView))
                    convertView = LayoutInflater.Inflate(defaultTemplate, null);
                var textView = convertView as TextView;
                if (textView != null)
                    textView.Text = value == null ? "(null)" : value.ToString();
                return textView;
            }
            var itemView = convertView as ListItem;
            if (itemView == null || itemView.TemplateId != templateId)
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
            var adapterView = Container as AdapterView;
            if (adapterView != null)
                PlatformDataBindingModule.AdapterViewSelectedPositionMember.SetValue(adapterView, adapterView.SelectedItemPosition);
            NotifyDataSetChanged();
        }

        private bool IsSpinner()
        {
#if API8
            return Container is Spinner;
#else
            return Container is Spinner || Container is ActionBarEx;
#endif
        }

        #endregion
    }
}