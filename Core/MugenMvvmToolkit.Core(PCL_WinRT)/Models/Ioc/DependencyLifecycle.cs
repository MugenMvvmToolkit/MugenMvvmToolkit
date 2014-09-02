#region Copyright
// ****************************************************************************
// <copyright file="DependencyLifecycle.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
    /// <summary>
    ///     Represent the various lifecycles available for coponents configured in the container
    /// </summary>
    public class DependencyLifecycle : StringConstantBase<DependencyLifecycle>
    {
        #region Fields

        /// <summary>
        ///     Singleton scope
        /// </summary>
        public static readonly DependencyLifecycle SingleInstance;

        /// <summary>
        ///     Transient scope.
        /// </summary>
        public static readonly DependencyLifecycle TransientInstance;

        #endregion

        #region Constructors

        static DependencyLifecycle()
        {
            SingleInstance = new DependencyLifecycle("Single");
            TransientInstance = new DependencyLifecycle("Transient");
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DependencyLifecycle" /> class.
        /// </summary>
        public DependencyLifecycle(string id)
            : base(id)
        {
        }

        #endregion
    }
}