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
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Accessors
{
    /// <summary>
    ///     Represents the accessor for the multi binding source.
    /// </summary>
    public sealed class MultiBindingSourceAccessor : BindingSourceAccessorBase
    {
        #region Fields

        private IBindingSource[] _sources;
        private Func<IDataContext, IList<object>, object> _formatExpression;
        private BindingActionValue _getSourceMemberValue;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultiBindingSourceAccessor" /> class.
        /// </summary>
        public MultiBindingSourceAccessor([NotNull] IBindingSource[] sources,
            [NotNull] Func<IDataContext, IList<object>, object> formatExpression, [NotNull] IDataContext context)
            : base(context, false)
        {
            Should.NotBeNull(sources, "sources");
            Should.NotBeNull(formatExpression, "formatExpression");
            _sources = sources;
            _formatExpression = formatExpression;
        }

        #endregion

        #region Overrides of BindingSourceAccessorBase

        /// <summary>
        ///     Gets a value indicating whether the member can be read.
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        ///     Gets a value indicating whether the property can be written to.
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the underlying sources.
        /// </summary>
        public override IList<IBindingSource> Sources
        {
            get { return _sources; }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _formatExpression = null;
            _getSourceMemberValue = null;
            for (int index = 0; index < _sources.Length; index++)
                _sources[index].Dispose();
            _sources = Empty.Array<IBindingSource>();
            base.Dispose();
        }

        /// <summary>
        ///     Occurs before the value changes.
        /// </summary>
        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangingEventArgs> ValueChanging
        {
            add { }
            remove { }
        }

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        public override event EventHandler<IBindingSourceAccessor, ValueAccessorChangedEventArgs> ValueChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        ///     Gets the raw value from source.
        /// </summary>
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

        /// <summary>
        ///     Sets the source value.
        /// </summary>
        /// <param name="targetAccessor">The specified accessor to get value.</param>
        /// <param name="context">The specified operation context.</param>
        /// <param name="throwOnError">
        ///     true to throw an exception if the value cannot be set.
        /// </param>
        protected override bool SetValueInternal(IBindingSourceAccessor targetAccessor, IDataContext context,
            bool throwOnError)
        {
            //NOTE By default multibinding doesn't support update source operation.   
            return false;
        }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        protected override object GetValueInternal(IBindingMemberInfo targetMember, IDataContext context,
            bool throwOnError)
        {
            if (BindingMemberType.Event.Equals(targetMember.MemberType))
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