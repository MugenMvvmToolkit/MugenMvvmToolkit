#region Copyright

// ****************************************************************************
// <copyright file="LayoutInflaterResult.cs">
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
using Android.Views;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding;

namespace MugenMvvmToolkit.Android.Models
{
    public class LayoutInflaterResult : List<LayoutInflaterResult.LayoutInflaterResultItem>
    {
        #region Nested types

        public struct LayoutInflaterResultItem
        {
            public readonly object Target;
            public readonly string Bind;
            public readonly Action<object> PostAction;

            public LayoutInflaterResultItem(object target, string bind, Action<object> postAction)
            {
                Target = target;
                Bind = bind;
                PostAction = postAction;
            }
        }

        #endregion

        #region Constructors

        protected internal LayoutInflaterResult()
        {
        }

        #endregion

        #region Properties

        public View View { get; protected internal set; }

        #endregion

        #region Methods

        public void AddBindingInfo(object target, string bind, Action<object> postAction)
        {
            if (!string.IsNullOrEmpty(bind) || postAction != null)
                Insert(0, new LayoutInflaterResultItem(target, bind, postAction));
        }

        public void ApplyBindings()
        {
            for (var index = 0; index < Count; index++)
            {
                var item = this[index];
                if (!string.IsNullOrEmpty(item.Bind))
                {
                    var manualBindings = item.Target as IManualBindings;
                    if (manualBindings == null)
                        BindingServiceProvider.BindingProvider.CreateBindingsFromString(item.Target, item.Bind);
                    else
                        manualBindings.SetBindings(item.Bind);
                }
                item.PostAction?.Invoke(item.Target);
            }
        }

        #endregion
    }
}