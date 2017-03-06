#region Copyright

// ****************************************************************************
// <copyright file="DefaultValueOnExceptionBehaviorTest.cs">
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
    public class DefaultValueOnExceptionBehaviorTest : BindingBehaviorTestBase<DefaultValueOnExceptionBehavior>
    {
        #region Overrides of BindingBehaviorTestBase<DefaultValueOnExceptionBehavior>

        [Ignore]
        public override void BehaviorCanBeAttachedRepeatedly()
        {
        }

        protected override DefaultValueOnExceptionBehavior CreateBehavior()
        {
            return new DefaultValueOnExceptionBehavior(null);
        }

        #endregion
    }
}
