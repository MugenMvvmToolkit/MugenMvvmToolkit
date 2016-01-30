#region Copyright

// ****************************************************************************
// <copyright file="BindableApplicationBarMenuItem.cs">
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
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using MugenMvvmToolkit.WinPhone.Interfaces;

namespace MugenMvvmToolkit.WinPhone.AppBar
{
    public class BindableApplicationBarMenuItem : FrameworkElement, IBindableApplicationBarItem
    {
        #region Fields

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(true,
                    (o, args) =>
                        ((BindableApplicationBarMenuItem)o).ApplicationBarItem.IsEnabled = (bool)args.NewValue));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(
                    (o, args) => ((BindableApplicationBarMenuItem)o).ApplicationBarItem.Text = (string)args.NewValue));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(default(object)));

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(true,
                    (o, args) => ((BindableApplicationBarMenuItem)o).UpdateItem((bool)args.NewValue)));

        private IBindableApplicationBar _applicationBar;
        private int _position;
        private readonly IApplicationBarMenuItem _applicationBarItem;

        #endregion

        #region Constructors

        public BindableApplicationBarMenuItem()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            _applicationBarItem = CreateApplicationBarItem();
            _applicationBarItem.Click += ApplicationBarItemOnClick;
        }

        #endregion

        #region Properties

        protected IBindableApplicationBar ApplicationBar => _applicationBar;

        protected virtual IList OriginalList
        {
            get
            {
                if (ApplicationBar == null || ApplicationBar.OriginalApplicationBar == null)
                    return null;
                return ApplicationBar.OriginalApplicationBar.MenuItems;
            }
        }

        #endregion

        #region Implementation of IBindableApplicationBarItem

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public event EventHandler Click
        {
            add { ApplicationBarItem.Click += value; }
            remove { ApplicationBarItem.Click -= value; }
        }

        public IApplicationBarMenuItem ApplicationBarItem => _applicationBarItem;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public void Attach(IBindableApplicationBar applicationBar, int position)
        {
            Should.NotBeNull(applicationBar, nameof(applicationBar));
            if (_applicationBar != null)
                throw new InvalidOperationException(
                    "The BindableApplicationBarMenuItem is already attached to an IBindableApplicationBar.");
            var localValue = ReadLocalValue(DataContextProperty);
            if (localValue == DependencyProperty.UnsetValue)
                SetBinding(DataContextProperty, new System.Windows.Data.Binding("DataContext") { Source = applicationBar });
            _applicationBar = applicationBar;
            _position = position;
            UpdateItem(IsVisible);
        }

        public void Detach()
        {
            if (_applicationBar == null)
                throw new InvalidOperationException(
                    "The BindableApplicationBarMenuItem is not attached to an IBindableApplicationBar.");
            OriginalList.Remove(ApplicationBarItem);
            _applicationBar = null;
            _position = -1;
        }

        #endregion

        #region Methods

        protected virtual IApplicationBarMenuItem CreateApplicationBarItem()
        {
            return new ApplicationBarMenuItem();
        }

        private void UpdateItem(bool isVisible)
        {
            IList originalList = OriginalList;
            bool contains = originalList.Contains(ApplicationBarItem);
            if (isVisible)
            {
                if (contains)
                    return;
                if (originalList.Count > _position)
                    originalList.Insert(_position, ApplicationBarItem);
                else
                    originalList.Add(ApplicationBarItem);
            }
            else if (contains)
            {
                originalList.Remove(ApplicationBarItem);
            }
        }

        private void ApplicationBarItemOnClick(object sender, EventArgs eventArgs)
        {
            if (Command != null)
                Command.Execute(CommandParameter);
        }

        private void CommandOnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            if (Command != null && IsVisible)
                IsEnabled = Command.CanExecute(CommandParameter);
        }

        private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var menuItem = (BindableApplicationBarMenuItem)dependencyObject;
            var command = args.OldValue as ICommand;
            if (command != null)
                command.CanExecuteChanged -= menuItem.CommandOnCanExecuteChanged;
            command = args.NewValue as ICommand;
            if (command == null)
                menuItem.IsEnabled = true;
            else
            {
                menuItem.IsEnabled = command.CanExecute(menuItem.GetValue(CommandParameterProperty));
                command.CanExecuteChanged += menuItem.CommandOnCanExecuteChanged;
            }
        }

        #endregion
    }
}
