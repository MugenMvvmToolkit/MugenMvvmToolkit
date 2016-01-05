#region Copyright

// ****************************************************************************
// <copyright file="DisableEqualityCheckingBehavior.cs">
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
using System.Linq;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    //On some platforms like Android and iOS poor performance when reading properties because it is necessary to convert a native object to a NET object
    //in this caset to reduce the number of reading disabling the equality checking
    public sealed class DisableEqualityCheckingBehavior : IBindingBehavior
    {
        #region Fields

        private static readonly DisableEqualityCheckingBehavior TargetFalse;
        private static readonly DisableEqualityCheckingBehavior TargetTrue;
        private static readonly DisableEqualityCheckingBehavior SourceFalse;
        private static readonly DisableEqualityCheckingBehavior SourceTrue;
        public static readonly DisableEqualityCheckingBehavior TargetTrueNotTwoWay;

        private static readonly Guid IdTarget = new Guid("4216555D-608F-43E4-8228-77092E41FC7B");
        private static readonly Guid IdSource = new Guid("4F20DEF3-64B1-4639-BA84-E4C0C0A28CF6");
        private readonly bool _isTarget;
        private readonly bool _value;
        private readonly bool _checkTwoWay;

        #endregion

        #region Constructors

        static DisableEqualityCheckingBehavior()
        {
            TargetFalse = new DisableEqualityCheckingBehavior(true, false, false);
            TargetTrue = new DisableEqualityCheckingBehavior(true, true, false);
            SourceFalse = new DisableEqualityCheckingBehavior(false, false, false);
            SourceTrue = new DisableEqualityCheckingBehavior(false, true, false);
            TargetTrueNotTwoWay = new DisableEqualityCheckingBehavior(true, true, true);
        }

        private DisableEqualityCheckingBehavior(bool isTarget, bool value, bool checkTwoWay)
        {
            _isTarget = isTarget;
            _value = value;
            _checkTwoWay = checkTwoWay;
        }

        #endregion

        #region Properties

        public Guid Id => _isTarget ? IdTarget : IdSource;

        public int Priority => BindingModeBase.DefaultPriority - 1;

        #endregion

        #region Methods

        public static IBindingBehavior GetTargetBehavior(bool value)
        {
            if (value)
                return TargetTrue;
            return TargetFalse;
        }

        public static IBindingBehavior GetSourceBehavior(bool value)
        {
            if (value)
                return SourceTrue;
            return SourceFalse;
        }

        #endregion

        #region Implementation of interfaces

        public bool Attach(IDataBinding binding)
        {
            if (_checkTwoWay && binding.Behaviors.Any(behavior => behavior is TwoWayBindingMode))
                return false;
            if (_isTarget)
                binding.TargetAccessor.DisableEqualityChecking = _value;
            else
                binding.SourceAccessor.DisableEqualityChecking = _value;
            return false;
        }

        public void Detach(IDataBinding binding)
        {
        }

        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}