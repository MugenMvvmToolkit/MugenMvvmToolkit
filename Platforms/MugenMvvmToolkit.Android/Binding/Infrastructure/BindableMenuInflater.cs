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
using System.Xml;
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

        private static readonly Dictionary<int, object> MenuCache;
        private readonly Context _context;

        #endregion

        #region Constructors

        static BindableMenuInflater()
        {
            MenuCache = new Dictionary<int, object>();
        }

        public BindableMenuInflater([NotNull] Context context)
            : base(context)
        {
            Should.NotBeNull(context, nameof(context));
            _context = context;
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

        private static bool IsDefaultMenu(XmlReader reader)
        {
            var value = reader.NameTable.Get("http://schemas.android.com/apk/res/android");
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

        private static bool TryGetTemplate(int res, out MenuTemplate template)
        {
            object value;
            var result = MenuCache.TryGetValue(res, out value);
            template = (MenuTemplate)value;
            return result;
        }

        #endregion

        #region Implementation of interfaces

        public virtual void Inflate(int menuRes, IMenu menu, object parent)
        {
            MenuTemplate template;
            if (TryGetTemplate(menuRes, out template))
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
                    if (IsDefaultMenu(reader))
                    {
                        InflateDefault(menuRes, menu);
                        MenuCache[menuRes] = null;
                    }
                    else
                    {
                        var menuWrapper = reader.Deserialize<MenuTemplate>();
                        menuWrapper.Apply(menu, _context, parent);
                        MenuCache[menuRes] = menuWrapper;
                    }
                }
            }
        }

        #endregion
    }
}