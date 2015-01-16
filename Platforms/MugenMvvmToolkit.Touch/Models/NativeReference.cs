#region Copyright

// ****************************************************************************
// <copyright file="NativeReference.cs">
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
using Foundation;
using ObjCRuntime;

namespace MugenMvvmToolkit.Models
{
    internal struct NativeReference : IEquatable<NativeReference>
    {
        #region Equality members

        public bool Equals(NativeReference other)
        {
            return _handle == other._handle && _classHandle == other._classHandle;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NativeReference && Equals((NativeReference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_handle.GetHashCode() * 397) ^ _classHandle.GetHashCode();
            }
        }

        #endregion

        #region Fields

        private readonly IntPtr _classHandle;
        private readonly IntPtr _handle;
        private bool _isInvalid;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NativeReference" /> class.
        /// </summary>
        public NativeReference(NSObject nsObject)
        {
            _handle = nsObject.Handle;
            _classHandle = nsObject.ClassHandle;
            _isInvalid = false;
        }

        #endregion

        #region Properties

        public bool IsAlive
        {
            get { return Target != null; }
        }

        public NSObject Target
        {
            get
            {
                if (_isInvalid)
                    return null;
                try
                {
                    var nsObject = Runtime.GetNSObject(_handle);
                    if (nsObject != null && nsObject.ClassHandle == _classHandle)
                        return nsObject;
                    return null;
                }
                catch
                {
                    _isInvalid = true;
                    return null;
                }
            }
        }

        #endregion
    }
}