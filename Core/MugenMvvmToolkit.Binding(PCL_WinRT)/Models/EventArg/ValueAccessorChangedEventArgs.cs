#region Copyright

// ****************************************************************************
// <copyright file="ValueAccessorChangedEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class ValueAccessorChangedEventArgs : ValueChangedEventArgs<object>
    {
        #region Fields

        private readonly IDataContext _context;
        private readonly IBindingMemberInfo _lastMember;
        private readonly object _penultimateValue;

        #endregion

        #region Constructors

        public ValueAccessorChangedEventArgs(IDataContext context, object penultimateValue,
            IBindingMemberInfo lastMember, object oldValue, object newValue)
            : base(oldValue, newValue)
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(lastMember, nameof(lastMember));
            _context = context;
            _penultimateValue = penultimateValue;
            _lastMember = lastMember;
        }

        #endregion

        #region Properties

        [NotNull]
        public IDataContext Context
        {
            get { return _context; }
        }

        [CanBeNull]
        public object PenultimateValue
        {
            get { return _penultimateValue; }
        }

        [NotNull]
        public IBindingMemberInfo LastMember
        {
            get { return _lastMember; }
        }

        #endregion
    }
}
