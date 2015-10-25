#region Copyright

// ****************************************************************************
// <copyright file="PlatformType.cs">
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

namespace MugenMvvmToolkit.Models
{
    public class PlatformType : StringConstantBase<PlatformType>
    {
        #region Fields

        public static readonly PlatformType Android;

        public static readonly PlatformType iOS;

        public static readonly PlatformType Silverlight;

        public static readonly PlatformType WinPhone;

        public static readonly PlatformType WinForms;

        public static readonly PlatformType WinRT;

        public static readonly PlatformType WinRTPhone;

        public static readonly PlatformType WPF;

        public static readonly PlatformType Unknown;

        public static readonly PlatformType UnitTest;

        public static readonly PlatformType XamarinFormsAndroid;

        public static readonly PlatformType XamarinFormsiOS;

        public static readonly PlatformType XamarinFormsWinPhone;

        public static readonly PlatformType XamarinFormsWinRT;

        public static readonly PlatformType XamarinFormsWinRTPhone;

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
            WinRTPhone = new PlatformType(WinRT.Id + ".Phone");
            WPF = new PlatformType("WPF");
            XamarinFormsAndroid = new PlatformType("XamarinForms." + Android.Id);
            XamarinFormsiOS = new PlatformType("XamarinForms." + iOS.Id);
            XamarinFormsWinPhone = new PlatformType("XamarinForms." + WinPhone.Id);
            XamarinFormsWinRT = new PlatformType("XamarinForms." + WinRT.Id);
            XamarinFormsWinRTPhone = new PlatformType("XamarinForms." + WinRTPhone.Id);
            Unknown = new PlatformType("Unknown");
            UnitTest = new PlatformType("UnitTest");
        }

        public PlatformType(string id)
            : base(id)
        {
        }

        #endregion
    }
}
