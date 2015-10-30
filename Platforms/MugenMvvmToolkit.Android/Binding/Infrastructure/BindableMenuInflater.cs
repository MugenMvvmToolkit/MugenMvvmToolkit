#region Copyright

// ****************************************************************************
// <copyright file="BindableMenuInflater.cs">
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
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Android.Content;
using Android.Runtime;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Models;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    public class BindableMenuInflater : MenuInflater
    {
        #region Fields

        private static readonly XmlSerializer Serializer;
        private readonly Context _context;

        #endregion

        #region Constructors

        static BindableMenuInflater()
        {
            Serializer = new XmlSerializer(typeof(MenuTemplate), string.Empty);
        }

        public BindableMenuInflater([NotNull] Context context)
            : base(context)
        {
            Should.NotBeNull(context, "context");
            _context = context;
        }

        protected BindableMenuInflater(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        #endregion

        #region Overrides of MenuInflater

        public override void Inflate(int menuRes, IMenu menu)
        {
            Inflate(menuRes, menu, _context.GetActivity() ?? _context);
        }

        #endregion

        #region Implementation of IBindableMenuInflater

        public virtual MenuInflater NestedMenuInflater { get; set; }

        public virtual void Inflate(int menuRes, IMenu menu, object parent)
        {
            using (XmlReader reader = _context.Resources.GetLayout(menuRes))
            {
                //NOTE XDocument throws an error.
                var document = new XmlDocument();
                document.Load(reader);
                if (IsDefaultMenu(document))
                {
                    MenuInflater menuInflater = NestedMenuInflater;
                    if (menuInflater == null)
                        base.Inflate(menuRes, menu);
                    else
                        menuInflater.Inflate(menuRes, menu);
                }
                else
                {
                    using (var stringReader = new StringReader(PlatformExtensions.XmlTagsToUpper(document.InnerXml)))
                    {
                        var menuWrapper = (MenuTemplate)Serializer.Deserialize(stringReader);
                        menuWrapper.Apply(menu, _context, parent);
                    }
                }
            }
        }

        #endregion

        #region Methods

        private static bool IsDefaultMenu(XmlDocument document)
        {
            var value = document.NameTable.Get("http://schemas.android.com/apk/res/android");
            return !string.IsNullOrEmpty(value);

        }

        #endregion
    }
}
