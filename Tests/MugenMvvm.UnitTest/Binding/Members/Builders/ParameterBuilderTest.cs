using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members.Builders;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Builders
{
    public class ParameterBuilderTest
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            string name = "n";
            Type type = typeof(int);
            object underlyingMember = new object();
            var hasDefaultValue = true;
            object defaultValue = "";
            var requestType = typeof(int);
            var result = true;
            IParameterInfo parameter = null;
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
            var parameter = new ParameterBuilder("", typeof(object)).IsParamsArray().Build();
            parameter.IsDefined(typeof(ParamArrayAttribute)).ShouldBeTrue();
        }

        #endregion
    }
}