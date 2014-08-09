using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Should.Core.Exceptions;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{


}

namespace Microsoft.VisualStudio.TestPlatform.UnitTestFramework
{


}

namespace MugenMvvmToolkit
{
    public static class TestExtensions
    {
        public static readonly IList<string> TestStrings = new[] { "test1", "test2", "test3", "test4" };

        public static void ShouldThrow(this Action action)
        {
            bool isThrow = false;
            try
            {
                action();
            }
            catch (Exception)
            {
                isThrow = true;
            }
            if (!isThrow)
                throw new AssertException();
        }

        public static T GetDataTest<T>(this IDataContext context, DataConstant<T> constant)
        {
            T data;
            if (context.TryGetData(constant, out data))
                return data;
            return default(T);
        }

        internal static bool IsSerializable(this Type type)
        {
#if NETFX_CORE
            return type.IsDefined(typeof(DataContractAttribute), false) || type.GetTypeInfo().IsPrimitive;
#else
            return type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;            
#endif
        }
    }
}