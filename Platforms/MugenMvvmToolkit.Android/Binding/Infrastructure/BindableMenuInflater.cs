#region Copyright

// ****************************************************************************
// <copyright file="BindableMenuInflater.cs">
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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Android.Content;
using Android.Runtime;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Binding.Models;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public class BindableMenuInflater : MenuInflater, IBindableMenuInflater
    {
        #region Fields

        private readonly Dictionary<int, MenuTemplate> _menuCache;
        private readonly Context _context;
        //todo remove all
        private static readonly XmlSerializer Serializer;

        #endregion

        #region Constructors

        static BindableMenuInflater()
        {
            Serializer = new XmlSerializer(typeof(MenuTemplate), string.Empty);
        }

        public BindableMenuInflater([NotNull] Context context)
            : base(context)
        {
            Should.NotBeNull(context, nameof(context));
            _context = context;
            _menuCache = new Dictionary<int, MenuTemplate>();
        }

        [Preserve(Conditional = true)]
        protected BindableMenuInflater(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        #endregion

        #region Properties

        public virtual MenuInflater NestedMenuInflater { get; set; }

        #endregion

        #region Methods

        public override void Inflate(int menuRes, IMenu menu)
        {
            Inflate(menuRes, menu, _context.GetActivity() ?? _context);
        }

        private static bool IsDefaultMenu(XmlDocument document)
        {
            var value = document.NameTable.Get("http://schemas.android.com/apk/res/android");
            return !string.IsNullOrEmpty(value);
        }

        private void InflateDefault(int menuRes, IMenu menu)
        {
            var menuInflater = NestedMenuInflater;
            if (menuInflater == null)
                base.Inflate(menuRes, menu);
            else
                menuInflater.Inflate(menuRes, menu);
        }

        #endregion

        #region Implementation of interfaces

        public virtual void Inflate(int menuRes, IMenu menu, object parent)
        {
            MenuTemplate template;
            if (_menuCache.TryGetValue(menuRes, out template))
            {
                if (template == null)
                    InflateDefault(menuRes, menu);
                else
                    template.Apply(menu, _context, parent);
            }
            else
            {
                using (var reader = _context.Resources.GetLayout(menuRes))
                {
                    //NOTE XDocument throws an error.
                    var document = new XmlDocument();
                    document.Load(reader);
                    if (IsDefaultMenu(document))
                    {
                        InflateDefault(menuRes, menu);
                        _menuCache[menuRes] = null;
                    }
                    else
                    {
                        using (var stringReader = new StringReader(PlatformExtensions.XmlTagsToUpper(document.InnerXml)))
                        {
                            var menuWrapper = (MenuTemplate)Serializer.Deserialize(stringReader);
                            menuWrapper.Apply(menu, _context, parent);
                            _menuCache[menuRes] = menuWrapper;
                        }
                    }
                }
            }
        }

        #endregion
    }
}