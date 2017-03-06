#region Copyright

// ****************************************************************************
// <copyright file="BindingModeBase.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public abstract class BindingModeBase : BindingBehaviorBase
    {
        #region Fields

        public static readonly Guid IdBindingMode;
        public const int DefaultPriority = int.MinValue + 1000;

        #endregion

        #region Constructors

        static BindingModeBase()
        {
            IdBindingMode = new Guid("BA46EB20-298C-49D8-AED8-8A057A7D0D06");
        }

        #endregion

        #region Methods

        protected void SubscribeTarget()
        {
            if (Binding != null)
                Binding.TargetAccessor.Source.ValueChanged += TargetOnValueChanged;
        }

        protected void UnsubscribeTarget()
        {
            if (Binding != null)
                Binding.TargetAccessor.Source.ValueChanged -= TargetOnValueChanged;
        }

        protected void SubscribeSources()
        {
            SubscribeSources(SourceOnValueChanged);
        }

        protected void UnsubscribeSources()
        {
            UnsubscribeSources(SourceOnValueChanged);
        }

        protected void SubscribeSources(EventHandler<IObserver, ValueChangedEventArgs> handler)
        {
            if (Binding != null)
                SubscribeInternal(Binding.SourceAccessor, handler, true);
        }

        protected void UnsubscribeSources(EventHandler<IObserver, ValueChangedEventArgs> handler)
        {
            if (Binding != null)
                SubscribeInternal(Binding.SourceAccessor, handler, false);
        }

        private void SourceOnValueChanged(IObserver sender, ValueChangedEventArgs args)
        {
            Binding.UpdateTarget();
        }

        private void TargetOnValueChanged(IObserver sender, ValueChangedEventArgs args)
        {
            Binding.UpdateSource();
        }

        private static void SubscribeInternal(IBindingSourceAccessor accessor, EventHandler<IObserver, ValueChangedEventArgs> handler, bool subscribe)
        {
            var singleSourceAccessor = accessor as ISingleBindingSourceAccessor;
            if (singleSourceAccessor == null)
            {
                var sources = accessor.Sources;
                if (subscribe)
                {
                    for (int index = 0; index < sources.Count; index++)
                        sources[index].ValueChanged += handler;
                }
                else
                {
                    for (int index = 0; index < sources.Count; index++)
                        sources[index].ValueChanged -= handler;
                }
            }
            else
            {
                if (subscribe)
                    singleSourceAccessor.Source.ValueChanged += handler;
                else
                    singleSourceAccessor.Source.ValueChanged -= handler;
            }
        }

        #endregion

        #region Overrides of BindingBehaviorBase

        public sealed override Guid Id => IdBindingMode;

        public sealed override int Priority => DefaultPriority;

        #endregion
    }
}
