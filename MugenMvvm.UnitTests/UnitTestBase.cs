using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Commands;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal.Components;
using MugenMvvm.Serialization;
using MugenMvvm.Tests;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.Validation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests
{
    public class UnitTestBase : MugenUnitTestBase
    {
        protected const string SharedContext = nameof(SharedContext);

#if LONGRUNNINGTEST
        protected const int LongRunningTimeout = 60;
#else
        protected const int LongRunningTimeout = 10;
#endif

#if DEBUG
        protected const string ReleaseTest = "NOT SUPPORTED IN DEBUG";
#else
        protected const string? ReleaseTest = null;
#endif

        private CompositeCommand? _command;
        private Validator? _validator;
        private TestBinding? _binding;
        protected static readonly ReadOnlyDictionary<string, object?> EmptyDictionary = new(new Dictionary<string, object?>());
        protected static readonly SerializationContext<object?, object?> EmptySerializationContext = new(new SerializationFormat<object?, object?>(1, ""), null);

        static UnitTestBase()
        {
            EnumBase.ThrowOnDuplicate = false;
        }

        public UnitTestBase(ITestOutputHelper? outputHelper = null)
        {
            if (outputHelper != null)
                Logger.AddComponent(new DelegateLogger((l, msg, e, _) => outputHelper.WriteLine($"{l} - {msg} {e?.Flatten()}"), (_, _) => true));
        }

        protected CompositeCommand Command => _command ??= new CompositeCommand(null, ComponentCollectionManager);

        protected Validator Validator => _validator ??= new Validator(null, ComponentCollectionManager);

        protected TestBinding Binding => _binding ??= new TestBinding(ComponentCollectionManager);

        protected static void WaitCompletion(int milliseconds = 10, Func<bool>? predicate = null, int attemptCount = 20)
        {
            var count = 0;
            while (true)
            {
                Thread.Sleep(milliseconds);
                if (predicate == null || predicate() || count == attemptCount)
                    return;
                ++count;
            }
        }

        protected static void ShouldThrow<T>(Action action) where T : Exception => Assert.Throws<T>(action);

        protected void ShouldThrow(Action action) => Assert.ThrowsAny<Exception>(action);

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