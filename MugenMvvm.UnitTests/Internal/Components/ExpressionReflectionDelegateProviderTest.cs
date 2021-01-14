﻿using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ExpressionReflectionDelegateProviderTest : UnitTestBase
    {
        private delegate T RefGetter<TTarget, T>(ref TTarget target);

        private delegate void RefSetter<TTarget, T>(ref TTarget target, T value);

        private delegate void RefInvoker<TTarget, T>(TTarget target, ref T value, T result);

        public class TestMemberGetterSetter
        {
            public static string? FieldStatic;

            public string? Field;

            public static string? PropertyStatic { get; set; }

            public string? Property { get; set; }
        }

        public struct TestMemberGetterSetterValue
        {
            public string? Field;

            public string? Property { get; set; }
        }

        public class TestConstructorReflectionClass
        {
            public TestConstructorReflectionClass(string constructorStringValue)
            {
                ConstructorStringValue = constructorStringValue;
            }

            public TestConstructorReflectionClass(string constructorStringValue, int constructorIntValue)
            {
                ConstructorStringValue = constructorStringValue;
                ConstructorIntValue = constructorIntValue;
            }

            public TestConstructorReflectionClass()
            {
            }

            public string? ConstructorStringValue { get; }

            public int ConstructorIntValue { get; }
        }

        public class TestMethodClass
        {
            public static bool IsNoArgsStaticInvoked { get; set; }

            public static object?[]? ArgsStatic { get; set; }

            public bool IsNoArgsInvoked { get; set; }

            public object?[]? Args { get; set; }

            public static void NoArgsStatic() => IsNoArgsStaticInvoked = true;

            public static object MethodWithArgsStatic(int value1, string value2)
            {
                ArgsStatic = new object[] {value1, value2};
                return value2;
            }

            public static void HandleMethodStatic(object sender, object value) => ArgsStatic = new[] {sender, value};

            public void NoArgs() => IsNoArgsInvoked = true;

            public object MethodWithArgs(int value1, string value2)
            {
                Args = new object[] {value1, value2};
                return value2;
            }

            public void RefMethod(ref int value, int result) => value = result;

            public void HandleMethod(object sender, object value) => Args = new[] {sender, value};

            public void HandleMethodGeneric<T>(object sender, T value) => Args = new[] {sender, value};
        }

        [Fact]
        public void CanCreateDelegateTest()
        {
            IReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            component.CanCreateDelegate(null!, typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethod))!).ShouldBeTrue();
            component.CanCreateDelegate(null!, typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodGeneric))!).ShouldBeTrue();
            component.CanCreateDelegate(null!, typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodStatic))!).ShouldBeTrue();
            component.CanCreateDelegate(null!, typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!).ShouldBeFalse();
        }

        [Fact]
        public void TryCreateDelegateTest()
        {
            IReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMethodClass();
            var handler = (EventHandler) component.TryCreateDelegate(null!, typeof(EventHandler), target,
                typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethod))!)!;
            handler.Invoke(this, EventArgs.Empty);
            target.Args![0].ShouldEqual(this);
            target.Args![1].ShouldEqual(EventArgs.Empty);

            target.Args = null;
            handler = (EventHandler) component.TryCreateDelegate(null!, typeof(EventHandler), target,
                typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodGeneric))!)!;
            handler.Invoke(this, EventArgs.Empty);
            target.Args![0].ShouldEqual(this);
            target.Args![1].ShouldEqual(EventArgs.Empty);

            handler = (EventHandler) component.TryCreateDelegate(null!, typeof(EventHandler), null,
                typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodStatic))!)!;
            handler.Invoke(this, EventArgs.Empty);
            TestMethodClass.ArgsStatic![0].ShouldEqual(this);
            TestMethodClass.ArgsStatic![1].ShouldEqual(EventArgs.Empty);

            component.TryCreateDelegate(null!, typeof(EventHandler), null, typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!).ShouldBeNull();
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate1()
        {
            IActivatorReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var activator = component.TryGetActivator(null!, typeof(TestConstructorReflectionClass).GetConstructor(Default.Array<Type>())!)!;
            var o = (TestConstructorReflectionClass) activator(Default.Array<object>());
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldBeNull();
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate2()
        {
            const string value = "Test";
            IActivatorReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var activator = component.TryGetActivator(null!, typeof(TestConstructorReflectionClass).GetConstructor(new[] {typeof(string)})!)!;
            var o = (TestConstructorReflectionClass) activator(new[] {value});
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldEqual(value);
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate3()
        {
            const string value1 = "Test";
            const int value2 = 2;
            IActivatorReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var activator = component.TryGetActivator(null!, typeof(TestConstructorReflectionClass).GetConstructor(new[] {typeof(string), typeof(int)})!)!;
            var o = (TestConstructorReflectionClass) activator(new object[] {value1, value2});
            o.ConstructorIntValue.ShouldEqual(value2);
            o.ConstructorStringValue.ShouldEqual(value1);
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate4()
        {
            IActivatorReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var activator = (Func<TestConstructorReflectionClass>) component
                .TryGetActivator(null!, typeof(TestConstructorReflectionClass).GetConstructor(Default.Array<Type>())!, typeof(Func<TestConstructorReflectionClass>))!;
            var o = activator();
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldBeNull();
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate5()
        {
            const string value = "Test";
            IActivatorReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var activator = (Func<string, TestConstructorReflectionClass>) component
                .TryGetActivator(null!, typeof(TestConstructorReflectionClass).GetConstructor(new[] {typeof(string)})!, typeof(Func<string, TestConstructorReflectionClass>))!;
            var o = activator(value);
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldEqual(value);
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate6()
        {
            const string value1 = "Test";
            const int value2 = 2;
            IActivatorReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var activator = (Func<string, int, TestConstructorReflectionClass>) component
                .TryGetActivator(null!, typeof(TestConstructorReflectionClass).GetConstructor(new[] {typeof(string), typeof(int)})!,
                    typeof(Func<string, int, TestConstructorReflectionClass>))!;
            var o = activator(value1, value2);
            o.ConstructorIntValue.ShouldEqual(value2);
            o.ConstructorStringValue.ShouldEqual(value1);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateField1()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMemberGetterSetter();
            var field = target.GetType().GetField(nameof(target.Field), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (Func<TestMemberGetterSetter, string>) component.TryGetMemberGetter(null!, field, typeof(Func<TestMemberGetterSetter, string>))!;
            var setter = (Action<TestMemberGetterSetter, string>) component.TryGetMemberSetter(null!, field, typeof(Action<TestMemberGetterSetter, string>))!;
            getter(target).ShouldEqual(target.Field);

            setter.Invoke(target, value);
            target.Field.ShouldEqual(value);
            getter(target).ShouldEqual(target.Field);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateField2()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMemberGetterSetterValue();
            var field = target.GetType().GetField(nameof(target.Field), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (RefGetter<TestMemberGetterSetterValue, string>) component.TryGetMemberGetter(null!, field, typeof(RefGetter<TestMemberGetterSetterValue, string>))!;
            var setter = (RefSetter<TestMemberGetterSetterValue, string>) component.TryGetMemberSetter(null!, field, typeof(RefSetter<TestMemberGetterSetterValue, string>))!;
            getter(ref target).ShouldEqual(target.Field);

            setter.Invoke(ref target, value);
            target.Field.ShouldEqual(value);
            getter(ref target).ShouldEqual(target.Field);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateFieldStatic1()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var field = typeof(TestMemberGetterSetter).GetField(nameof(TestMemberGetterSetter.FieldStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<string>) component.TryGetMemberGetter(null!, field, typeof(Func<string>))!;
            var setter = (Action<string>) component.TryGetMemberSetter(null!, field, typeof(Action<string>))!;
            getter().ShouldEqual(TestMemberGetterSetter.FieldStatic);

            setter.Invoke(value);
            TestMemberGetterSetter.FieldStatic.ShouldEqual(value);
            getter().ShouldEqual(TestMemberGetterSetter.FieldStatic);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateFieldStatic2()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var field = typeof(TestMemberGetterSetter).GetField(nameof(TestMemberGetterSetter.FieldStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<object?, string>) component.TryGetMemberGetter(null!, field, typeof(Func<object?, string>))!;
            var setter = (Action<object?, string>) component.TryGetMemberSetter(null!, field, typeof(Action<object?, string>))!;
            getter(null).ShouldEqual(TestMemberGetterSetter.FieldStatic);

            setter.Invoke(null, value);
            TestMemberGetterSetter.FieldStatic.ShouldEqual(value);
            getter(null).ShouldEqual(TestMemberGetterSetter.FieldStatic);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateProperty1()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMemberGetterSetter();
            var property = target.GetType().GetProperty(nameof(target.Property), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (Func<TestMemberGetterSetter, string>) component.TryGetMemberGetter(null!, property, typeof(Func<TestMemberGetterSetter, string>))!;
            var setter = (Action<TestMemberGetterSetter, string>) component.TryGetMemberSetter(null!, property, typeof(Action<TestMemberGetterSetter, string>))!;
            getter(target).ShouldEqual(target.Property);

            setter.Invoke(target, value);
            target.Property.ShouldEqual(value);
            getter(target).ShouldEqual(target.Property);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateProperty2()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMemberGetterSetterValue();
            var property = target.GetType().GetProperty(nameof(target.Property), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (RefGetter<TestMemberGetterSetterValue, string>) component.TryGetMemberGetter(null!, property, typeof(RefGetter<TestMemberGetterSetterValue, string>))!;
            var setter = (RefSetter<TestMemberGetterSetterValue, string>) component.TryGetMemberSetter(null!, property, typeof(RefSetter<TestMemberGetterSetterValue, string>))!;
            getter(ref target).ShouldEqual(target.Property);

            setter.Invoke(ref target, value);
            target.Property.ShouldEqual(value);
            getter(ref target).ShouldEqual(target.Property);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegatePropertyStatic1()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var property = typeof(TestMemberGetterSetter).GetProperty(nameof(TestMemberGetterSetter.PropertyStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<string>) component.TryGetMemberGetter(null!, property, typeof(Func<string>))!;
            var setter = (Action<string>) component.TryGetMemberSetter(null!, property, typeof(Action<string>))!;
            getter().ShouldEqual(TestMemberGetterSetter.PropertyStatic);

            setter.Invoke(value);
            TestMemberGetterSetter.PropertyStatic.ShouldEqual(value);
            getter().ShouldEqual(TestMemberGetterSetter.PropertyStatic);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegatePropertyStatic2()
        {
            const string value = "Test";
            IMemberReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var property = typeof(TestMemberGetterSetter).GetProperty(nameof(TestMemberGetterSetter.PropertyStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<object?, string>) component.TryGetMemberGetter(null!, property, typeof(Func<object?, string>))!;
            var setter = (Action<object?, string>) component.TryGetMemberSetter(null!, property, typeof(Action<object?, string>))!;
            getter(null).ShouldEqual(TestMemberGetterSetter.PropertyStatic);

            setter.Invoke(null, value);
            TestMemberGetterSetter.PropertyStatic.ShouldEqual(value);
            getter(null).ShouldEqual(TestMemberGetterSetter.PropertyStatic);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate1()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgs))!;
            var invoker = component.TryGetMethodInvoker(null!, method)!;

            target.IsNoArgsInvoked.ShouldBeFalse();
            invoker.Invoke(target, Default.Array<object>()).ShouldBeNull();
            target.IsNoArgsInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate2()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgsStatic))!;
            var invoker = component.TryGetMethodInvoker(null!, method)!;

            TestMethodClass.IsNoArgsStaticInvoked = false;
            invoker.Invoke(null, Default.Array<object>()).ShouldBeNull();
            TestMethodClass.IsNoArgsStaticInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate3()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgs))!;
            var invoker = (Action<object>) component.TryGetMethodInvoker(null!, method, typeof(Action<object>))!;

            target.IsNoArgsInvoked.ShouldBeFalse();
            invoker.Invoke(target);
            target.IsNoArgsInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate4()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgsStatic))!;
            var invoker = (Action) component.TryGetMethodInvoker(null!, method, typeof(Action))!;

            TestMethodClass.IsNoArgsStaticInvoked = false;
            invoker.Invoke();
            TestMethodClass.IsNoArgsStaticInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate5()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!;
            var invoker = component.TryGetMethodInvoker(null!, method)!;

            var args = new object[] {1, "test"};
            invoker.Invoke(target, args).ShouldEqual(args[1]);
            target.Args.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate6()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgsStatic))!;
            var invoker = component.TryGetMethodInvoker(null!, method)!;

            var args = new object[] {1, "test"};
            invoker.Invoke(null, args).ShouldEqual(args[1]);
            TestMethodClass.ArgsStatic.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate7()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!;
            var invoker = (Func<TestMethodClass, int, string, string>) component.TryGetMethodInvoker(null!, method, typeof(Func<TestMethodClass, int, string, string>))!;

            var args = new object[] {1, "test"};
            invoker.Invoke(target, (int) args[0], (string) args[1]).ShouldEqual(args[1]);
            target.Args.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate8()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgsStatic))!;
            var invoker = (Func<int, string, string>) component.TryGetMethodInvoker(null!, method, typeof(Func<int, string, string>))!;

            var args = new object[] {1, "test"};
            invoker.Invoke((int) args[0], (string) args[1]).ShouldEqual(args[1]);
            TestMethodClass.ArgsStatic.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate9()
        {
            IMethodReflectionDelegateProviderComponent component = new ExpressionReflectionDelegateProvider();
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.RefMethod))!;
            var invoker = (RefInvoker<TestMethodClass, int>) component.TryGetMethodInvoker(null!, method, typeof(RefInvoker<TestMethodClass, int>))!;

            var value = 0;
            var resultValue = 10;
            invoker.Invoke(target, ref value, resultValue);
            value.ShouldEqual(resultValue);
        }
    }
}