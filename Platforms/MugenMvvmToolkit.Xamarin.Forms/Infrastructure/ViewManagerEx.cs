#region Copyright

// ****************************************************************************
// <copyright file="ViewManagerEx.cs">
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

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Infrastructure
{
    public class ViewManagerEx : ViewManager
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ViewManagerEx" /> class.
        /// </summary>
        public ViewManagerEx([NotNull] IThreadManager threadManager, [NotNull] IViewMappingProvider viewMappingProvider,
            [NotNull] IWrapperManager wrapperManager)
            : base(threadManager, viewMappingProvider, wrapperManager)
        {
        }

        #endregion

        #region Overrides of ViewManager

        /// <summary>
        ///     Clears view in the specified view-model
        /// </summary>
        /// <param name="viewModel">The specified view model.</param>
        /// <param name="view">The specified view.</param>
        /// <param name="context">The specified <see cref="IDataContext" />, if any.</param>
        protected override void CleanupView(IViewModel viewModel, object view, IDataContext context)
        {
            base.CleanupView(viewModel, view, context);
            ClearBindings(view as BindableObject);
        }

        #endregion

        #region Methods

        private static void ClearBindings(BindableObject item)
        {
            if (item == null)
                return;
            Type type = item.GetType();
            var attribute = type
                .GetTypeInfo()
                .GetCustomAttribute<ContentPropertyAttribute>(true);
            if (attribute != null)
            {
                IBindingMemberInfo bindingMember = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(type, attribute.Name, true, false);
                if (bindingMember != null)
                {
                    object content = bindingMember.GetValue(item, null);
                    var enumerable = content as IEnumerable;
                    if (enumerable == null)
                        ClearBindings(content as BindableObject);
                    else
                    {
                        foreach (var child in enumerable.OfType<BindableObject>())
                            ClearBindings(child);
                    }
                }
            }
            try
            {
                BindingServiceProvider.BindingManager.ClearBindings(item);
                ServiceProvider.AttachedValueProvider.Clear(item);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        #endregion
    }
}