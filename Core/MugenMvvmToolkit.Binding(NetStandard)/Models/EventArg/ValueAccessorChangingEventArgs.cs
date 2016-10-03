#region Copyright

// ****************************************************************************
// <copyright file="ValueAccessorChangingEventArgs.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class ValueAccessorChangingEventArgs : ValueAccessorChangedEventArgs
    {
        #region Constructors

        public ValueAccessorChangingEventArgs(IDataContext context, object penultimateValue,
            IBindingMemberInfo lastMember, object oldValue, object newValue)
            : base(context, penultimateValue, lastMember, oldValue, newValue)
        {
        }

        #endregion

        #region Properties

        public bool Cancel { get; set; }

        [CanBeNull]
        public new object NewValue
        {
            get { return base.NewValue; }
            set { base.NewValue = value; }
        }

        #endregion
    }
}
