using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Should.Core.Exceptions;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
}

namespace Microsoft.VisualStudio.TestPlatform.UnitTestFramework
{
}

namespace MugenMvvmToolkit.Silverlight.Binding.Converters
{
}

namespace MugenMvvmToolkit.Silverlight.Binding.Modules
{
}

namespace MugenMvvmToolkit.Silverlight.Infrastructure
{
}

namespace MugenMvvmToolkit.Silverlight.Infrastructure.Mediators
{
}

namespace MugenMvvmToolkit.Silverlight
{
}

namespace MugenMvvmToolkit.Silverlight.Interfaces.Views
{
}

namespace MugenMvvmToolkit.Silverlight.Infrastructure.Navigation
{
}

namespace MugenMvvmToolkit.Silverlight.Interfaces.Navigation
{
}

namespace MugenMvvmToolkit.Silverlight.Models.EventArg
{
}

namespace MugenMvvmToolkit.WPF.Infrastructure.Mediators
{
}

namespace MugenMvvmToolkit.WPF.Binding.Converters
{
}

namespace MugenMvvmToolkit.WPF.Binding.Modules
{
}

namespace MugenMvvmToolkit.WPF.Infrastructure
{
}

namespace MugenMvvmToolkit.WPF
{
}

namespace MugenMvvmToolkit.WPF.Interfaces.Views
{
}

namespace MugenMvvmToolkit.WPF.Infrastructure.Navigation
{
}

namespace MugenMvvmToolkit.WPF.Interfaces.Navigation
{
}

namespace MugenMvvmToolkit.WPF.Models.EventArg
{
}

namespace MugenMvvmToolkit.WinRT.Infrastructure.Mediators
{
}

namespace MugenMvvmToolkit.WinRT.Binding.Converters
{
}

namespace MugenMvvmToolkit.WinRT.Binding.Modules
{
}

namespace MugenMvvmToolkit.WinRT.Infrastructure
{
}

namespace MugenMvvmToolkit.WinRT
{
}

namespace MugenMvvmToolkit.WinRT.Interfaces.Views
{
}

namespace MugenMvvmToolkit.WinRT.Infrastructure.Navigation
{
}

namespace MugenMvvmToolkit.WinRT.Interfaces.Navigation
{
}

namespace MugenMvvmToolkit.WinRT.Models.EventArg
{
}

namespace MugenMvvmToolkit.WinRT.Infrastructure.Callbacks
{
}

namespace MugenMvvmToolkit.WPF.Infrastructure.Callbacks
{
}

namespace MugenMvvmToolkit.Silverlight.Infrastructure.Callbacks
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

        public static IList<object> GetObservers(this IEventAggregator aggregator)
        {
            return aggregator.GetSubscribers().Select(subscriber => subscriber.Target).ToList();
        }

        public static T GetDataTest<T>(this IDataContext context, DataConstant<T> constant)
        {
            T data;
            if (context.TryGetData(constant, out data))
                return data;
            return default(T);
        }
    }
}
