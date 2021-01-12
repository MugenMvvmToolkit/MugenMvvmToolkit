using System;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Builders
{
    public class ParameterBuilderTest
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            string name = "n";
            Type type = typeof(int);
            object underlyingMember = new();
            var hasDefaultValue = true;
            object defaultValue = "";
            var requestType = typeof(int);
            var result = true;
            IParameterInfo? parameter = null;
            var invokeCount = 0;
            parameter = new ParameterBuilder(name, type).DefaultValue(defaultValue).IsDefinedHandler((info, t) =>
            {
                ++invokeCount;
                info.ShouldEqual(parameter);
                t.ShouldEqual(requestType);
                return result;
            }).UnderlyingMember(underlyingMember).Build();
            parameter.Name.ShouldEqual(name);
            parameter.HasDefaultValue.ShouldEqual(hasDefaultValue);
            parameter.DefaultValue.ShouldEqual(defaultValue);
            parameter.ParameterType.ShouldEqual(type);
            parameter.UnderlyingParameter.ShouldEqual(underlyingMember);

            parameter.IsDefined(requestType).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            result = false;
            parameter.IsDefined(requestType).ShouldEqual(result);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var parameter = new ParameterBuilder("", typeof(object[])).IsParamsArray().Build();
            parameter.IsDefined(typeof(ParamArrayAttribute)).ShouldBeTrue();
            Assert.Throws<ArgumentException>(() => new ParameterBuilder("", typeof(object)).IsParamsArray());
        }

        [Fact]
        public void ConstructorShouldInitializeValues3()
        {
            var parameter = new ParameterBuilder("", typeof(object)).WithState(this).Build();
            ((DelegateParameterInfo<object?>) parameter).State.ShouldEqual(this);
        }

        #endregion
    }
}