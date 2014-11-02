#region Copyright
// ****************************************************************************
// <copyright file="PlatformType.cs">
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
namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the platform type.
    /// </summary>
    public class PlatformType : StringConstantBase<PlatformType>
    {
        #region Fields

        public static readonly PlatformType Android;

        public static readonly PlatformType iOS;

        public static readonly PlatformType Silverlight;

        public static readonly PlatformType WinPhone;

        public static readonly PlatformType WinForms;

        public static readonly PlatformType WinRT;

        public static readonly PlatformType WPF;

        public static readonly PlatformType Unknown;

        public static readonly PlatformType UnitTest;

        #endregion

        #region Constructors

        static PlatformType()
        {
            Android = new PlatformType("Android");
            iOS = new PlatformType("iOS");
            Silverlight = new PlatformType("Silverlight");
            WinPhone = new PlatformType("WinPhone");
            WinForms = new PlatformType("WinForms");
            WinRT = new PlatformType("WinRT");
            WPF = new PlatformType("WPF");
            Unknown = new PlatformType("Unknown");
            UnitTest = new PlatformType("UnitTest");            
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlatformType" /> class.
        /// </summary>
        public PlatformType(string id)
            : base(id)
        {
        }

        #endregion
    }
}