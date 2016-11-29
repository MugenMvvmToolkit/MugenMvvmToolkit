#region Copyright

// ****************************************************************************
// <copyright file="OperationResult.cs">
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
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    public abstract class OperationResult : IOperationResult
    {
        #region Nested types

        private sealed class OperationResultImpl<TResult> : OperationResult, IOperationResult<TResult>
        {
            #region Constructors

            public OperationResultImpl(OperationType operation, [NotNull] object source, Exception exception,
                bool isCanceled, TResult result, IDataContext context)
                : base(operation, source, exception, isCanceled, result, context)
            {
            }

            #endregion

            #region Implementation of IOperationResult<out TResult>

            public new TResult Result => (TResult)base.Result;

            #endregion
        }

        #endregion

        #region Fields

        private static readonly MethodInfo CreateCancelResultMethod;
        private static readonly MethodInfo CreateErrorResultMethod;
        private static readonly MethodInfo CreateResultMethod;
        private static readonly MethodInfo ConvertMethod;

        private readonly IDataContext _context;
        private readonly Exception _exception;
        private readonly bool _isCanceled;
        private readonly OperationType _operation;
        private readonly object _result;
        private readonly object _source;

        #endregion

        #region Constructors

        static OperationResult()
        {
            CreateCancelResultMethod = typeof(OperationResult)
                .GetMethodsEx(MemberFlags.Public | MemberFlags.Static)
                .First(info => info.Name == nameof(CreateCancelResult) && info.IsGenericMethodDefinition);
            CreateErrorResultMethod = typeof(OperationResult)
                .GetMethodsEx(MemberFlags.Public | MemberFlags.Static)
                .First(info => info.Name == nameof(CreateErrorResult) && info.IsGenericMethodDefinition);
            CreateResultMethod = typeof(OperationResult)
                .GetMethodsEx(MemberFlags.Public | MemberFlags.Static)
                .First(info => info.Name == nameof(CreateResult) && info.IsGenericMethodDefinition);
            ConvertMethod = typeof(OperationResult)
                .GetMethodsEx(MemberFlags.Public | MemberFlags.Static)
                .First(info => info.Name == nameof(Convert) && info.IsGenericMethodDefinition);
        }

        protected OperationResult(OperationType operation, [NotNull] object source, Exception exception, bool isCanceled,
            object result, IDataContext context)
        {
            Should.NotBeNull(source, nameof(source));
            Should.NotBeNull(operation, nameof(operation));
            _operation = operation;
            _source = source;
            _exception = exception;
            _result = result;
            _isCanceled = isCanceled;
            _context = context ?? DataContext.Empty;
        }

        #endregion

        #region Implementation of IOperationResult

        public OperationType Operation => _operation;

        public object Source => _source;

        public Exception Exception => _exception;

        public bool IsCanceled => _isCanceled;

        public bool IsFaulted => Exception != null;

        public object Result
        {
            get
            {
                if (Exception != null)
                    throw Exception;
                if (IsCanceled)
                    throw new OperationCanceledException();
                return _result;
            }
        }

        public IDataContext OperationContext => _context;

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        public static IOperationResult<TType> Convert<TType>([NotNull] IOperationResult result)
        {
            Should.NotBeNull(result, nameof(result));
            var genericResult = result as IOperationResult<TType>;
            if (genericResult != null)
                return genericResult;
            if (result.IsCanceled)
                return CreateCancelResult<TType>(result.Operation, result.Source, result.OperationContext);
            if (result.IsFaulted)
                return CreateErrorResult<TType>(result.Operation, result.Source, result.Exception, result.OperationContext);
            return CreateResult(result.Operation, result.Source,
                result.Result == null ? default(TType) : (TType)result.Result, result.OperationContext);
        }

        public static IOperationResult Convert([NotNull] Type resultType, [NotNull] IOperationResult result)
        {
            Should.NotBeNull(resultType, nameof(resultType));
            return (IOperationResult)ConvertMethod
                .MakeGenericMethod(resultType)
                .Invoke(null, new object[] { result });
        }

        [Preserve(Conditional = true)]
        public static IOperationResult<TType> CreateCancelResult<TType>(OperationType operation, object source, IDataContext context = null)
        {
            return new OperationResultImpl<TType>(operation, source, null, true, default(TType), context);
        }

        public static IOperationResult CreateCancelResult([NotNull] Type resultType, OperationType operation, object source, IDataContext context = null)
        {
            Should.NotBeNull(resultType, nameof(resultType));
            return (IOperationResult)CreateCancelResultMethod
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { operation, source, context });
        }

        [Preserve(Conditional = true)]
        public static IOperationResult<TType> CreateErrorResult<TType>(OperationType operation, object source, Exception exception, IDataContext context = null)
        {
            return new OperationResultImpl<TType>(operation, source, exception, false, default(TType), context);
        }

        public static IOperationResult CreateErrorResult([NotNull] Type resultType, OperationType operation, object source, Exception exception, IDataContext context = null)
        {
            Should.NotBeNull(resultType, nameof(resultType));
            return (IOperationResult)CreateErrorResultMethod
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { operation, source, exception, context });
        }

        [Preserve(Conditional = true)]
        public static IOperationResult<TType> CreateResult<TType>(OperationType operation, object source, TType result, IDataContext context = null)
        {
            return new OperationResultImpl<TType>(operation, source, null, false, result, context);
        }

        public static IOperationResult CreateResult([NotNull] Type resultType, OperationType operation, object source, object result, IDataContext context = null)
        {
            Should.NotBeNull(resultType, nameof(resultType));
            return (IOperationResult)CreateResultMethod
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { operation, source, result, context });
        }

        #endregion
    }
}
