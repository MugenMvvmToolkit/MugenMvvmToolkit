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

            InitializeThreadDispatcher();

            var reflectionDelegateProvider = new ReflectionDelegateProvider();
            reflectionDelegateProvider.AddComponent(new ExpressionReflectionDelegateProviderComponent());
            MugenService.Configuration.Initialize<IReflectionDelegateProvider>(reflectionDelegateProvider);
        }

        #endregion

        #region Methods

        protected virtual void InitializeThreadDispatcher()
        {
            var threadDispatcher = new ThreadDispatcher();
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent());
            MugenService.Configuration.Initialize<IThreadDispatcher>(threadDispatcher);
        }

        protected static void ShouldThrow<T>(Action action) where T : Exception
        {
            Assert.Throws<T>(action);
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
                aggregateException = (exception as AggregateException)!;
            }

            return exception;
        }

        #endregion
    }
}