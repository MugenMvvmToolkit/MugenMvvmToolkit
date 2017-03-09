#region Copyright

// ****************************************************************************
// <copyright file="BindingResourceExtensions.cs">
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
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Binding
{
    public static class BindingResourceExtensions
    {
        #region Properties

        public static Func<Context, int, int> ColorHandler { get; set; }

        public static Func<Context, int, Drawable> DrawableHandler { get; set; }

        #endregion

        #region Methods

        public static void Initialize()
        {
            var descriptors = new Dictionary<string, ResourceBindingParserHandler.ResourceDescriptor>();
            ICollection<KeyValuePair<string, ResourceBindingParserHandler.ResourceDescriptor>> collection = descriptors;
            collection.Add(CreateDescriptor(nameof(Color), (context, i) => new Color(ColorHandler?.Invoke(context, i) ?? context.GetColor(i))));
            collection.Add(CreateDescriptor(nameof(Drawable), null));
            collection.Add(CreateDescriptor(nameof(Dimen), (context, i) => context.Resources.GetDimension(i)));
            collection.Add(CreateDescriptor(nameof(Bool), (context, i) => context.Resources.GetBoolean(i)));
            collection.Add(CreateDescriptor(nameof(Id), (context, i) => i));
            collection.Add(CreateDescriptor(nameof(Integer), (context, i) => context.Resources.GetInteger(i)));
            collection.Add(CreateDescriptor(nameof(String), (context, i) => context.GetString(i)));
            var handler = new ResourceBindingParserHandler(descriptors);
            BindingServiceProvider.BindingProvider.Parser.Handlers.Add(handler);
        }

        public static Color Color(string name, object target = null)
        {
            var context = GetContext(target);
            var id = context.Resources.GetIdentifier(name, "color", context.PackageName);
            if (ColorHandler == null)
                return new Color(context.GetColor(id));
            return new Color(ColorHandler(context, id));
        }

        public static Drawable Drawable(string name, object target = null)
        {
            var context = GetContext(target);
            var id = context.Resources.GetIdentifier(name, "drawable", context.PackageName);
            if (id == 0)
                id = context.Resources.GetIdentifier(name, "mipmap", context.PackageName);
            if (DrawableHandler == null)
                return context.GetDrawable(id);
            return DrawableHandler(context, id);
        }

        public static float Dimen(string name, object target = null)
        {
            var context = GetContext(target);
            var id = context.Resources.GetIdentifier(name, "dimen", context.PackageName);
            return context.Resources.GetDimension(id);
        }

        public static bool Bool(string name, object target = null)
        {
            var context = GetContext(target);
            var id = context.Resources.GetIdentifier(name, "bool", context.PackageName);
            return context.Resources.GetBoolean(id);
        }

        public static int Id(string name, object target = null)
        {
            var context = GetContext(target);
            return context.Resources.GetIdentifier(name, "id", context.PackageName);
        }

        public static int Integer(string name, object target = null)
        {
            var context = GetContext(target);
            var id = context.Resources.GetIdentifier(name, "integer", context.PackageName);
            return context.Resources.GetInteger(id);
        }

        public static string String(string name, object target = null)
        {
            var context = GetContext(target);
            var id = context.Resources.GetIdentifier(name, "string", context.PackageName);
            return context.Resources.GetString(id);
        }

        internal static Context GetContext(object target)
        {
            return target == null ? (PlatformExtensions.CurrentActivity ?? Application.Context) : AttachedMembersRegistration.GetContextFromItem(target);
        }

        private static KeyValuePair<string, ResourceBindingParserHandler.ResourceDescriptor> CreateDescriptor(string method, Func<Context, int, object> getResource)
        {
            var type = method.ToLower();
            var resourceDescriptor = new ResourceBindingParserHandler.ResourceDescriptor(typeof(BindingResourceExtensions), method,
                getResource == null
                    ? (Func<Context, string, object>)null
                    : (ctx, s) =>
                    {
                        var id = ctx.Resources.GetIdentifier(s, type, ctx.PackageName);
                        return getResource(ctx, id);
                    });
            return new KeyValuePair<string, ResourceBindingParserHandler.ResourceDescriptor>(type, resourceDescriptor);
        }

        #endregion
    }
}