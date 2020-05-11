using System;
using System.Linq;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Models;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core
{
    public class BindingParameterValueTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(BindingParameterValue).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var dispose = false;
            var testDisposable = new TestDisposable
            {
                Dispose = () => dispose = true
            };
            var parameterValue = new BindingParameterValue(testDisposable, null);
            parameterValue.GetValue<object>(DefaultMetadata).ShouldEqual(testDisposable);
            parameterValue.GetValue<TestDisposable>(DefaultMetadata).ShouldEqual(testDisposable);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(DefaultMetadata));
            parameterValue.Dispose();
            dispose.ShouldBeFalse();
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var dispose = false;
            var target = new object();
            var result = this;
            var member = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var observer = new TestMemberPathObserver
            {
                Dispose = () => dispose = true,
                GetLastMember = metadata =>
                {
                    metadata.ShouldEqual(DefaultMetadata);
                    return new MemberPathLastMember(target, member);
                }
            };
            var parameterValue = new BindingParameterValue(observer, null);
            parameterValue.GetValue<object>(DefaultMetadata).ShouldEqual(result);
            parameterValue.GetValue<BindingParameterValueTest>(DefaultMetadata).ShouldEqual(result);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(DefaultMetadata));
            parameterValue.Dispose();
            dispose.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues3()
        {
            var dispose = false;
            var result = this;
            var values = new object[] { new object(), "", 1 };
            var compiledExpression = new TestCompiledExpression
            {
                Dispose = () => dispose = true,
                Invoke = (list, context) =>
                {
                    list.ToArray().SequenceEqual(values.Select(o => new ExpressionValue(o.GetType(), o))).ShouldBeTrue();
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var parameterValue = new BindingParameterValue(values, compiledExpression);
            parameterValue.GetValue<object>(DefaultMetadata).ShouldEqual(result);
            parameterValue.GetValue<BindingParameterValueTest>(DefaultMetadata).ShouldEqual(result);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(DefaultMetadata));
            parameterValue.Dispose();
            dispose.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues4()
        {
            int invokeCount = 0;
            var disposeExpression = false;
            var disposeObserver = false;
            var target = new object();
            var memberResult = BindingMetadata.UnsetValue;
            var member = new TestMemberAccessorInfo
            {
                GetValue = (o, context) =>
                {
                    context.ShouldEqual(DefaultMetadata);
                    return memberResult;
                }
            };
            var observer = new TestMemberPathObserver
            {
                Dispose = () => disposeObserver = true,
                GetLastMember = metadata =>
                {
                    metadata.ShouldEqual(DefaultMetadata);
                    return new MemberPathLastMember(target, member);
                }
            };

            var result = this;
            var values = new object[] { observer, new object(), "", 1 };
            var compiledExpression = new TestCompiledExpression
            {
                Dispose = () => disposeExpression = true,
                Invoke = (list, context) =>
                {
                    ++invokeCount;
                    list.ToArray().SequenceEqual(values.Select(o =>
                    {
                        if (o is IMemberPathObserver ob)
                            o = ob.GetLastMember(DefaultMetadata).GetValueOrThrow(DefaultMetadata)!;
                        return new ExpressionValue(o.GetType(), o);
                    })).ShouldBeTrue();
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var parameterValue = new BindingParameterValue(values, compiledExpression);
            parameterValue.GetValue<object>(DefaultMetadata).ShouldEqual(BindingMetadata.UnsetValue);
            ShouldThrow<InvalidCastException>(() => parameterValue.GetValue<string>(DefaultMetadata));
            invokeCount.ShouldEqual(0);

            memberResult = "";
            parameterValue.GetValue<object>(DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            parameterValue.Dispose();
            disposeExpression.ShouldBeTrue();
            disposeObserver.ShouldBeTrue();
        }

        #endregion
    }
}