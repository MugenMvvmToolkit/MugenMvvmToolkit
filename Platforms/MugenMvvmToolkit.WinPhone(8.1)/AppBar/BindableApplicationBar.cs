#region Copyright

// ****************************************************************************
// <copyright file="BindableApplicationBar.cs">
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MugenMvvmToolkit.WinPhone.Interfaces;

namespace MugenMvvmToolkit.WinPhone.AppBar
{
    [ContentProperty("Buttons")]
    public class BindableApplicationBar : DependencyObject, IBindableApplicationBar
    {
        #region Attached properties

        public static readonly DependencyProperty ApplicationBarProperty =
            DependencyProperty.RegisterAttached("ApplicationBar", typeof(IBindableApplicationBar),
                typeof(BindableApplicationBar), new PropertyMetadata(OnApplicationBarChanged));

        public static void SetApplicationBar(UIElement element, IBindableApplicationBar value)
        {
            element.SetValue(ApplicationBarProperty, value);
        }

        public static IBindableApplicationBar GetApplicationBar(UIElement element)
        {
            return (IBindableApplicationBar)element.GetValue(ApplicationBarProperty);
        }

        private static void OnApplicationBarChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            if (ServiceProvider.IsDesignMode)
                return;
            var applicationPage = (PhoneApplicationPage)dependencyObject;
            var applicationBar = (IBindableApplicationBar)args.OldValue;
            if (applicationBar != null)
                applicationBar.Detach();
            applicationBar = (IBindableApplicationBar)args.NewValue;
            if (applicationBar != null)
                applicationBar.Attach(applicationPage);
        }

        #endregion

        #region Fields

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(BindableApplicationBar),
                new PropertyMetadata(true,
                    (o, args) => ((BindableApplicationBar)o)._applicationBar.IsVisible = (bool)args.NewValue));

        public static readonly DependencyProperty OpacityProperty =
            DependencyProperty.Register("Opacity", typeof(double), typeof(BindableApplicationBar),
                new PropertyMetadata(1d,
                    (o, args) => ((BindableApplicationBar)o)._applicationBar.Opacity = (double)args.NewValue));

        public static readonly DependencyProperty IsMenuEnabledProperty =
            DependencyProperty.Register("IsMenuEnabled", typeof(bool), typeof(BindableApplicationBar),
                new PropertyMetadata(true,
                    (o, args) => ((BindableApplicationBar)o)._applicationBar.IsMenuEnabled = (bool)args.NewValue));

        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(BindableApplicationBar),
                new PropertyMetadata(
                    (o, args) => ((BindableApplicationBar)o)._applicationBar.BackgroundColor = (Color)args.NewValue));

        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor", typeof(Color), typeof(BindableApplicationBar),
                new PropertyMetadata(
                    (o, args) => ((BindableApplicationBar)o)._applicationBar.ForegroundColor = (Color)args.NewValue));

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(ApplicationBarMode), typeof(BindableApplicationBar),
                new PropertyMetadata(ApplicationBarMode.Default,
                    (o, args) => ((BindableApplicationBar)o)._applicationBar.Mode = (ApplicationBarMode)args.NewValue));

        public static readonly DependencyProperty ButtonItemsSourceProperty =
            DependencyProperty.Register("ButtonItemsSource", typeof(IEnumerable), typeof(BindableApplicationBar),
                new PropertyMetadata(OnButtonItemsSourceChanged));

        public static readonly DependencyProperty MenuItemsSourceProperty =
            DependencyProperty.Register("MenuItemsSource", typeof(IEnumerable), typeof(BindableApplicationBar),
                new PropertyMetadata(OnMenuItemsSourceChanged));

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(object), typeof(BindableApplicationBar), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty ButtonItemTemplateProperty =
            DependencyProperty.Register("ButtonItemTemplate", typeof(DataTemplate), typeof(BindableApplicationBar),
                new PropertyMetadata(OnButtonItemTemplateChanged));

        public static readonly DependencyProperty MenuItemTemplateProperty =
            DependencyProperty.Register("MenuItemTemplate", typeof(DataTemplate), typeof(BindableApplicationBar),
                new PropertyMetadata(OnMenuItemTemplateChanged));

        private static readonly NotifyCollectionChangedEventArgs ResetEvent =
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

        private readonly ApplicationBar _applicationBar;
        private readonly List<IBindableApplicationBarItem> _items;
        private PhoneApplicationPage _page;
        private readonly IList _buttons;
        private readonly IList _menuItems;

        #endregion

        #region Constructors

        public BindableApplicationBar()
        {
            _applicationBar = new ApplicationBar();
            _items = new List<IBindableApplicationBarItem>();

            var buttonItems = new ObservableCollection<IBindableApplicationBarItem>();
            buttonItems.CollectionChanged += ButtonItemsOnCollectionChanged;
            _buttons = buttonItems;

            var menuItems = new ObservableCollection<IBindableApplicationBarItem>();
            menuItems.CollectionChanged += MenuItemsOnCollectionChanged;
            _menuItems = menuItems;
        }

        #endregion

        #region Implementation of IBindableApplicationBar

        public object DataContext
        {
            get { return GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public DataTemplate ButtonItemTemplate
        {
            get { return (DataTemplate)GetValue(ButtonItemTemplateProperty); }
            set { SetValue(ButtonItemTemplateProperty, value); }
        }

        public DataTemplate MenuItemTemplate
        {
            get { return (DataTemplate)GetValue(MenuItemTemplateProperty); }
            set { SetValue(MenuItemTemplateProperty, value); }
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        public bool IsMenuEnabled
        {
            get { return (bool)GetValue(IsMenuEnabledProperty); }
            set { SetValue(IsMenuEnabledProperty, value); }
        }

        public Color BackgroundColor
        {
            get { return (Color)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public Color ForegroundColor
        {
            get { return (Color)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        public ApplicationBarMode Mode
        {
            get { return (ApplicationBarMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public IEnumerable ButtonItemsSource
        {
            get { return (IEnumerable)GetValue(ButtonItemsSourceProperty); }
            set { SetValue(ButtonItemsSourceProperty, value); }
        }

        public IEnumerable MenuItemsSource
        {
            get { return (IEnumerable)GetValue(MenuItemsSourceProperty); }
            set { SetValue(MenuItemsSourceProperty, value); }
        }

        public double DefaultSize => _applicationBar.DefaultSize;

        public double MiniSize => _applicationBar.MiniSize;

        public IList Buttons => _buttons;

        public IList MenuItems => _menuItems;

        public IApplicationBar OriginalApplicationBar => _applicationBar;

        public event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged
        {
            add { _applicationBar.StateChanged += value; }
            remove { _applicationBar.StateChanged -= value; }
        }

        public void Attach(object target)
        {
            Should.NotBeNull(target, nameof(target));
            _page = (PhoneApplicationPage)target;
            _page.ApplicationBar = OriginalApplicationBar;
            if (ReadLocalValue(DataContextProperty) == DependencyProperty.UnsetValue)
                BindingOperations.SetBinding(this, DataContextProperty, new System.Windows.Data.Binding("DataContext") { Source = _page });
        }

        public void Detach()
        {
            _page.ApplicationBar = null;
            ClearValue(DataContextProperty);
        }

        #endregion

        #region Methods

        private static void OnButtonItemTemplateChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var bindableApplicationBar = (BindableApplicationBar)dependencyObject;
            if (bindableApplicationBar.ButtonItemsSource == null)
                return;
            bindableApplicationBar.OnMenuItemsSourceCollectionChanged(bindableApplicationBar.ButtonItemsSource,
                ResetEvent);
        }

        private static void OnMenuItemTemplateChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var bindableApplicationBar = (BindableApplicationBar)dependencyObject;
            if (bindableApplicationBar.MenuItemsSource == null)
                return;
            bindableApplicationBar.OnMenuItemsSourceCollectionChanged(bindableApplicationBar.MenuItemsSource, ResetEvent);
        }

        private static void OnButtonItemsSourceChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var applicationBar = (BindableApplicationBar)dependencyObject;
            if (applicationBar.Buttons.Count != 0)
                throw new InvalidOperationException("Buttons collection must be empty before using ButtonItemsSource.");
            applicationBar.OnItemsSourceChanged(args, applicationBar.OnButtonItemsSourceCollectionChanged,
                objects => applicationBar.OnButtonItemsSourceCollectionChanged(objects, ResetEvent));
        }

        private static void OnMenuItemsSourceChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var applicationBar = (BindableApplicationBar)dependencyObject;
            if (applicationBar.MenuItems.Count != 0)
                throw new InvalidOperationException("MenuItems collection must be empty before using MenuItemsSource.");
            applicationBar.OnItemsSourceChanged(args, applicationBar.OnMenuItemsSourceCollectionChanged,
                objects => applicationBar.OnMenuItemsSourceCollectionChanged(objects, ResetEvent));
        }

        private void OnItemsSourceChanged(DependencyPropertyChangedEventArgs args,
            NotifyCollectionChangedEventHandler handler, Action<IEnumerable<object>> updateItems)
        {
            var items = args.OldValue as IEnumerable<object>;
            if (items != null)
            {
                UpdateItems(ResetEvent, Enumerable.Empty<object>(), CastItemToAppBar, CastItemToAppBar);
                var collectionChanged = items as INotifyCollectionChanged;
                if (collectionChanged != null)
                    collectionChanged.CollectionChanged -= handler;
            }
            items = args.NewValue as IEnumerable<object>;
            if (items != null)
            {
                updateItems(items);
                var collectionChanged = items as INotifyCollectionChanged;
                if (collectionChanged != null)
                    collectionChanged.CollectionChanged += handler;
            }
        }

        private void MenuItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (ServiceProvider.IsDesignMode)
                return;
            if (MenuItemsSource != null)
                throw new InvalidOperationException(
                    "Operation is not valid while MenuItemsSource is in use. Access and modify elements with BindableApplicationBar.MenuItemsSource instead.");
            UpdateItems(args, (IEnumerable<object>)MenuItems, CastItemToAppBar, CastItemToAppBar);
        }

        private void ButtonItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (ServiceProvider.IsDesignMode)
                return;
            if (ButtonItemsSource != null)
                throw new InvalidOperationException(
                    "Operation is not valid while ButtonItemsSource is in use. Access and modify elements with BindableApplicationBar.ButtonItemsSource instead.");
            UpdateItems(args, (IEnumerable<object>)Buttons, CastItemToAppBar, CastItemToAppBar);
        }

        private void OnButtonItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateItems(e, (IEnumerable<object>)sender, CreateButtonItemFromTemplate, FindAppBarByItem);
        }

        private void OnMenuItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateItems(e, (IEnumerable<object>)sender, CreateMenuItemFromTemplate, FindAppBarByItem);
        }

        private void UpdateItems(NotifyCollectionChangedEventArgs args, IEnumerable<object> sourceCollection,
            Func<object, IBindableApplicationBarItem> createItem, Func<object, IBindableApplicationBarItem> findItem)
        {
            IEnumerable oldItems = args.OldItems;
            IEnumerable newItems = args.NewItems;
            int startIndex = args.NewStartingIndex;
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                for (int index = 0; index < _items.Count; index++)
                    _items[index].Detach();
                _items.Clear();
                newItems = sourceCollection;
                startIndex = 0;
            }

            if (oldItems != null)
            {
                foreach (
                    IBindableApplicationBarItem oldItem in
                        oldItems.OfType<object>().Select(findItem).Where(item => item != null))
                {
                    if (_items.Remove(oldItem))
                        oldItem.Detach();
                }
            }

            if (newItems != null)
            {
                int i = startIndex;
                foreach (
                    IBindableApplicationBarItem newItem in
                        newItems.OfType<object>().Select(createItem).Where(item => item != null))
                {
                    newItem.Attach(this, +i);
                    _items.Add(newItem);
                    i++;
                }
            }
        }

        private IBindableApplicationBarItem CreateButtonItemFromTemplate(object item)
        {
            if (ButtonItemTemplate == null)
                return null;
            var content = (IBindableApplicationBarItem)ButtonItemTemplate.LoadContent();
            if (content == null)
                throw new InvalidOperationException(
                    "BindableApplicationBar cannot use the ButtonItemsSource property without a valid ButtonTemplate");
            content.DataContext = item;
            return content;
        }

        private IBindableApplicationBarItem CreateMenuItemFromTemplate(object item)
        {
            if (MenuItemTemplate == null)
                return null;
            var content = (IBindableApplicationBarItem)MenuItemTemplate.LoadContent();
            if (content == null)
                throw new InvalidOperationException(
                    "BindableApplicationBar cannot use the MenuItemsSource property without a valid MenuItemTemplate");
            content.DataContext = item;
            return content;
        }

        private IBindableApplicationBarItem FindAppBarByItem(object obj)
        {
            return _items.FirstOrDefault(item => item.DataContext == obj);
        }

        private static IBindableApplicationBarItem CastItemToAppBar(object item)
        {
            return (IBindableApplicationBarItem)item;
        }

        #endregion
    }
}
