#region Copyright

// ****************************************************************************
// <copyright file="DefaultValueOnExceptionBehavior.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class DefaultValueOnExceptionBehavior : BindingBehaviorBase
    {
        #region Fields

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

        public object Value => _value;

        #endregion

        #region Methods

        #region Methods

        private void OnBindingException(object sender, BindingEventArgs args)
        {
            if (args.Exception == null)
                return;
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

        private void SetDefaultValue(IObserver source)
        {
            var pathMembers = source.GetPathMembers(false);
            if (pathMembers.AllMembersAvailable)
            {
                object value = _value;
                if (!pathMembers.LastMember.Type.IsInstanceOfType(value))
                    value = pathMembers.LastMember.Type.GetDefaultValue();
                pathMembers.LastMember.SetSingleValue(pathMembers.PenultimateValue, value);
            }
        }

        #endregion

        #endregion

        #region Overrides of BindingBehaviorBase

        public override Guid Id => IdDefaultValuesOnExceptionBehavior;

        public override int Priority => 0;

        protected override bool OnAttached()
        {
            Binding.BindingUpdated += OnBindingException;
            return true;
        }

        protected override void OnDetached()
        {
            Binding.BindingUpdated -= OnBindingException;
        }

        protected override IBindingBehavior CloneInternal()
        {
            return new DefaultValueOnExceptionBehavior(_value);
        }

        #endregion
    }
}
