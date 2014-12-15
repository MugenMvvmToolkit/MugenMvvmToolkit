#region Copyright
// ****************************************************************************
// <copyright file="DefaultValueOnExceptionBehavior.cs">
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
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class DefaultValueOnExceptionBehavior : BindingBehaviorBase
    {
        #region Fields

        /// <summary>
        ///     Gets the id of behavior.
        /// </summary>
        public static readonly Guid IdDefaultValuesOnExceptionBehavior;

        private readonly object _value;

        #endregion

        #region Constructors

        static DefaultValueOnExceptionBehavior()
        {
            IdDefaultValuesOnExceptionBehavior = new Guid("BB266907-520E-4461-9D95-A549326049DA");
        }

        public DefaultValueOnExceptionBehavior(object value)
        {
            _value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the default value to set.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        #endregion

        #region Methods

        #region Methods

        private void OnBindingException(object sender, BindingExceptionEventArgs args)
        {
            var dataBinding = sender as IDataBinding;
            if (dataBinding != null && args.Action == BindingAction.UpdateSource)
                SetDefaultValue(dataBinding);
        }

        private void SetDefaultValue(IDataBinding dataBinding)
        {
            var singleAccessor = dataBinding.SourceAccessor as ISingleBindingSourceAccessor;
            if (singleAccessor == null)
            {
                var sources = dataBinding.SourceAccessor.Sources;
                for (int index = 0; index < sources.Count; index++)
                    SetDefaultValue(sources[index]);
            }
            else
                SetDefaultValue(singleAccessor.Source);
        }

        private void SetDefaultValue(IBindingSource source)
        {
            var pathMembers = source.GetPathMembers(false);
            if (pathMembers.AllMembersAvailable)
            {
                object value = _value;
                if (!pathMembers.LastMember.Type.IsInstanceOfType(value))
                    value = pathMembers.LastMember.Type.GetDefaultValue();
                pathMembers.LastMember.SetValue(pathMembers.PenultimateValue, new[] { value });
            }
        }

        #endregion

        #endregion

        #region Overrides of BindingBehaviorBase

        /// <summary>
        ///     Gets the id of behavior. Each <see cref="IDataBinding" /> can have only one instance with the same id.
        /// </summary>
        public override Guid Id
        {
            get { return IdDefaultValuesOnExceptionBehavior; }
        }

        /// <summary>
        ///     Gets the behavior priority.
        /// </summary>
        public override int Priority
        {
            get { return 0; }
        }

        /// <summary>
        ///     Attaches to the specified binding.
        /// </summary>
        protected override bool OnAttached()
        {
            Binding.BindingException += OnBindingException;
            return true;
        }

        /// <summary>
        ///     Detaches this instance from its associated binding.
        /// </summary>
        protected override void OnDetached()
        {
            Binding.BindingException -= OnBindingException;
        }

        /// <summary>
        ///     Creates a new binding behavior that is a copy of the current instance.
        /// </summary>
        protected override IBindingBehavior CloneInternal()
        {
            return new DefaultValueOnExceptionBehavior(_value);
        }

        #endregion
    }
}