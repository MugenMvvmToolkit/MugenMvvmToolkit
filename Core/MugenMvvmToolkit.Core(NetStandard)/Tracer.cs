#region Copyright

// ****************************************************************************
// <copyright file="Tracer.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

#define DEBUG
using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using TraceLevel = MugenMvvmToolkit.Models.TraceLevel;

namespace MugenMvvmToolkit
{
    public class Tracer : ITracer
    {
        #region Fields

        internal static readonly Tracer Instance;

        #endregion

        #region Constructors

        static Tracer()
        {
            Instance = new Tracer();
            var isAttached = Debugger.IsAttached;
            TraceWarning = isAttached;
            TraceError = isAttached;
        }

        protected Tracer()
        {
        }

        #endregion

        #region Properties

        public static bool TraceInformation { get; set; }

        public static bool TraceWarning { get; set; }

        public static bool TraceError { get; set; }

        #endregion

        #region Methods

        public static void TraceViewModel(ViewModelLifecycleType lifecycleType, IViewModel viewModel)
        {
            TraceViewModelHandler?.Invoke(lifecycleType, viewModel);
            ServiceProvider.Tracer.TraceViewModel(lifecycleType, viewModel);
        }

        public static void Info(string message)
        {
            if (TraceInformation)
                ServiceProvider.Tracer.Trace(TraceLevel.Information, message);
        }

        public static void Warn(string message)
        {
            if (TraceWarning)
                ServiceProvider.Tracer.Trace(TraceLevel.Warning, message);
        }

        public static void Error(string message)
        {
            if (TraceError)
                ServiceProvider.Tracer.Trace(TraceLevel.Error, message);
        }

        [StringFormatMethod("format")]
        public static void Info(string format, params object[] args)
        {
            if (TraceInformation)
                ServiceProvider.Tracer.Trace(TraceLevel.Information, format, args);
        }

        [StringFormatMethod("format")]
        public static void Warn(string format, params object[] args)
        {
            if (TraceWarning)
                ServiceProvider.Tracer.Trace(TraceLevel.Warning, format, args);
        }

        [StringFormatMethod("format")]
        public static void Error(string format, params object[] args)
        {
            if (TraceError)
                ServiceProvider.Tracer.Trace(TraceLevel.Error, format, args);
        }

        public static bool CanTrace(TraceLevel level)
        {
            switch (level)
            {
                case TraceLevel.Information:
                    return TraceInformation;
                case TraceLevel.Warning:
                    return TraceWarning;
                case TraceLevel.Error:
                    return TraceError;
                default:
                    throw ExceptionManager.EnumOutOfRange("level", level);
            }
        }

        protected virtual void TraceInternal(TraceLevel level, string message)
        {
            switch (level)
            {
                case TraceLevel.Information:
                    Debug.WriteLine(message);
                    break;
                case TraceLevel.Warning:
                    Debug.WriteLine(message);
                    break;
                case TraceLevel.Error:
                    Debug.WriteLine(message);
                    break;
            }
        }

        #endregion

        #region Events

        public static event Action<ViewModelLifecycleType, IViewModel> TraceViewModelHandler;

        #endregion

        #region Implementation of ITracer

        void ITracer.TraceViewModel(ViewModelLifecycleType lifecycleType, IViewModel viewModel)
        {
            TraceLevel traceLevel = lifecycleType == ViewModelLifecycleType.Finalized ? TraceLevel.Warning : TraceLevel.Information;
            if (!CanTrace(traceLevel))
                return;
            var displayName = viewModel as IHasDisplayName;
            if (displayName == null)
                Trace(traceLevel, "{0} ({1}) - {2};", viewModel.GetType(),
                    viewModel.GetHashCode().ToString(CultureInfo.InvariantCulture), lifecycleType);
            else
                Trace(traceLevel, "{0} (Hash - {1}; DisplayName - {2};) - {3}", viewModel.GetType(),
                    viewModel.GetHashCode().ToString(CultureInfo.InvariantCulture), displayName.DisplayName, lifecycleType);
        }

        public void Trace(TraceLevel level, string message)
        {
            if (CanTrace(level))
                TraceInternal(level, level + ": " + message);
        }

        public void Trace(TraceLevel level, string format, params object[] args)
        {
            if (CanTrace(level))
                TraceInternal(level, level + ": " + string.Format(format, args));
        }

        #endregion
    }
}
