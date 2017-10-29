#region Copyright

// ****************************************************************************
// <copyright file="MugenIocAdapterTest.cs">
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
using MugenMvvmToolkit.Infrastructure;

namespace MugenMvvmToolkit.Test.Ioc
{
    [TestClass]
    public class MugenContainerTest : IocContainerTestBase<MugenContainer>
    {
        #region Constructors

        public MugenContainerTest()
            : base(() => new MugenContainer())
        {
        }

        #endregion

        #region Methods

        [TestInitialize]
        public void SetUp()
        {
            ToolkitServiceProvider.ReflectionManager = new ExpressionReflectionManager();
        }

        #endregion
    }
}
