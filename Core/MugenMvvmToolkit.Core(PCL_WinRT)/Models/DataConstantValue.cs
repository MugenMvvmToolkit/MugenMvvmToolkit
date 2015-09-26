#region Copyright

// ****************************************************************************
// <copyright file="DataConstantValue.cs">
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

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models
{
    [StructLayout(LayoutKind.Auto), Serializable]
    public struct DataConstantValue
    {
        #region Fields

        public readonly DataConstant DataConstant;

        public readonly object Value;

        #endregion

        #region Constructors

        private DataConstantValue([NotNull] DataConstant dataConstant, object value)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            DataConstant = dataConstant;
            Value = value;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return ReferenceEquals(DataConstant, null); }
        }

        #endregion

        #region Methods

        public static DataConstantValue Create<T>(DataConstant<T> dataConstant, T value)
        {
            return new DataConstantValue(dataConstant, value);
        }

        public static DataConstantValue Create(DataConstant dataConstant, object value)
        {
            return new DataConstantValue(dataConstant, value);
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return string.Format("DataConstant: {0}, Value: {1}", DataConstant, Value);
        }

        #endregion
    }
}
