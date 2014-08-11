#region Copyright
// ****************************************************************************
// <copyright file="BindingResourceResolverEx.cs">
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

#if WINDOWSCOMMON || NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the dynamic resource resolver supports {StaticResources}.
    /// </summary>
    public class BindingResourceResolverEx : BindingResourceResolver
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceResolverEx" /> class.
        /// </summary>
        public BindingResourceResolverEx()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BindingResourceResolverEx" /> class.
        /// </summary>
        public BindingResourceResolverEx([NotNull] BindingResourceResolver resolver)
            : base(resolver)
        {
        }

        #endregion

        #region Overrides of BindingResourceResolver

        /// <summary>
        ///     Gets an instance of <see cref="IBindingValueConverter" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingValueConverter" />.</returns>
        public override IBindingValueConverter ResolveConverter(string name, bool throwOnError)
        {
            var result = base.ResolveConverter(name, false);
            if (result != null)
                return result;

            var application = Application.Current;
            if (application != null)
            {
                var item = TryFindResource(application, name);
                if (item != null)
                {
                    var valueConverter = item as IBindingValueConverter;
                    if (valueConverter != null)
                        return valueConverter;
                    var converter = item as IValueConverter;
                    if (converter != null)
                        return new ValueConverterWrapper(converter.Convert, converter.ConvertBack);
                }
            }
            if (throwOnError)
                throw BindingExceptionManager.CannotResolveInstanceByName(this, "converter", name);
            return null;
        }

        /// <summary>
        ///     Gets an instance of <see cref="IBindingResourceObject" /> by the specified name.
        /// </summary>
        /// <param name="name">The specified name.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the type cannot be found; false to return null. Specifying
        ///     false also suppresses some other exception conditions, but not all of them.
        /// </param>
        /// <returns>An instance of <see cref="IBindingResourceMethod" />.</returns>
        public override IBindingResourceObject ResolveObject(string name, bool throwOnError)
        {
            var result = base.ResolveObject(name, false);
            if (result != null)
                return result;
            var application = Application.Current;
            if (application != null)
            {
                var item = TryFindResource(application, name);
                if (item != null)
                    return new BindingResourceObject(item);
            }
            if (throwOnError)
                throw BindingExceptionManager.CannotResolveInstanceByName(this, "dynamic object", name);
            return null;
        }

        #endregion

        #region Methods

        private static object TryFindResource(Application application, string key)
        {
#if WPF
            return application.TryFindResource(key);
#elif WINDOWSCOMMON || NETFX_CORE
            object value;
            application.Resources.TryGetValue(key, out value);
            return value;
#elif SILVERLIGHT
            if (application.Resources.Contains(key))
                return application.Resources[key];
            return null;
#endif
        }

        #endregion
    }
}