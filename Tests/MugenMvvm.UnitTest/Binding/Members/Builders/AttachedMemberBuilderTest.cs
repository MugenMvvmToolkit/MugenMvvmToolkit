using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Builders
{
    public class AttachedMemberBuilderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void EventShouldBuildEvent1()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var build = AttachedMemberBuilder.Event<object>(name, declaredType, memberType).Build();
            build.MemberType.ShouldEqual(MemberType.Event);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void EventShouldBuildEvent2()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            BindableEventDescriptor<AttachedMemberBuilderTest> descriptor = name;
            var build = descriptor.ToBuilder(memberType).Build();
            build.MemberType.ShouldEqual(MemberType.Event);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void PropertyShouldBuildProperty1()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var build = AttachedMemberBuilder.Property<object, object>(name, declaredType, memberType).Build();
            build.MemberType.ShouldEqual(MemberType.Accessor);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void PropertyShouldBuildProperty2()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            BindablePropertyDescriptor<AttachedMemberBuilderTest, Action> descriptor = name;
            var build = descriptor.ToBuilder().Build();
            build.MemberType.ShouldEqual(MemberType.Accessor);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void MethodShouldBuildMethod1()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var build = AttachedMemberBuilder.Method<object, object>(name, declaredType, memberType).InvokeHandler((member, target, args, metadata) => null!).Build();
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void MethodShouldBuildMethod2()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            BindableMethodDescriptor<AttachedMemberBuilderTest, Action> descriptor = new BindableMethodDescriptor<AttachedMemberBuilderTest, Action>(name, Default.Array<Type>());
            var build = descriptor.ToBuilder().InvokeHandler((member, target, args, metadata) => null!).Build();
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void ParameterShouldBuildParameter1()
        {
            string name = "t";
            var type = GetType();
            var parameterInfo = AttachedMemberBuilder.Parameter(name, type).Build();
            parameterInfo.ParameterType.ShouldEqual(type);
            parameterInfo.Name.ShouldEqual(name);
        }
        
        [Fact]
        public void ParameterShouldBuildParameter2()
        {
            string name = "t";
            var type = GetType();
            var parameterInfo = AttachedMemberBuilder.Parameter<AttachedMemberBuilderTest>(name).Build();
            parameterInfo.ParameterType.ShouldEqual(type);
            parameterInfo.Name.ShouldEqual(name);
        }

        #endregion
    }
}