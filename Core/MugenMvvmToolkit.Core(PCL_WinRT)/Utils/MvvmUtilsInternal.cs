#region Copyright
// ****************************************************************************
// <copyright file="MvvmUtilsInternal.cs">
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Utils
{
    internal static class MvvmUtilsInternal
    {
        #region Fields

        private const MemberFlags PropertyBindingFlag =
            MemberFlags.Instance | MemberFlags.NonPublic | MemberFlags.Public;

        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs NotificationCountPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs;

        private static readonly Dictionary<Type, IList<string>> CachedIgnoreAttributes;
        private static readonly Dictionary<Type, IDictionary<string, ICollection<string>>> CachedViewModelProperties;
        private static readonly IList<string> ExcludedProperties;
        private static readonly Dictionary<Type, Action<object, IViewModel>> ViewToViewModelInterface;
        private static readonly Dictionary<Type, PropertyInfo> ViewModelToViewInterface;
        private static readonly Dictionary<Type, Func<object, ICommand>[]> TypesToCommandsProperties;

        private const int MaxPrime = 2146435069;

        private static readonly int[] Primes =
        {
            3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
            1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
            17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
            187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
            1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369
        };

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MvvmUtilsInternal" /> class.
        /// </summary>
        static MvvmUtilsInternal()
        {
            CountPropertyChangedArgs = new PropertyChangedEventArgs("Count");
            NotificationCountPropertyChangedArgs = new PropertyChangedEventArgs("NotificationCount");
            IndexerPropertyChangedArgs = new PropertyChangedEventArgs("Item[]");
            CachedIgnoreAttributes = new Dictionary<Type, IList<string>>();
            CachedViewModelProperties = new Dictionary<Type, IDictionary<string, ICollection<string>>>();
            ExcludedProperties = typeof(EditableViewModel<>)
                .GetPropertiesEx(PropertyBindingFlag)
                .ToArrayFast(info => info.Name);
            ViewToViewModelInterface = new Dictionary<Type, Action<object, IViewModel>>();
            ViewModelToViewInterface = new Dictionary<Type, PropertyInfo>();
            TypesToCommandsProperties = new Dictionary<Type, Func<object, ICommand>[]>();
        }

        #endregion

        #region Methods

        public static int ExpandPrime(int oldSize)
        {
            int min = 2 * oldSize;
            if (min > MaxPrime && MaxPrime > oldSize)
                return MaxPrime;
            return GetPrime(min);
        }

        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) != 0)
            {
                var limit = (int)Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2)
                {
                    if ((candidate % divisor) == 0)
                        return false;
                }
                return true;
            }
            return candidate == 2;
        }

        public static int GetPrime(int min)
        {
            for (int i = 0; i < Primes.Length; i++)
            {
                int prime = Primes[i];
                if (prime >= min)
                    return prime;
            }
            for (int i = (min | 1); i < Int32.MaxValue; i += 2)
            {
                if (IsPrime(i))
                    return i;
            }
            return min;
        }

        public static void InvokeOnUiThreadAsync(Action action)
        {
            ServiceProvider.ThreadManager.InvokeOnUiThreadAsync(action);
        }

        internal static void TraceSubscribe(object item, object listener)
        {
            Tracer.Info("The {0} is subscribed to {1}", listener, item);
        }

        internal static void TraceUnsubscribe(object item, object listener)
        {
            Tracer.Info("The {0} is unsubscribed from {1}", listener, item);
        }

        internal static void DisposeCommands(object item)
        {
            Should.NotBeNull(item, "item");
            Func<object, ICommand>[] list;
            lock (TypesToCommandsProperties)
            {
                Type type = item.GetType();
                if (!TypesToCommandsProperties.TryGetValue(type, out list))
                {
                    list = type
                        .GetPropertiesEx(PropertyBindingFlag)
                        .Where(c => typeof(ICommand).IsAssignableFrom(c.PropertyType) && c.CanRead &&
                                    c.GetIndexParameters().Length == 0)
                        .Select(ServiceProvider.ReflectionManager.GetMemberGetter<ICommand>)
                        .ToArray();
                    TypesToCommandsProperties[type] = list;
                }
            }
            if (list.Length == 0) return;
            for (int index = 0; index < list.Length; index++)
            {
                try
                {
                    var disposable = list[index].Invoke(item) as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                catch (MemberAccessException)
                {
                    //To avoid method access exception.                
                }
            }
        }

        internal static Action<object, IViewModel> GetViewModelPropertySetter(Type viewType)
        {
            lock (ViewToViewModelInterface)
            {
                Action<object, IViewModel> result;
                if (!ViewToViewModelInterface.TryGetValue(viewType, out result))
                {
#if PCL_WINRT
                    foreach (Type @interface in viewType.GetInterfaces().Where(type => type.GetTypeInfo().IsGenericType))
#else
                    foreach (Type @interface in viewType.GetInterfaces().Where(type => type.IsGenericType))
#endif

                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewModelAwareView<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view", "IViewModelAwareView<>", viewType);
                        result = ServiceProvider.ReflectionManager.GetMemberSetter<IViewModel>(@interface.GetPropertyEx("ViewModel", MemberFlags.Public | MemberFlags.Instance));
                    }
                    ViewToViewModelInterface[viewType] = result;
                }
                return result;
            }
        }

        internal static PropertyInfo GetViewProperty(Type viewModelType)
        {
            lock (ViewModelToViewInterface)
            {
                PropertyInfo result;
                if (!ViewModelToViewInterface.TryGetValue(viewModelType, out result))
                {
#if PCL_WINRT
                    foreach (Type @interface in viewModelType.GetInterfaces().Where(type => type.GetTypeInfo().IsGenericType))
#else
                    foreach (Type @interface in viewModelType.GetInterfaces().Where(type => type.IsGenericType))
#endif
                    {
                        if (@interface.GetGenericTypeDefinition() != typeof(IViewAwareViewModel<>)) continue;
                        if (result != null)
                            throw ExceptionManager.DuplicateInterface("view model", "IViewAwareViewModel<>", viewModelType);
                        result = @interface.GetPropertyEx("View", MemberFlags.Public | MemberFlags.Instance);
                    }
                    ViewModelToViewInterface[viewModelType] = result;
                }
                return result;
            }
        }

        internal static MemberInfo ParseMemberExpression(LambdaExpression expression)
        {
            Should.NotBeNull(expression, "expression");
            // Get the last element of the include path
            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var memberExpression = unaryExpression.Operand as MemberExpression;
                if (memberExpression != null)
                    return memberExpression.Member;
            }
            var expressionBody = expression.Body as MemberExpression;
            Should.BeSupported(expressionBody != null, "Expession {0} not supported", expression);
            // ReSharper disable once PossibleNullReferenceException
            return expressionBody.Member;
        }

        internal static AssemblyName GetAssemblyName(this Assembly assembly)
        {
            Should.NotBeNull(assembly, "assembly");
            return new AssemblyName(assembly.FullName);
        }

        internal static Dictionary<string, ICollection<string>> GetViewModelToModelProperties(Type type)
        {
            IDictionary<string, ICollection<string>> result;
            lock (CachedViewModelProperties)
            {
                if (!CachedViewModelProperties.TryGetValue(type, out result))
                {
                    result = new Dictionary<string, ICollection<string>>();
                    foreach (PropertyInfo propertyInfo in type.GetPropertiesEx(PropertyBindingFlag))
                    {
                        IEnumerable<ModelPropertyAttribute> attributes = propertyInfo
                            .GetAttributes()
                            .OfType<ModelPropertyAttribute>();
                        foreach (ModelPropertyAttribute viewModelToModelAttribute in attributes)
                        {
                            ICollection<string> list;
                            if (!result.TryGetValue(viewModelToModelAttribute.Property, out list))
                            {
                                list = new HashSet<string>();
                                result[viewModelToModelAttribute.Property] = list;
                            }
                            list.Add(propertyInfo.Name);
                        }
                    }
                    CachedViewModelProperties[type] = result;
                }
            }
            return new Dictionary<string, ICollection<string>>(result);
        }

        internal static IList<string> GetIgnoreProperties(Type type)
        {
            lock (CachedIgnoreAttributes)
            {
                IList<string> result;
                if (!CachedIgnoreAttributes.TryGetValue(type, out result))
                {
                    result = new List<string>(ExcludedProperties);
                    foreach (PropertyInfo propertyInfo in type.GetPropertiesEx(PropertyBindingFlag))
                    {
                        if (propertyInfo.IsDefined(typeof(IgnorePropertyAttribute), true))
                            result.Add(propertyInfo.Name);
                    }
                    result = result.ToArrayFast();
                    CachedIgnoreAttributes[type] = result;
                }
                return result;
            }
        }

        #endregion
    }
}