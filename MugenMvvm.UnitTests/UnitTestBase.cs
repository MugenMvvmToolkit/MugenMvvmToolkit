using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using MugenMvvm.Bindings.Convert;
using MugenMvvm.Bindings.Convert.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Bindings.Resources.Components;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Serialization;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MugenMvvm.UnitTests
{
    public class UnitTestBase
    {
        protected const string SharedContext = nameof(SharedContext);

        protected const string SharedContextTest = nameof(SharedContextTest);

#if DEBUG
        protected const string ReleaseTest = "NOT SUPPORTED IN DEBUG";
#else
        protected const string ReleaseTest = null;
#endif

        protected static readonly ObservationManager ObservationManager;
        protected static readonly ResourceManager ResourceManager;
        protected static readonly ComponentCollectionManager ComponentCollectionManager;
        protected static readonly ReflectionManager ReflectionManager;
        protected static readonly AttachedValueManager AttachedValueManager;
        protected static readonly ThreadDispatcher ThreadDispatcher;
        protected static readonly GlobalValueConverter GlobalValueConverter;
        protected static readonly ReadOnlyDictionary<string, object?> EmptyDictionary = new(new Dictionary<string, object?>());
        protected static readonly SerializationContext<object?, object?> EmptySerializationContext = new(new SerializationFormat<object?, object?>(1, ""), null);
        protected static readonly CancellationToken DefaultCancellationToken = new CancellationTokenSource().Token;
        protected static readonly IReadOnlyMetadataContext DefaultMetadata = new ReadOnlyMetadataContext(Default.Array<KeyValuePair<IMetadataContextKey, object?>>());
        private static ITestOutputHelper? _outputHelper;

        static UnitTestBase()
        {
            ComponentCollectionManager = new ComponentCollectionManager();

            AttachedValueManager = new AttachedValueManager(ComponentCollectionManager);
            AttachedValueManager.AddComponent(new ConditionalWeakTableAttachedValueStorage());
            AttachedValueManager.AddComponent(new StaticTypeAttachedValueStorage());

            ThreadDispatcher = new ThreadDispatcher(ComponentCollectionManager);
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent {Priority = int.MinValue});

            var weakReferenceManager = new WeakReferenceManager(ComponentCollectionManager);
            weakReferenceManager.AddComponent(new WeakReferenceProvider());
            MugenService.Configuration.InitializeInstance<IWeakReferenceManager>(weakReferenceManager);

            ReflectionManager = new ReflectionManager(ComponentCollectionManager);
            ReflectionManager.AddComponent(new ExpressionReflectionDelegateProvider());
            MugenService.Configuration.InitializeInstance<IReflectionManager>(ReflectionManager);

            GlobalValueConverter = new GlobalValueConverter(ComponentCollectionManager);
            GlobalValueConverter.AddComponent(new DefaultGlobalValueConverter());

            ResourceManager = new ResourceManager(ComponentCollectionManager);
            ResourceManager.AddComponent(new TypeResolver());

            ObservationManager = new ObservationManager(ComponentCollectionManager);

            ILogger logger = new Logger(ComponentCollectionManager);
            logger.AddComponent(new DelegateLogger((l, msg, e, m) => _outputHelper?.WriteLine($"{l} - {msg} {e?.Flatten()}"), (level, context) => true));
            MugenService.Configuration.InitializeInstance(logger);
        }

        public UnitTestBase(ITestOutputHelper? outputHelper = null)
        {
            _outputHelper = outputHelper;
        }

        protected static void WaitCompletion(int milliseconds = 10) => Thread.Sleep(milliseconds);

        protected static void ShouldThrow<T>(Action action) where T : Exception => Assert.Throws<T>(action);

        protected static string NewId() => Guid.NewGuid().ToString("N");

        protected void ShouldThrow(Action action) => Assert.ThrowsAny<Exception>(action);

        protected static void GcCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();
        }

        protected TestExpressionNode GetTestEqualityExpression(IExpressionEqualityComparer? comparer, int hash) =>
            new()
            {
                EqualsHandler = (x, y, equalityComparer) =>
                {
                    equalityComparer.ShouldEqual(comparer);
                    return x.Id == y.Id;
                },
                GetHashCodeHandler = (e, h, c) =>
                {
                    GetBaseHashCode(e).ShouldEqual(h);
                    c.ShouldEqual(comparer);
                    return hash;
                },
                Id = hash
            };

        protected int GetBaseHashCode(IExpressionNode expression) => (expression.ExpressionType.Value * 397) ^ expression.Metadata.Count;
    }
}