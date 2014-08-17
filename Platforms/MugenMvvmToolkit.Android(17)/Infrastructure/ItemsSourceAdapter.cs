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
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Views;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Infrastructure
{
    [Preserve(AllMembers = true)]
    public class ItemsSourceAdapter : BaseAdapter
    {
        #region Fields

        private const string Key = "#ItemsSourceAdatapter";
        private IEnumerable _itemsSource;
        private readonly object _container;
        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private readonly LayoutInflater _layoutInflater;
        private readonly ValueTemplateManager _dropDownTemplateManager;
        private readonly ValueTemplateManager _itemTemplateManager;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemsSourceAdapter" /> class.
        /// </summary>
        public ItemsSourceAdapter([NotNull] object container, Context context, bool listenCollectionChanges, string dropDownItemTemplateSelectorName = AttachedMemberNames.DropDownItemTemplateSelector,
            string itemTemplateSelectorName = AttachedMemberConstants.ItemTemplateSelector, string dropDownItemTemplateIdName = AttachedMemberNames.DropDownItemTemplate,
            string itemTemplateIdName = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(container, "container");
            _container = container;
            _itemTemplateManager = new ValueTemplateManager(container, itemTemplateIdName, itemTemplateSelectorName);
            _dropDownTemplateManager = new ValueTemplateManager(container, dropDownItemTemplateIdName,
                dropDownItemTemplateSelectorName);
            _layoutInflater = LayoutInflater.From(context);
            if (listenCollectionChanges)
                _weakHandler = PlatformExtensions.MakeWeakCollectionChangedHandler(this, (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
        }

        #endregion

        #region Properties

        protected object Container
        {
            get { return _container; }
        }

        protected LayoutInflater LayoutInflater
        {
            get { return _layoutInflater; }
        }

        public virtual IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set { SetItemsSource(value); }
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
            return CreateView(GetRawItem(position), convertView, parent, _dropDownTemplateManager, IsSpinner()
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
            return CreateView(GetRawItem(position), convertView, parent, _itemTemplateManager, Android.Resource.Layout.SimpleListItem1);
        }

        #endregion

        #region Methods

        public static ItemsSourceAdapter Get(object container)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<ItemsSourceAdapter>(container, Key, false);
        }

        public static void Set(object container, ItemsSourceAdapter adapter)
        {
            ServiceProvider.AttachedValueProvider.SetValue(container, Key, adapter);
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

        protected virtual View CreateView(object value, View convertView, ViewGroup parent, ValueTemplateManager templateManager, int defaultTemplate)
        {
            var valueView = value as View;
            if (valueView != null)
                return valueView;
            int? templateId = null;
            object template;
            if (templateManager.TrySelectTemplate(value, out template))
            {
                if (template != null)
                {
                    valueView = template as View;
                    if (valueView != null)
                    {
                        BindingProvider.Instance
                                       .ContextManager
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
                templateId = templateManager.GetTemplateId();
            if (templateId == null)
            {
                if (!(convertView is TextView))
                    convertView = LayoutInflater.Inflate(defaultTemplate, null);
                var textView = convertView as TextView;
                if (textView != null)
                    textView.Text = value == null ? "(null)" : value.ToString();
                return textView;
            }
            var itemView = convertView as ListItemView;
            if (itemView == null || itemView.TemplateId != templateId)
                convertView = CreateView(value, parent, templateId.Value);
            BindingProvider.Instance.ContextManager.GetBindingContext(convertView).Value = value;
            return convertView;
        }

        protected virtual View CreateView(object value, ViewGroup parent, int templateId)
        {
            return new ListItemView(templateId, LayoutInflater);
        }

        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var adapterView = Container as AdapterView;
            if (adapterView != null)
                AttachedMembersModule.AdapterViewSelectedPositionMember.SetValue(adapterView, new object[] { adapterView.SelectedItemPosition });
            NotifyDataSetChanged();
        }

        private bool IsSpinner()
        {
#if API8
            return Container is Spinner;
#else
            return Container is Spinner || Container is ActionBar;
#endif
        }

        #endregion
    }
}