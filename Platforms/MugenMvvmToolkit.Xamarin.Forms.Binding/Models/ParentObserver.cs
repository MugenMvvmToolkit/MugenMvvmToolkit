#region Copyright

// ****************************************************************************
// <copyright file="ParentObserver.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.ComponentModel;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Models
{
    internal sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private WeakReference _parent;
        private readonly WeakReference _view;
        private bool _isAttached;

        #endregion

        #region Constructors

        private ParentObserver(Element view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view);
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(FindParent(view), Empty.WeakReference, false);
            view.PropertyChanged += OnPropertyChanged;
        }

        #endregion

        #region Properties

        public Element Source => (Element)_view.Target;

        public object Parent
        {
            get { return _parent.Target; }
            set
            {
                if (!_isAttached)
                {
                    _isAttached = true;
                    var element = GetSource();
                    if (element != null)
                        element.PropertyChanged -= OnPropertyChanged;
                }
                SetParent(GetSource(), value);
            }
        }

        #endregion

        #region Methods

        public static object Get(Element view)
        {
            var value = GetOrAdd(view);
            var parentObserver = value as ParentObserver;
            if (parentObserver == null)
                return (value as WeakReference)?.Target;
            return parentObserver.Parent;
        }

        public static void Set(Element view, object parent)
        {
            var value = GetOrAdd(view);
            var parentObserver = value as ParentObserver;
            if (parentObserver != null)
                parentObserver.Parent = parent;
        }

        public static IDisposable AddListener(Element view, IEventListener listener)
        {
            var value = GetOrAdd(view);
            return (value as ParentObserver)?.AddWithUnsubscriber(listener);
        }

        private static object GetOrAdd(Element element)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd<Element, object>(element, "#ParentListener", (item, o) =>
                {
                    bool? value;
                    item.TryGetBindingMemberValue(AttachedMembersBase.Object.IsFlatTree, out value);
                    if (value == null)
                    {
                        var parent = FindParent(item) as View;
                        while (parent != null)
                        {
                            parent.TryGetBindingMemberValue(AttachedMembersBase.Object.IsFlatTree, out value);
                            if (value == null)
                                parent = FindParent(parent) as View;
                            else if (value.Value)
                                return ServiceProvider.WeakReferenceFactory(parent);
                            else
                                break;
                        }
                    }
                    return new ParentObserver(item);
                }, null);
        }

        private void SetParent(object source, object value)
        {
            if (source == null)
                return;
            if (ReferenceEquals(value, _parent.Target))
                return;
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, false);
            Raise(source, EventArgs.Empty);
        }

        private Element GetSource()
        {
            var source = Source;
            if (source == null)
                Clear();
            return source;
        }

        private static object FindParent(Element target)
        {
            return target.Parent;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Parent")
                SetParent(sender, FindParent((Element)sender));
        }

        #endregion
    }
}
