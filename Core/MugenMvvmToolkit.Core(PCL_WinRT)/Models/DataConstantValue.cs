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
    /// <summary>
    ///     Represents the DataConstant with value.
    /// </summary>
    [StructLayout(LayoutKind.Auto), Serializable]
    public struct DataConstantValue
    {
        #region Fields

        /// <summary>
        ///     Gets the <see cref="DataConstant" />
        /// </summary>
        public readonly DataConstant DataConstant;

        /// <summary>
        ///     Gets the value.
        /// </summary>
        public readonly object Value;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataConstantValue" /> class.
        /// </summary>
        private DataConstantValue([NotNull] DataConstant dataConstant, object value)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            DataConstant = dataConstant;
            Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the value that indicates that struct is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return ReferenceEquals(DataConstant, null); }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates a new instance of the <see cref="DataConstantValue" /> class.
        /// </summary>
        public static DataConstantValue Create<T>(DataConstant<T> dataConstant, T value)
        {
            return new DataConstantValue(dataConstant, value);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataConstantValue" /> class.
        /// </summary>
        public static DataConstantValue Create(DataConstant dataConstant, object value)
        {
            return new DataConstantValue(dataConstant, value);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("DataConstant: {0}, Value: {1}", DataConstant, Value);
        }

        #endregion
    }
}