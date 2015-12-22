#region Copyright

// ****************************************************************************
// <copyright file="MultiBindingSourceAccessor.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Accessors
{
    public sealed class MultiBindingSourceAccessor : BindingSourceAccessorBase
    {
        #region Fields

        private readonly IObserver[] _sources;
        private Func<IDataContext, IList<object>, object> _formatExpression;
        private BindingActionValue _getSourceMemberValue;

        #endregion

        #region Constructors

        public MultiBindingSourceAccessor([NotNull] IObserver[] sources,
            [NotNull] Func<IDataContext, IList<object>, object> formatExpression, [NotNull] IDataContext context)
            : base(context, false)
        {
            Should.NotBeNull(sources, nameof(sources));
            Should.NotBeNull(formatExpression, nameof(formatExpression));
            _sources = sources;
            _formatExpression = formatExpression;
        }

        #endregion

        #region Overrides of BindingSourceAccessorBase

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override bool DisableEqualityChecking
        {
            get { return false; }
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        public override IList<IObserver> Sources => _sources;

        public override void Dispose()
        {
            _formatExpression = null;
            _getSourceMemberValue = null;
            for (int index = 0; index < _sources.Length; index++)
                _sources[index].Dispose();
            base.Dispose();
        }

        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging
        {
            add { }
            remove { }
        }

        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged
        {
            add { }
            remove { }
        }

        protected override object GetRawValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError)
        {
            var values = new object[_sources.Length];
            for (int i = 0; i < _sources.Length; i++)
            {
                IBindingPathMembers members = _sources[i].GetPathMembers(true);
                object value = members.LastMember.GetValue(members.PenultimateValue, null);
                if (value.IsDoNothing())
                    return BindingConstants.DoNothing;
                if (value.IsUnsetValue())
                    return BindingConstants.UnsetValue;
                values[i] = value;
            }
            return _formatExpression(context, values);
        }

        protected override bool SetValueInternal(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError)
        {
            //NOTE By default multibinding doesn't support update source operation.
            return false;
        }

        protected override object GetValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError)
        {
            if (BindingMemberType.Event.EqualsWithoutNullCheck(targetMember.MemberType))
            {
                if (_getSourceMemberValue == null)
                    _getSourceMemberValue = new BindingActionValue(this, BindingMemberInfo.MultiBindingSourceAccessorMember);
                return _getSourceMemberValue;
            }
            return base.GetValueInternal(targetMember, context, throwOnError);
        }

        #endregion

        #region Methods

        internal object GetRawValueInternal(IDataContext context)
        {
            return GetRawValueInternal(null, context ?? DataContext.Empty, true);
        }

        #endregion
    }
}
