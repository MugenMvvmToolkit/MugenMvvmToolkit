using System;
using MugenMvvm.Bindings.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class DelegateParameterInfoTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            string name = "n";
            Type type = typeof(int);
            object underlyingMember = new();
            var hasDefaultValue = true;
            object defaultValue = "";
            var state = (this, "");
            var requestType = typeof(int);
            var result = true;
            DelegateParameterInfo<(DelegateParameterInfoTest, string)>? parameter = null;
            var invokeCount = 0;
            parameter = new DelegateParameterInfo<(DelegateParameterInfoTest, string)>(name, type, underlyingMember, hasDefaultValue, defaultValue, state, (info, t) =>
            {
                ++invokeCount;
                info.ShouldEqual(parameter);
                t.ShouldEqual(requestType);
                return result;
            });
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
    }
}