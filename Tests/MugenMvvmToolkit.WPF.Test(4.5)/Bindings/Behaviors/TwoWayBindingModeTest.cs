#region Copyright

// ****************************************************************************
// <copyright file="TwoWayBindingModeTest.cs">
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

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public class TwoWayBindingModeTest : BindingModeTestBase<TwoWayBindingMode>
    {
        #region Overrides of BindingModeTestBase<TwoWayBindingMode>

        [Ignore]
        public override void BehaviorCanBeAttachedRepeatedly()
        {

        }

        [Ignore]
        public override void ModeShouldDoNothingOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldUpdateSourceOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldNotListenAnySourceChange()
        {
        }

        protected override TwoWayBindingMode CreateBehavior()
        {
            return new TwoWayBindingMode();
        }

        #endregion
    }
}
