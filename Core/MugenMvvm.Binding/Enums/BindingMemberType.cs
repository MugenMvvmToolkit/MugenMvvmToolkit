﻿using MugenMvvm.Attributes;
using MugenMvvm.Enums;

namespace MugenMvvm.Binding.Enums
{
    public class BindingMemberType : EnumBase<BindingMemberType, string>
    {
        #region Fields

        public static readonly BindingMemberType Property = new BindingMemberType(nameof(Property));
        public static readonly BindingMemberType Field = new BindingMemberType(nameof(Field));
        public static readonly BindingMemberType Event = new BindingMemberType(nameof(Event));

        #endregion

        #region Constructors

        public BindingMemberType(string value) : base(value)
        {
        }

        [Preserve(Conditional = true)]
        protected BindingMemberType()
        {
        }

        #endregion
    }
}