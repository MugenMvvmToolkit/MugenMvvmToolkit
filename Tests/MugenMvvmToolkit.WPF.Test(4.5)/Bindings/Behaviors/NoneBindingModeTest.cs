#region Copyright

// ****************************************************************************
// <copyright file="NoneBindingModeTest.cs">
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
    public class NoneBindingModeTest : BindingModeTestBase<NoneBindingMode>
    {
        #region Overrides of BindingModeTestBase<NoneBindingMode>

        protected override NoneBindingMode CreateBehavior()
        {
            return NoneBindingMode.Instance;
        }

        [Ignore]
        public override void BehaviorCanBeAttachedOnlyOnce()
        {
        }

        [Ignore]
        public override void ModeShouldUpdateSourceOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldUpdateTargetOnAttach()
        {
        }

        [Ignore]
        public override void ModeShouldListenTargetChange()
        {
        }

        [Ignore]
        public override void ModeShouldListenSourceChange()
        {
        }

        #endregion
    }
}
