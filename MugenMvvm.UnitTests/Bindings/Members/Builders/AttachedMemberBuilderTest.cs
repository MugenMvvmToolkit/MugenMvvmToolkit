using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Bindings.Members.Descriptors;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Builders
{
    public class AttachedMemberBuilderTest : UnitTestBase
    {
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
            var build = descriptor.GetBuilder(memberType).Build();
            build.MemberType.ShouldEqual(MemberType.Event);
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
            var request = new MemberTypesRequest(name, Array.Empty<Type>());
            var descriptor = new BindableMethodDescriptor<AttachedMemberBuilderTest, Action>(request);
            var build = descriptor.GetBuilder().InvokeHandler((member, target, args, metadata) => null!).Build();
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void MethodShouldBuildMethod3()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var request = new MemberTypesRequest(name, new[] { typeof(int) });
            var descriptor = new BindableMethodDescriptor<AttachedMemberBuilderTest, int, Action>(request);
            var build = descriptor.GetBuilder().InvokeHandler((member, target, args, metadata) => null!).Build();
            var parameters = build.GetParameters();
            parameters.Count.ShouldEqual(1);
            parameters[0].ParameterType.ShouldEqual(typeof(int));
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void MethodShouldBuildMethod4()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var request = new MemberTypesRequest(name, new[] { typeof(byte), typeof(char) });
            var descriptor = new BindableMethodDescriptor<AttachedMemberBuilderTest, byte, char, Action>(request);
            var build = descriptor.GetBuilder().InvokeHandler((member, target, args, metadata) => null!).Build();
            var parameters = build.GetParameters();
            parameters.Count.ShouldEqual(2);
            parameters[0].ParameterType.ShouldEqual(typeof(byte));
            parameters[1].ParameterType.ShouldEqual(typeof(char));
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void MethodShouldBuildMethod5()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var request = new MemberTypesRequest(name, new[] { typeof(byte), typeof(char), typeof(short) });
            var descriptor = new BindableMethodDescriptor<AttachedMemberBuilderTest, byte, char, short, Action>(request);
            var build = descriptor.GetBuilder().InvokeHandler((member, target, args, metadata) => null!).Build();
            var parameters = build.GetParameters();
            parameters.Count.ShouldEqual(3);
            parameters[0].ParameterType.ShouldEqual(typeof(byte));
            parameters[1].ParameterType.ShouldEqual(typeof(char));
            parameters[2].ParameterType.ShouldEqual(typeof(short));
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void MethodShouldBuildMethod6()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var request = new MemberTypesRequest(name, new[] { typeof(byte), typeof(char), typeof(short), typeof(int) });
            var descriptor = new BindableMethodDescriptor<AttachedMemberBuilderTest, byte, char, short, int, Action>(request);
            var build = descriptor.GetBuilder().InvokeHandler((member, target, args, metadata) => null!).Build();
            var parameters = build.GetParameters();
            parameters.Count.ShouldEqual(4);
            parameters[0].ParameterType.ShouldEqual(typeof(byte));
            parameters[1].ParameterType.ShouldEqual(typeof(char));
            parameters[2].ParameterType.ShouldEqual(typeof(short));
            parameters[3].ParameterType.ShouldEqual(typeof(int));
            build.MemberType.ShouldEqual(MemberType.Method);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }

        [Fact]
        public void MethodShouldBuildMethod7()
        {
            string name = "t";
            Type declaredType = GetType();
            var memberType = typeof(Action);
            var request = new MemberTypesRequest(name, new[] { typeof(byte), typeof(char), typeof(short), typeof(int), typeof(long) });
            var descriptor = new BindableMethodDescriptor<AttachedMemberBuilderTest, byte, char, short, int, long, Action>(request);
            var build = descriptor.GetBuilder().InvokeHandler((member, target, args, metadata) => null!).Build();
            var parameters = build.GetParameters();
            parameters.Count.ShouldEqual(5);
            parameters[0].ParameterType.ShouldEqual(typeof(byte));
            parameters[1].ParameterType.ShouldEqual(typeof(char));
            parameters[2].ParameterType.ShouldEqual(typeof(short));
            parameters[3].ParameterType.ShouldEqual(typeof(int));
            parameters[4].ParameterType.ShouldEqual(typeof(long));
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
            var build = descriptor.GetBuilder().Build();
            build.MemberType.ShouldEqual(MemberType.Accessor);
            build.Name.ShouldEqual(name);
            build.DeclaringType.ShouldEqual(declaredType);
            build.Type.ShouldEqual(memberType);
        }
    }
}