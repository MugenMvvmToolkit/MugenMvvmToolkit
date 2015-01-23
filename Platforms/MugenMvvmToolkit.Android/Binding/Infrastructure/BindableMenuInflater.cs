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
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Android.Content;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindableMenuInflater : MenuInflater, IBindableMenuInflater
    {
        #region Fields

        private readonly Context _context;
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
            Should.NotBeNull(context, "context");
            _context = context;
        }

        #endregion

        #region Overrides of MenuInflater

        public override void Inflate(int menuRes, IMenu menu)
        {
            Inflate(menuRes, menu, _context.GetActivity() ?? _context);
        }

        #endregion

        #region Implementation of IBindableMenuInflater

        /// <summary>
        ///     Gets or sets underlying menu inflater, if any.
        /// </summary>
        public MenuInflater MenuInflater { get; set; }

        /// <summary>
        ///     Inflate a menu hierarchy from the specified XML resource.
        /// </summary>
        public void Inflate(int menuRes, IMenu menu, object parent)
        {
            using (XmlReader reader = _context.Resources.GetLayout(menuRes))
            {
                //NOTE XDocument throws an error.
                var document = new XmlDocument();
                document.Load(reader);
                if (document.FirstChild != null && !IsDefaultMenu(document))
                {
                    using (var stringReader = new StringReader(PlatformExtensions.XmlTagsToUpper(document.InnerXml)))
                    {
                        var menuWrapper = (MenuTemplate)Serializer.Deserialize(stringReader);
                        menuWrapper.Apply(menu, _context, parent);
                    }
                }
                else
                {
                    MenuInflater menuInflater = MenuInflater;
                    if (menuInflater == null)
                        base.Inflate(menuRes, menu);
                    else
                        menuInflater.Inflate(menuRes, menu);
                }
            }
        }

        #endregion

        #region Methods

        private static bool IsDefaultMenu(XmlDocument document)
        {
            foreach (var attribute in document.FirstChild.Attributes.OfType<XmlAttribute>())
            {
                if (string.Equals(attribute.Value, "http://schemas.android.com/apk/res/android",
                    StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion
    }
}