﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Parsing;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Build
{
    public class BindingBuilderToTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var from = "from";
            var to = "to";
            var param1 = "p1";
            var param2 = "p2";
            var builder = new BindingBuilderTo<string, object>(new BindingBuilderFrom<string, object>(from), to, default);
            var request = (ExpressionConverterRequest) builder;
            request.Target.ShouldEqual(from);
            request.Source.ShouldEqual(to);
            request.Parameters.AsList().Count.ShouldEqual(0);

            request = builder.BindingParameter(param1, param1);
            request.Target.ShouldEqual(from);
            request.Source.ShouldEqual(to);
            request.Parameters.AsList().SequenceEqual(new[] {new KeyValuePair<string?, object>(param1, param1)}).ShouldBeTrue();

            request = builder.BindingParameter(param2, param2);
            request.Target.ShouldEqual(from);
            request.Source.ShouldEqual(to);
            request.Parameters.AsList().SequenceEqual(new[] {new KeyValuePair<string?, object>(param1, param1), new KeyValuePair<string?, object>(param2, param2)}).ShouldBeTrue();
        }

        #endregion
    }
}