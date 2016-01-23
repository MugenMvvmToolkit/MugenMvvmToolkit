#region Copyright

// ****************************************************************************
// <copyright file="BindingBuilderConstants.cs">
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

using System;
using System.Collections.Generic;
using System.Globalization;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.DataConstants
{
    public static class BindingBuilderConstants
    {
        #region Fields

        public static readonly DataConstant<IList<object>> RawSources;
        public static readonly DataConstant<object> Source;
        public static readonly DataConstant<Func<IDataContext, IDataBinding>> BuildDelegate;
        public static readonly DataConstant<object> Target;
        public static readonly DataConstant<ISourceValue> TargetSource;
        public static readonly DataConstant<IBindingPath> TargetPath;
        public static readonly DataConstant<IList<Func<IDataContext, IObserver>>> Sources;
        public static readonly DataConstant<Func<IDataContext, IList<object>, object>> MultiExpression;
        public static readonly DataConstant<List<IBindingBehavior>> Behaviors;
        public static readonly DataConstant<Func<IDataContext, IBindingValueConverter>> Converter;
        public static readonly DataConstant<Func<IDataContext, object>> ConverterParameter;
        public static readonly DataConstant<Func<IDataContext, CultureInfo>> ConverterCulture;
        public static readonly DataConstant<Func<IDataContext, object>> CommandParameter;
        public static readonly DataConstant<Func<IDataContext, object>> Fallback;
        public static readonly DataConstant<object> TargetNullValue;
        public static readonly DataConstant<bool> ToggleEnabledState;
        public static readonly DataConstant<bool> HasStablePath;
        public static readonly DataConstant<bool> Observable;
        public static readonly DataConstant NoCache;

        #endregion

        #region Constructors

        static BindingBuilderConstants()
        {
            var type = typeof(BindingBuilderConstants);
            RawSources = DataConstant.Create<IList<object>>(type, nameof(RawSources), true);
            Source = DataConstant.Create<object>(type, nameof(Source), true);
            BuildDelegate = DataConstant.Create<Func<IDataContext, IDataBinding>>(type, nameof(BuildDelegate), true);
            Target = DataConstant.Create<object>(type, nameof(Target), true);
            TargetSource = DataConstant.Create<ISourceValue>(type, nameof(TargetSource), true);
            TargetPath = DataConstant.Create<IBindingPath>(type, nameof(TargetPath), true);
            Sources = DataConstant.Create<IList<Func<IDataContext, IObserver>>>(type, nameof(Sources), true);
            MultiExpression = DataConstant.Create<Func<IDataContext, IList<object>, object>>(type, nameof(MultiExpression), true);
            Behaviors = DataConstant.Create<List<IBindingBehavior>>(type, nameof(Behaviors), true);
            Converter = DataConstant.Create<Func<IDataContext, IBindingValueConverter>>(type, nameof(Converter), true);
            ConverterParameter = DataConstant.Create<Func<IDataContext, object>>(type, nameof(ConverterParameter), true);
            ConverterCulture = DataConstant.Create<Func<IDataContext, CultureInfo>>(type, nameof(ConverterCulture), true);
            CommandParameter = DataConstant.Create<Func<IDataContext, object>>(type, nameof(CommandParameter), true);
            ToggleEnabledState = DataConstant.Create<bool>(type, nameof(ToggleEnabledState));
            Fallback = DataConstant.Create<Func<IDataContext, object>>(type, nameof(Fallback), true);
            TargetNullValue = DataConstant.Create<object>(type, nameof(TargetNullValue), false);
            HasStablePath = DataConstant.Create<bool>(type, nameof(HasStablePath));
            Observable = DataConstant.Create<bool>(type, nameof(Observable));
            NoCache = DataConstant.Create(type, nameof(NoCache));
        }

        #endregion
    }
}
