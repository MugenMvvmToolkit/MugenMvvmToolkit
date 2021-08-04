using System;
using System.Linq;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Tests.Bindings.Compiling;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Bindings.Observation;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class BindingParameterValueTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var dispose = false;
            var testDisposable = new TestDisposable
            {
                Dispose = () => dispose = true
            };
            var parameterValue = new BindingParameterValue(testDisposable, null);
            parameterValue.GetValue<object>(Metadata).ShouldEqual(testDisposable);
            parameterValue.GetValue<TestDisposable>(Metadata).ShouldEqual(testDisposable);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(Metadata));
            parameterValue.Dispose();
            dispose.ShouldBeFalse();
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var dispose = false;
            var target = new object();
            var result = this;
            var member = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    context.ShouldEqual(Metadata);
                    return result;
                }
            };
            var observer = new TestMemberPathObserver
            {
                Dispose = () => dispose = true,
                GetLastMember = metadata =>
                {
                    metadata.ShouldEqual(Metadata);
                    return new MemberPathLastMember(target, member);
                }
            };
            var parameterValue = new BindingParameterValue(observer, null);
            parameterValue.GetValue<object>(Metadata).ShouldEqual(result);
            parameterValue.GetValue<BindingParameterValueTest>(Metadata).ShouldEqual(result);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(Metadata));
            parameterValue.Dispose();
            dispose.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues3()
        {
            var result = this;
            var values = new[] { new object(), "", 1 };
            var compiledExpression = new TestCompiledExpression
            {
                Invoke = (list, context) =>
                {
                    list.ShouldEqual(values.Select(o => new ParameterValue(o.GetType(), o)));
                    context.ShouldEqual(Metadata);
                    return result;
                }
            };
            var parameterValue = new BindingParameterValue(values, compiledExpression);
            parameterValue.GetValue<object>(Metadata).ShouldEqual(result);
            parameterValue.GetValue<BindingParameterValueTest>(Metadata).ShouldEqual(result);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(Metadata));
            parameterValue.Dispose();
        }

        [Fact]
        public void ConstructorShouldInitializeValues4()
        {
            var invokeCount = 0;
            var disposeObserver = false;
            var target = new object();
            var memberResult = BindingMetadata.UnsetValue;
            var member = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    context.ShouldEqual(Metadata);
                    return memberResult;
                }
            };
            var observer = new TestMemberPathObserver
            {
                Dispose = () => disposeObserver = true,
                GetLastMember = metadata =>
                {
                    metadata.ShouldEqual(Metadata);
                    return new MemberPathLastMember(target, member);
                }
            };

            var result = this;
            var values = new[] { observer, new object(), "", 1 };
            var compiledExpression = new TestCompiledExpression
            {
                Invoke = (list, context) =>
                {
                    ++invokeCount;
                    list.ShouldEqual(values.Select(o =>
                    {
                        if (o is IMemberPathObserver ob)
                            o = ob.GetLastMember(Metadata).GetValueOrThrow(Metadata)!;
                        return new ParameterValue(o.GetType(), o);
                    }));
                    context.ShouldEqual(Metadata);
                    return result;
                }
            };
            var parameterValue = new BindingParameterValue(values, compiledExpression);
            parameterValue.GetValue<object>(Metadata).ShouldEqual(BindingMetadata.UnsetValue);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(Metadata));
            invokeCount.ShouldEqual(0);

            memberResult = "";
            parameterValue.GetValue<object>(Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            parameterValue.Dispose();
            disposeObserver.ShouldBeTrue();
        }

        [Fact]
        public void DefaultShouldBeEmpty() => default(BindingParameterValue).IsEmpty.ShouldBeTrue();
    }
}