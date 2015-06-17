#region Copyright

// ****************************************************************************
// <copyright file="DataTemplateSelectorBase.cs">
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
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     DataTemplateSelector allows the app writer to provide custom template selection logic.
    /// </summary>
    public abstract class DataTemplateSelectorBase<TSource, TTemplate> : IDataTemplateSelector where TTemplate : class
    {
        #region Fields

        // ReSharper disable once StaticFieldInGenericType
        private static readonly bool IsTemplateObjectType;
        private BindingSet<TTemplate, TSource> _bindingSet;

        #endregion

        #region Constructors

        static DataTemplateSelectorBase()
        {
            IsTemplateObjectType = typeof(TTemplate) == typeof(object);
        }

        #endregion

        #region Implementation of IDataTemplateSelector

        /// <summary>
        ///     Override this method to return an app specific template.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>
        ///     An app-specific template to apply, or null.
        /// </returns>
        public object SelectTemplate(object item, object container)
        {
            TTemplate template = SelectTemplate((TSource)item, container);
            if (template != null && CanInitialize(template, container))
            {
                if (_bindingSet == null)
                    Interlocked.CompareExchange(ref _bindingSet, new BindingSet<TTemplate, TSource>(template), null);
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(_bindingSet, ref lockTaken);
                    _bindingSet.Target = template;
                    Initialize(template, _bindingSet);
                    _bindingSet.Apply();
                }
                finally
                {
                    _bindingSet.Target = null;
                    if (lockTaken)
                        Monitor.Exit(_bindingSet);
                }
            }
            return template;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Override this method to return an app specific template.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>
        ///     An app-specific template to apply, or null.
        /// </returns>
        protected abstract TTemplate SelectTemplate(TSource item, object container);

        /// <summary>
        ///     Initializes the specified template.
        /// </summary>
        protected abstract void Initialize(TTemplate template, BindingSet<TTemplate, TSource> bindingSet);

        /// <summary>
        ///     Checks to see whether the template selector can initialize template.
        /// </summary>
        protected virtual bool CanInitialize([NotNull] TTemplate template, [NotNull] object container)
        {
            return !IsTemplateObjectType || !(template is ValueType);
        }

        #endregion
    }
}