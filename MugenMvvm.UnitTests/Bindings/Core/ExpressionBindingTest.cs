using System;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Tests.Bindings.Core;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    [Collection(SharedContext)]
    public class ExpressionBindingTest : UnitTestBase
    {
        public ExpressionBindingTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(BindingManager));
            RegisterDisposeToken(WithGlobalService(GlobalValueConverter));
        }

        [Fact]
        public virtual void DisposeShouldDisposeExpression()
        {
            var targetDisposed = false;
            var sourceDisposed = false;
            IMemberPathObserverListener? targetListener = null;
            IMemberPathObserverListener? sourceListener = null;
            var expression = new TestCompiledExpression();
            var target = new TestMemberPathObserver
            {
                Dispose = () => targetDisposed = true,
                AddListener = l =>
                {
                    targetListener.ShouldBeNull();
                    targetListener = l;
                },
                RemoveListener = l =>
                {
                    targetListener.ShouldEqual(l);
                    targetListener = null;
                }
            };
            var source = new TestMemberPathObserver
            {
                Dispose = () => sourceDisposed = true,
                AddListener = l =>
                {
                    sourceListener.ShouldBeNull();
                    sourceListener = l;
                },
                RemoveListener = l =>
                {
                    sourceListener.ShouldEqual(l);
                    sourceListener = null;
                }
            };

            var components = new IComponent<IBinding>[] { new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener() };
            var binding = new ExpressionBinding(target, ItemOrArray.FromItem<object?>(source), expression);
            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, DefaultMetadata);
            targetListener.ShouldEqual(binding);
            sourceListener.ShouldEqual(binding);

            var disposeCount = 0;
            BindingManager.AddComponent(new TestBindingLifecycleListener
            {
                OnLifecycleChanged = (_, b, state, _, m) =>
                {
                    ++disposeCount;
                    b.ShouldEqual(binding);
                    state.ShouldEqual(BindingLifecycleState.Disposed);
                    m.ShouldBeNull();
                }
            });

            binding.Dispose();
            binding.State.ShouldEqual(BindingState.Disposed);
            targetDisposed.ShouldBeFalse();
            sourceDisposed.ShouldBeFalse();
            binding.GetComponents<object>().AsList().ShouldContain(components);
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);

            binding.Components.Clear();
            binding.TryAddComponent(components[0]).IsEmpty.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
            ShouldThrow<ObjectDisposedException>(() =>
            {
                var compiledExpression = binding.Expression;
            });
        }

        [Fact]
        public void MetadataShouldReturnBindingAndExpressionBinding()
        {
            var binding = new ExpressionBinding(EmptyPathObserver.Empty, sources: default, new TestCompiledExpression());
            var context = (IReadOnlyMetadataContext)binding;
            context.Count.ShouldEqual(2);
            context.Contains(BindingMetadata.Binding).ShouldBeTrue();
            context.Contains(BindingMetadata.IsExpressionBinding).ShouldBeTrue();
            context.TryGet(BindingMetadata.Binding, out var b).ShouldBeTrue();
            context.TryGet(BindingMetadata.IsExpressionBinding, out var isMulti).ShouldBeTrue();
            b.ShouldEqual(binding);
            isMulti.ShouldBeTrue();
            context.GetValues().AsList().ShouldEqual(new[] { BindingMetadata.Binding.ToValue(binding), BindingMetadata.IsExpressionBinding.ToValue(true) });
        }

        [Fact]
        public void UpdateTargetShouldUseObserverValue()
        {
            ExpressionBinding? binding = null;
            var targetObj = new object();
            var sourceObj = new object();
            object sourceValue = "2";
            object expressionValue = "3";
            int targetSet = 0, sourceGet = 0, expressionInvoke = 0;

            var expression = new TestCompiledExpression
            {
                Invoke = (list, context) =>
                {
                    ++expressionInvoke;
                    list.AsList().ShouldEqual(new[] { new ParameterValue(sourceValue.GetType(), sourceValue) });
                    context.ShouldEqual(binding);
                    return expressionValue;
                }
            };
            var targetMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) => throw new NotSupportedException()
            };
            targetMember.SetValue = (o, v, context) =>
            {
                ++targetSet;
                o.ShouldEqual(targetObj);
                v.ShouldEqual(targetMember.MemberType == MemberType.Event ? binding : expressionValue);
                context.ShouldEqual(binding);
            };
            var sourceMember = new TestAccessorMemberInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++sourceGet;
                    o.ShouldEqual(sourceObj);
                    context.ShouldEqual(binding);
                    return sourceValue;
                },
                SetValue = (o, v, context) => throw new NotSupportedException()
            };
            var target = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(targetObj, targetMember)
            };
            var source = new TestMemberPathObserver
            {
                GetLastMember = metadata => new MemberPathLastMember(sourceObj, sourceMember)
            };

            binding = new ExpressionBinding(target, ItemOrArray.FromItem<object?>(source), expression);
            binding.Expression.ShouldEqual(expression);
            binding.UpdateTarget();
            targetSet.ShouldEqual(1);
            sourceGet.ShouldEqual(1);
            expressionInvoke.ShouldEqual(1);

            targetMember.MemberType = MemberType.Event;
            binding.UpdateTarget();
            targetSet.ShouldEqual(2);
            sourceGet.ShouldEqual(1);
            expressionInvoke.ShouldEqual(1);

            binding.Invoke().ShouldEqual(expressionValue);
            sourceGet.ShouldEqual(2);
            expressionInvoke.ShouldEqual(2);
        }

        protected override IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);
    }
}