#region Copyright

// ****************************************************************************
// <copyright file="BindableLayoutInflaterProxy.cs">
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
using System.Xml;
using Android.Content;
using Android.Views;
using MugenMvvmToolkit.Android.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public sealed class BindableLayoutInflaterProxy : BindableLayoutInflater
    {
        #region Fields

        private readonly BindableLayoutInflater _layoutInflater;

        #endregion

        #region Constructors

        public BindableLayoutInflaterProxy(BindableLayoutInflater layoutInflater)
            : base(layoutInflater.ViewFactory, layoutInflater.Context)
        {
            _layoutInflater = layoutInflater;
        }

        #endregion

        #region Overrides of LayoutInflater

        public override IFilter Filter
        {
            get { return _layoutInflater.Filter; }
            set { _layoutInflater.Filter = value; }
        }

        public override IViewFactory ViewFactory
        {
            get { return _layoutInflater.ViewFactory; }
            set { _layoutInflater.ViewFactory = value; }
        }

        public override IFactory NestedFactory
        {
            get { return _layoutInflater.NestedFactory; }
            set { _layoutInflater.NestedFactory = value; }
        }

        public override IFactory2 NestedFactory2
        {
            get { return _layoutInflater.NestedFactory2; }
            set { _layoutInflater.NestedFactory2 = value; }
        }

        public override View Inflate(int resource, ViewGroup root)
        {
            EnsureFactoryInitialized();
            return _layoutInflater.Inflate(resource, root);
        }

        public override View Inflate(int resource, ViewGroup root, bool attachToRoot)
        {
            EnsureFactoryInitialized();
            return _layoutInflater.Inflate(resource, root, attachToRoot);
        }

        public override View Inflate(XmlReader parser, ViewGroup root)
        {
            EnsureFactoryInitialized();
            return _layoutInflater.Inflate(parser, root);
        }

        public override View Inflate(XmlReader parser, ViewGroup root, bool attachToRoot)
        {
            EnsureFactoryInitialized();
            return _layoutInflater.Inflate(parser, root, attachToRoot);
        }

        public override LayoutInflater CloneInContext(Context newContext)
        {
            EnsureFactoryInitialized();
            var inflater = (BindableLayoutInflater)_layoutInflater.CloneInContext(newContext);
            return new BindableLayoutInflaterProxy(inflater);
        }

        protected override void Initialize()
        {
        }

        #endregion

        #region Methods

        private void EnsureFactoryInitialized()
        {
            try
            {
                if (Factory != null && NestedFactory == null)
                    NestedFactory = Factory;
                if (PlatformExtensions.IsApiGreaterThan10 && Factory2 != null && NestedFactory2 == null)
                    NestedFactory2 = Factory2;
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        #endregion
    }
}
