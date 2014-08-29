#region Copyright
// ****************************************************************************
// <copyright file="BindingModeBase.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    /// <summary>
    ///     Represents the base class that describes the direction of the data flow in a binding.
    /// </summary>
    public abstract class BindingModeBase : BindingBehaviorBase
    {
        #region Fields

        /// <summary>
        ///     Gets the id of binding mode behavior.
        /// </summary>
        public static readonly Guid IdBindingMode;

        private bool _isSourceUpdating;
        private bool _isTargetUpdating;

        #endregion

        #region Constructors

        static BindingModeBase()
        {
            IdBindingMode = new Guid("BA46EB20-298C-49D8-AED8-8A057A7D0D06");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Subscribes the target source.
        /// </summary>
        protected void SubscribeTarget()
        {
            Binding.TargetAccessor.Source.ValueChanged += TargetOnValueChanged;
        }

        /// <summary>
        ///     Unsubscribes the target source.
        /// </summary>
        protected void UnsubscribeTarget()
        {
            Binding.TargetAccessor.Source.ValueChanged -= TargetOnValueChanged;
        }

        /// <summary>
        ///     Subscribes the sources.
        /// </summary>
        protected void SubscribeSources()
        {
            SubscribeSources(SourceOnValueChanged);
        }

        /// <summary>
        ///     Unsubscribes the sources.
        /// </summary>
        protected void UnsubscribeSources()
        {
            UnsubscribeSources(SourceOnValueChanged);
        }

        /// <summary>
        ///     Subscribes the sources.
        /// </summary>
        protected void SubscribeSources(EventHandler<IBindingSource, ValueChangedEventArgs> handler)
        {
            SubscribeInternal(Binding.SourceAccessor, handler, true);
        }

        /// <summary>
        ///     Unsubscribes the sources.
        /// </summary>
        protected void UnsubscribeSources(EventHandler<IBindingSource, ValueChangedEventArgs> handler)
        {
            SubscribeInternal(Binding.SourceAccessor, handler, false);
        }

        /// <summary>
        ///     Updates the target binding when source value changed.
        /// </summary>
        private void SourceOnValueChanged(IBindingSource sender, ValueChangedEventArgs valueChangedEventArg)
        {
            if (_isSourceUpdating)
                return;
            try
            {
                _isSourceUpdating = true;
                Binding.UpdateTarget();
            }
            finally
            {
                _isSourceUpdating = false;
            }
        }

        /// <summary>
        ///     Updates the source binding when target value changed.
        /// </summary>
        private void TargetOnValueChanged(IBindingSource sender, ValueChangedEventArgs valueChangedEventArg)
        {
            if (_isTargetUpdating)
                return;
            try
            {
                _isTargetUpdating = true;
                Binding.UpdateSource();
            }
            finally
            {
                _isTargetUpdating = false;
            }
        }

        private static void SubscribeInternal(IBindingSourceAccessor accessor, EventHandler<IBindingSource, ValueChangedEventArgs> handler, bool subscribe)
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

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public override sealed Guid Id
        {
            get { return IdBindingMode; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public override sealed int Priority
        {
            get { return int.MinValue; }
        }

        #endregion
    }
}