#region Copyright
// ****************************************************************************
// <copyright file="FrameNavigationService.cs">
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
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = System.Windows.Navigation.NavigationMode;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    /// <summary>
    ///     Represents the frame navigation service.
    /// </summary>
    public class FrameNavigationService : INavigationService
    {
        #region Fields

        private readonly Frame _frame;
        private readonly ISerializer _serializer;
        private const string UriParameterSerializer = "viewmodelparameterdata";
        private const string UriParameterString = "viewmodelparameter";

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="FrameNavigationService" /> class.
        /// </summary>
        public FrameNavigationService([NotNull] Frame frame, ISerializer serializer)
        {
            Should.NotBeNull(frame, "frame");
            _frame = frame;
            _serializer = serializer;
            _frame.Navigating += OnNavigating;
            _frame.Navigated += OnNavigated;
        }

        #endregion

        #region Methods

        private void OnNavigated(object sender, NavigationEventArgs args)
        {
            var handler = Navigated;
            if (handler != null)
                handler(this, new NavigationEventArgsWrapper(args));
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs args)
        {
            var handler = Navigating;
            if (handler != null)
                handler(this, new NavigatingCancelEventArgsWrapper(args));
        }

        private object GetParameter(Uri uri)
        {
            IDictionary<string, string> parameters = uri.GetUriParameters();
            string value;
            if (parameters.TryGetValue(UriParameterString, out value))
                return value;
            if (!parameters.TryGetValue(UriParameterSerializer, out value))
                return null;
            return _serializer.Deserialize<object>(value);
        }

        #endregion

        #region Implementation of INavigationService

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public bool CanGoBack
        {
            get { return _frame.CanGoBack; }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public bool CanGoForward
        {
            get { return _frame.CanGoForward; }
        }

        /// <summary>
        ///     The current content.
        /// </summary>
        public object CurrentContent
        {
            get { return _frame.Content; }
        }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        public void GoBack()
        {
            _frame.GoBack();
        }

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        public void GoForward()
        {
            _frame.GoForward();
        }

#if WINDOWS_PHONE
        /// <summary>
        ///     Removes the most recent entry from the back stack.
        /// </summary>
        /// <returns> The entry that was removed. </returns>
        public JournalEntry RemoveBackEntry()
        {
            var page = _frame.Content as Page;
            if (page != null && page.NavigationService != null)
                return page.NavigationService.RemoveBackEntry();
            return null;
        }
#endif

        /// <summary>
        /// Gets a navigation parameter from event args.
        /// </summary>
        public object GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, "args");
            var cancelArgs = args as NavigatingCancelEventArgsWrapper;
            var uri = cancelArgs == null ? ((NavigationEventArgsWrapper)args).Args.Uri : cancelArgs.Args.Uri;
            return GetParameter(uri);
        }

        /// <summary>
        ///     Navigates using cancel event args.
        /// </summary>
        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, "args");
            var originalArgs = ((NavigatingCancelEventArgsWrapper)args).Args;
            if (originalArgs.NavigationMode == NavigationMode.Back)
            {
                _frame.GoBack();
                return true;
            }
            return _frame.Navigate(originalArgs.Uri);
        }

        /// <summary>
        ///     Displays the content located at the specified <see cref="IViewMappingItem" />.
        /// </summary>
        /// <param name="source">
        ///     The <c>IViewPageMappingItem</c> of the content to display.
        /// </param>
        /// <param name="parameter">
        ///     A <see cref="T:System.Object" /> that contains data to be used for processing during
        ///     navigation.
        /// </param>
        /// <param name="dataContext">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the content was successfully displayed; otherwise, <c>false</c>.
        /// </returns>
        public bool Navigate(IViewMappingItem source, object parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            Uri uri = source.Uri;
            if (parameter != null)
            {
                var s = parameter as string;
                var uriParameter = s == null
                    ? new KeyValuePair<string, string>(UriParameterSerializer, _serializer.SerializeToBase64String(parameter))
                    : new KeyValuePair<string, string>(UriParameterString, s);
                uri = uri.MergeUri(new[] { uriParameter });
            }
            return _frame.Navigate(uri);
        }

        /// <summary>
        ///     Raised after navigation.
        /// </summary>
        public void OnNavigated(NavigationEventArgs args)
        {
            OnNavigated(_frame, args);
        }

        /// <summary>
        ///     Raised prior to navigation.
        /// </summary>
        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        /// <summary>
        ///     Raised after navigation.
        /// </summary>
        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion
    }
}