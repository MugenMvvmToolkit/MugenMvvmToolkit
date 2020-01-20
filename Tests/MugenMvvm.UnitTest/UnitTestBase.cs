using System;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Metadata.Components;
using MugenMvvm.Threading;
using MugenMvvm.UnitTest.Threading;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MugenMvvm.UnitTest
{
    public class UnitTestBase
    {
        #region Fields

        protected static readonly IReadOnlyMetadataContext DefaultMetadata = new ReadOnlyMetadataContext(Default.EmptyArray<MetadataContextValue>());

        #endregion

        #region Constructors

        public UnitTestBase()
        {
            MugenService.Configuration.Initialize<IComponentCollectionProvider>(new ComponentCollectionProvider());

            var metadataContextProvider = new MetadataContextProvider();
            metadataContextProvider.AddComponent(new MetadataContextProviderComponent());
            MugenService.Configuration.Initialize<IMetadataContextProvider>(metadataContextProvider);

            var weakReferenceProvider = new WeakReferenceProvider();
            weakReferenceProvider.AddComponent(new WeakReferenceProviderComponent());
            MugenService.Configuration.Initialize<IWeakReferenceProvider>(weakReferenceProvider);

            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            MugenService.Configuration.Initialize<IThreadDispatcher>(threadDispatcher);

            var reflectionDelegateProvider = new ReflectionDelegateProvider();
            reflectionDelegateProvider.AddComponent(new ExpressionReflectionDelegateProviderComponent());
            MugenService.Configuration.Initialize<IReflectionDelegateProvider>(reflectionDelegateProvider);
        }

        #endregion

        #region Methods

        protected static void ShouldThrow<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                var aggregateException = exception as AggregateException;
                if (aggregateException != null)
                    exception = GetOriginalException(aggregateException);
                if (!(exception is T))
                    throw new InvalidOperationException($"The exception is wrong {exception}.");
                Tracer.Info()?.Trace("Error : " + exception);
                return;
            }

            throw new InvalidOperationException($"The exception {typeof(T)} was not thrown.");
        }

        protected void ShouldThrow(Action action)
        {
            ShouldThrow<Exception>(action);
        }

        protected static Exception GetOriginalException(AggregateException aggregateException)
        {
            Exception exception = aggregateException;
            while (aggregateException != null)
            {
                exception = aggregateException.InnerException;
                aggregateException = exception as AggregateException;
            }

            return exception;
        }

        #endregion
    }
}