#region Copyright
// ****************************************************************************
// <copyright file="BindableApplicationBarMenuItem.cs">
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
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;

// ReSharper disable once CheckNamespace

namespace MugenMvvmToolkit.Controls
{
    /// <summary>
    ///     An bindable item that can be added to the menu of an <see cref="IBindableApplicationBarItem" />.
    /// </summary>
    public class BindableApplicationBarMenuItem : FrameworkElement, IBindableApplicationBarItem
    {
        #region Fields

        /// <summary>
        ///     Identifies the <see cref="IsEnabled" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(true,
                    (o, args) =>
                        ((BindableApplicationBarMenuItem)o).ApplicationBarItem.IsEnabled = (bool)args.NewValue));

        /// <summary>
        ///     Identifies the <see cref="Text" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(
                    (o, args) => ((BindableApplicationBarMenuItem)o).ApplicationBarItem.Text = (string)args.NewValue));

        /// <summary>
        ///     Identifies the <see cref="Command" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(OnCommandChanged));

        /// <summary>
        ///     Identifies the <see cref="CommandParameter" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(default(object)));

        /// <summary>
        ///     Identifies the <see cref="IsVisible" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(BindableApplicationBarMenuItem),
                new PropertyMetadata(true,
                    (o, args) => ((BindableApplicationBarMenuItem)o).UpdateItem((bool)args.NewValue)));

        private IBindableApplicationBar _applicationBar;
        private int _position;
        private readonly IApplicationBarMenuItem _applicationBarItem;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindableApplicationBarMenuItem" /> class.
        /// </summary>
        public BindableApplicationBarMenuItem()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            _applicationBarItem = CreateApplicationBarItem();
            _applicationBarItem.Click += ApplicationBarItemOnClick;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the attached application bar.
        /// </summary>
        protected IBindableApplicationBar ApplicationBar
        {
            get { return _applicationBar; }
        }

        /// <summary>
        ///     Gets the original list of items.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the enabled status of the menu item.
        /// </summary>
        /// <returns>
        ///     true if the menu item is enabled; otherwise, false.
        /// </returns>
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        /// <summary>
        ///     The string to display on the menu item.
        /// </summary>
        /// <returns>
        ///     Type: <see cref="T:System.String" />.
        /// </returns>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the Application Bar Item is visible.
        /// </summary>
        /// <returns>
        ///     true if the Application Bar Item is visible; otherwise, false.
        /// </returns>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        ///     Occurs when the user taps the menu item.
        /// </summary>
        public event EventHandler Click
        {
            add { ApplicationBarItem.Click += value; }
            remove { ApplicationBarItem.Click -= value; }
        }

        /// <summary>
        ///     Gets the original application bar item.
        /// </summary>
        public IApplicationBarMenuItem ApplicationBarItem
        {
            get { return _applicationBarItem; }
        }

        /// <summary>
        ///     Gets or sets the command to invoke when this button is pressed.
        /// </summary>
        /// <returns>
        ///     The command to invoke when this button is pressed. The default is null.
        /// </returns>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the parameter to pass to the <see cref="IBindableApplicationBarItem.Command" />
        ///     property.
        /// </summary>
        /// <returns>
        ///     The parameter to pass to the <see cref="IBindableApplicationBarItem.Command" /> property. The
        ///     default is null.
        /// </returns>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        ///     Attaches to the specified <see cref="IBindableApplicationBar" />.
        /// </summary>
        public void Attach(IBindableApplicationBar applicationBar, int position)
        {
            Should.NotBeNull(applicationBar, "applicationBar");
            if (_applicationBar != null)
                throw new InvalidOperationException(
                    "The BindableApplicationBarMenuItem is already attached to an IBindableApplicationBar.");
            var localValue = ReadLocalValue(DataContextProperty);
            if (localValue == DependencyProperty.UnsetValue)
                SetBinding(DataContextProperty, new Binding("DataContext") { Source = applicationBar });
            _applicationBar = applicationBar;
            _position = position;
            UpdateItem(IsVisible);
        }

        /// <summary>
        ///     Detaches this instance from its associated object.
        /// </summary>
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

        /// <summary>
        ///     Creates an instance of <see cref="IApplicationBarMenuItem" />
        /// </summary>
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