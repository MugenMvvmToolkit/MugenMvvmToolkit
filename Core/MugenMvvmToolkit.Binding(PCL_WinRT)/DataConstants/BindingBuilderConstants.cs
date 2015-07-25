#region Copyright

// ****************************************************************************
// <copyright file="BindingBuilderConstants.cs">
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
using System.Collections.Generic;
using System.Globalization;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.DataConstants
{
    /// <summary>
    ///     Contains the binding builder constants.
    /// </summary>
    public static class BindingBuilderConstants
    {
        #region Fields

        public static readonly DataConstant<IList<object>> RawSources;

        public static readonly DataConstant<object> Source;

        public static readonly DataConstant<Func<IDataContext, IDataBinding>> BuildDelegate;

        public static readonly DataConstant<object> Target;

        public static readonly DataConstant<ISourceValue> TargetSource;

        public static readonly DataConstant<IBindingPath> TargetPath;

        public static readonly DataConstant<IList<Func<IDataContext, IBindingSource>>> Sources;

        public static readonly DataConstant<Func<IDataContext, IList<object>, object>> MultiExpression;

        public static readonly DataConstant<List<IBindingBehavior>> Behaviors;

        public static readonly DataConstant<Func<IDataContext, IBindingValueConverter>> Converter;

        public static readonly DataConstant<Func<IDataContext, object>> ConverterParameter;

        public static readonly DataConstant<Func<IDataContext, CultureInfo>> ConverterCulture;

        public static readonly DataConstant<Func<IDataContext, object>> CommandParameter;

        public static readonly DataConstant<Func<IDataContext, object>> Fallback;

        public static readonly DataConstant<object> TargetNullValue;

        public static readonly DataConstant<bool> ToggleEnabledState;

        public static readonly DataConstant NoCache;

        #endregion

        #region Constructors

        static BindingBuilderConstants()
        {
            RawSources = DataConstant.Create(() => RawSources, true);
            Source = DataConstant.Create(() => Source, true);
            BuildDelegate = DataConstant.Create(() => BuildDelegate, true);
            Target = DataConstant.Create(() => Target, true);
            TargetSource = DataConstant.Create(() => TargetSource, true);
            TargetPath = DataConstant.Create(() => TargetPath, true);
            Sources = DataConstant.Create(() => Sources, true);
            MultiExpression = DataConstant.Create(() => MultiExpression, true);
            Behaviors = DataConstant.Create(() => Behaviors, true);
            Converter = DataConstant.Create(() => Converter, true);
            ConverterParameter = DataConstant.Create(() => ConverterParameter, true);
            ConverterCulture = DataConstant.Create(() => ConverterCulture, true);
            CommandParameter = DataConstant.Create(() => CommandParameter, true);
            ToggleEnabledState = DataConstant.Create(() => ToggleEnabledState);
            Fallback = DataConstant.Create(() => Fallback, true);
            TargetNullValue = DataConstant.Create(() => TargetNullValue, false);
            NoCache = DataConstant.Create(() => NoCache);
        }

        #endregion
    }
}