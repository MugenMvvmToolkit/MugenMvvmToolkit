#region Copyright

// ****************************************************************************
// <copyright file="DependencyLifecycle.cs">
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

namespace MugenMvvmToolkit.Models.IoC
{
    public class DependencyLifecycle : StringConstantBase<DependencyLifecycle>
    {
        #region Fields

        public static readonly DependencyLifecycle SingleInstance;

        public static readonly DependencyLifecycle TransientInstance;

        #endregion

        #region Constructors

        static DependencyLifecycle()
        {
            SingleInstance = new DependencyLifecycle("Single");
            TransientInstance = new DependencyLifecycle("Transient");
        }

        public DependencyLifecycle(string id)
            : base(id)
        {
        }

        #endregion
    }
}
