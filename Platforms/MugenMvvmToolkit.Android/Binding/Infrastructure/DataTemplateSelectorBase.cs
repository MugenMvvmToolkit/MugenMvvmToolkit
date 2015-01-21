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

using System.Threading;
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
        private static readonly bool SetAttachedParentMemberField;
        private BindingSet<TTemplate, TSource> _bindingSet;

        #endregion

        #region Constructors

        static DataTemplateSelectorBase()
        {
#if ANDROID
            SetAttachedParentMemberField = !typeof(Android.Views.View).IsAssignableFrom(typeof(TTemplate));
#elif TOUCH
            SetAttachedParentMemberField = !typeof(UIKit.UIView).IsAssignableFrom(typeof(TTemplate));
#elif WINFORMS
            SetAttachedParentMemberField =
                !typeof(System.Windows.Forms.ToolStripItem).IsAssignableFrom(typeof(TTemplate)) &&
                !typeof(System.Windows.Forms.Control).IsAssignableFrom(typeof(TTemplate));
#endif
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
            if (template != null)
            {
                if (SupportInitialize)
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
                if (SetAttachedParentMember && container != null &&
                    BindingExtensions.AttachedParentMember.GetValue(template, Empty.Array<object>()) == null)
                    BindingExtensions.AttachedParentMember.SetValue(template, container);
            }
            return template;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Specifies that this template supports initialize method default is <c>true</c>.
        /// </summary>
        protected virtual bool SupportInitialize
        {
            get { return true; }
        }

        /// <summary>
        ///     If <c>true</c> the container will be set as parent for a template; default is <c>true</c>.
        /// </summary>
        protected virtual bool SetAttachedParentMember
        {
            get { return SetAttachedParentMemberField; }
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

        #endregion
    }
}