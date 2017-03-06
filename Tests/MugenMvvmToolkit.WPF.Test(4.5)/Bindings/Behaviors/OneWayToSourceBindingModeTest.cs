#region Copyright

// ****************************************************************************
// <copyright file="OneWayToSourceBindingModeTest.cs">
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
    public class OneWayToSourceBindingModeTest : BindingModeTestBase<OneWayToSourceBindingMode>
    {
        #region Overrides of BindingModeTestBase<OneWayToSourceBindingMode>

        [Ignore]
        public override void BehaviorCanBeAttachedRepeatedly()
        {
        }

        [Ignore]
        public override void ModeShouldDoNothingOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldUpdateTargetOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldListenSourceChange()
        {
        }

        [Ignore]
        public override void ModeShouldNotListenAnySourceChange()
        {
        }

        protected override OneWayToSourceBindingMode CreateBehavior()
        {
            return new OneWayToSourceBindingMode();
        }

        #endregion
    }
}
