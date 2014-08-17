#region Copyright
// ****************************************************************************
// <copyright file="MvvmUtils.cs">
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Utils
{
    /// <summary>
    ///     Represents the helper class.
    /// </summary>
    public static class MvvmUtils
    {
        #region Fields

        /// <summary>
        ///     Gets the true task result.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static readonly Task<bool> TrueTaskResult;

        /// <summary>
        ///     Gets the false task result.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static readonly Task<bool> FalseTaskResult;

        /// <summary>
        /// Gets the empty weak reference.
        /// </summary>
        public static readonly WeakReference EmptyWeakReference;

        private static readonly HashSet<string> KnownPublicKeys;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MvvmUtils" /> class.
        /// </summary>
        static MvvmUtils()
        {
            EmptyWeakReference = new WeakReference(null, false);
            TrueTaskResult = MvvmExtensions.FromResult(true);
            FalseTaskResult = MvvmExtensions.FromResult(false);
            CanTraceFinalizedItem = Debugger.IsAttached;
            //NOTE: 7cec85d7bea7798e, 31bf3856ad364e35, b03f5f7f11d50a3a, b77a5c561934e089 - NET FRAMEWORK
            //NOTE: 0738eb9f132ed756, 84e04ff9cfb79065 - MONO
            //NOTE: 5803cfa389c90ce7 - Telerik
            //NOTE: 17863af14b0044da - Autofac
            KnownPublicKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "7cec85d7bea7798e", "31bf3856ad364e35", "b03f5f7f11d50a3a",  "b77a5c561934e089", 
                "0738eb9f132ed756", "84e04ff9cfb79065", "5803cfa389c90ce7", "17863af14b0044da"
            };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value that is responsible to trace disposed items.
        /// </summary>
        public static bool CanTraceFinalizedItem { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to close view-model.
        /// </summary>
        public static Task<bool> TryCloseAsync([NotNull]IViewModel viewModel, [CanBeNull] object parameter, [CanBeNull] INavigationContext context)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (context == null)
                context = parameter as INavigationContext ??
                          new NavigationContext(NavigationMode.Back, viewModel, viewModel.GetParentViewModel(), null);
            if (parameter == null)
                parameter = context;
            //NOTE: Close view model only on back navigation.
            var closeableViewModel = context.NavigationMode == NavigationMode.Back
                ? viewModel as ICloseableViewModel
                : null;
            var navigableViewModel = viewModel as INavigableViewModel;
            if (closeableViewModel == null && navigableViewModel == null)
                return TrueTaskResult;
            if (closeableViewModel != null && navigableViewModel != null)
            {
                var navigatingTask = navigableViewModel.OnNavigatingFrom(context);
                if (navigatingTask.IsCompleted)
                {
                    if (navigatingTask.Result)
                        return closeableViewModel.CloseAsync(parameter);
                    return FalseTaskResult;
                }
                return navigatingTask
                    .TryExecuteSynchronously(task =>
                    {
                        if (task.Result)
                            return closeableViewModel.CloseAsync(parameter);
                        return FalseTaskResult;
                    }).Unwrap();
            }
            if (closeableViewModel == null)
                return navigableViewModel.OnNavigatingFrom(context);
            return closeableViewModel.CloseAsync(parameter);
        }

        /// <summary>
        /// Writes information about an item.
        /// </summary>
        public static void TraceFinalizedItem(object item, string message = null)
        {
            if (CanTraceFinalizedItem)
                Tracer.Warn("Finalized - {0} ({1}); {2}", item.GetType(), item.GetHashCode(), message);
        }

        /// <summary>
        ///     Gets the modules.
        /// </summary>
        public static IList<IModule> GetModules([NotNull] IEnumerable<Assembly> assemblies, bool throwOnError)
        {
            Should.NotBeNull(assemblies, "assemblies");
            var modulesToLoad = new List<Type>();
            foreach (var assembly in SkipFrameworkAssemblies(assemblies).Distinct())
            {
                foreach (var type in assembly.SafeGetTypes(throwOnError))
                {
#if PCL_WINRT
                    var typeInfo = type.GetTypeInfo();
                    if (typeof(IModule).IsAssignableFrom(type) && typeInfo.IsClass
                                            && !typeInfo.IsAbstract && typeInfo.IsPublic
                                            && type.GetConstructor(EmptyValue<Type>.ArrayInstance) != null)
#else
                    if (typeof(IModule).IsAssignableFrom(type) && type.IsClass
                                            && !type.IsAbstract && type.IsPublic
                                            && type.GetConstructor(EmptyValue<Type>.ArrayInstance) != null)
#endif
                        modulesToLoad.Add(type);
                }
            }
            var modules = new List<IModule>();
            for (int index = 0; index < modulesToLoad.Count; index++)
            {
                Type moduleType = modulesToLoad[index];
#if PCL_WINRT
                if (modulesToLoad.Any(type => type != moduleType && type.GetTypeInfo().IsSubclassOf(moduleType)))
#else
                if (modulesToLoad.Any(type => type != moduleType && type.IsSubclassOf(moduleType)))
#endif

                {
                    modulesToLoad.Remove(moduleType);
                    index--;
                    continue;
                }
                var module = (IModule)Activator.CreateInstance(moduleType);
                modules.Add(module);
            }
            modules.Sort((module, module1) => module1.Priority.CompareTo(module.Priority));
            return modules;
        }

        /// <summary>
        ///     Tries to initialize <see cref="IDesignTimeManager" />.
        /// </summary>
        public static void InitializeDesignTimeManager()
        {
            var getManager = ApplicationSettings.GetDesignTimeManager;
            if (getManager != null)
                getManager();
        }

        /// <summary>
        ///     Tries to initialize design view model using the <see cref="ApplicationSettings.GetDesignTimeManager" />.
        /// </summary>
        public static void TryInitializeDesignViewModel(IViewModel viewModel)
        {
            if (!ApplicationSettings.IsDesignMode)
                return;
            Action<IViewModel> action = ApplicationSettings.InitializeDesignViewModel;
            if (action != null)
            {
                action(viewModel);
                return;
            }
            Func<IDesignTimeManager> getManager = ApplicationSettings.GetDesignTimeManager;
            if (getManager == null)
                return;
            IDesignTimeManager designTimeManager = getManager();
            if (designTimeManager == null)
                return;
            SynchronizationContext context = SynchronizationContext.Current;
            Task.Factory.StartNew(() =>
            {
                if (context == null)
                    designTimeManager.InitializeViewModel(viewModel);
                else
                    context.Post(o => designTimeManager.InitializeViewModel(viewModel), null);
            });
        }

        public static void WithTaskExceptionHandler(Task task, object sender)
        {
            var viewModel = sender as IViewModel;
            if (viewModel == null)
                task.WithTaskExceptionHandler(sender, ServiceProvider.IocContainer);
            else
                task.WithTaskExceptionHandler(viewModel);
        }

        [NotNull]
        public static IIocContainer GetIocContainer([NotNull] IViewModel viewModel, bool useGlobal)
        {
            return viewModel.GetIocContainer(useGlobal);
        }

        /// <summary>
        /// Tries to get parent view model, the result value can be null.
        /// </summary>
        [CanBeNull]
        public static IViewModel GetParentViewModel(this IViewModel viewModel)
        {
            var reference = viewModel.Settings.Metadata.GetData(ViewModelConstants.ParentViewModel);
            if (reference == null)
                return null;
            return (IViewModel)reference.Target;
        }

        /// <summary>
        /// Filters assemblies.
        /// </summary>
        public static IEnumerable<Assembly> SkipFrameworkAssemblies(IEnumerable<Assembly> assemblies)
        {
            return assemblies.Where(NonFrameworkAssemblyFilter).Distinct();
        }

        public static bool NonFrameworkAssemblyFilter(Assembly assembly)
        {
#if !PCL_Silverlight
            if (assembly.IsDynamic)
                return false;
#endif
            return !assembly.HasKnownPublicKey();
        }

        private static bool HasKnownPublicKey(this Assembly assembly)
        {
            var assemblyName = assembly.GetAssemblyName();
            var bytes = assemblyName.GetPublicKeyToken();
            if (bytes == null || bytes.Length == 0)
                return false;
            var builder = new StringBuilder(16);
            for (int i = 0; i < bytes.Length; i++)
                builder.Append(bytes[i].ToString("x2"));
            return KnownPublicKeys.Contains(builder.ToString());
        }

        #endregion
    }
}