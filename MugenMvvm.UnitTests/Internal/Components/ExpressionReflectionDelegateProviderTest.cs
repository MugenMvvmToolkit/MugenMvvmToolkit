using System;
using System.Reflection;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ExpressionReflectionDelegateProviderTest : UnitTestBase
    {
        public ExpressionReflectionDelegateProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void CanCreateDelegateTest()
        {
            ReflectionManager.CanCreateDelegate(typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethod))!).ShouldBeTrue();
            ReflectionManager.CanCreateDelegate(typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodGeneric))!).ShouldBeTrue();
            ReflectionManager.CanCreateDelegate(typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodStatic))!).ShouldBeTrue();
            ReflectionManager.CanCreateDelegate(typeof(EventHandler), typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!).ShouldBeFalse();
        }

        [Fact]
        public void TryCreateDelegateTest()
        {
            var target = new TestMethodClass();
            var handler = (EventHandler)ReflectionManager.TryCreateDelegate(typeof(EventHandler), target,
                typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethod))!)!;
            handler.Invoke(this, EventArgs.Empty);
            target.Args![0].ShouldEqual(this);
            target.Args![1].ShouldEqual(EventArgs.Empty);

            target.Args = null;
            handler = (EventHandler)ReflectionManager.TryCreateDelegate(typeof(EventHandler), target,
                typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodGeneric))!)!;
            handler.Invoke(this, EventArgs.Empty);
            target.Args![0].ShouldEqual(this);
            target.Args![1].ShouldEqual(EventArgs.Empty);

            handler = (EventHandler)ReflectionManager.TryCreateDelegate(typeof(EventHandler), null,
                typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.HandleMethodStatic))!)!;
            handler.Invoke(this, EventArgs.Empty);
            TestMethodClass.ArgsStatic![0].ShouldEqual(this);
            TestMethodClass.ArgsStatic![1].ShouldEqual(EventArgs.Empty);

            ReflectionManager.TryCreateDelegate(typeof(EventHandler), null, typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!).ShouldBeNull();
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate1()
        {
            var activator = ReflectionManager.TryGetActivator(typeof(TestConstructorReflectionClass).GetConstructor(Array.Empty<Type>())!)!;
            var o = (TestConstructorReflectionClass)activator(Array.Empty<object>());
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldBeNull();
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate2()
        {
            const string value = "Test";
            var activator = ReflectionManager.TryGetActivator(typeof(TestConstructorReflectionClass).GetConstructor(new[] { typeof(string) })!)!;
            var o = (TestConstructorReflectionClass)activator(new[] { value });
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldEqual(value);
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate3()
        {
            const string value1 = "Test";
            const int value2 = 2;
            var activator = ReflectionManager.TryGetActivator(typeof(TestConstructorReflectionClass).GetConstructor(new[] { typeof(string), typeof(int) })!)!;
            var o = (TestConstructorReflectionClass)activator(new object[] { value1, value2 });
            o.ConstructorIntValue.ShouldEqual(value2);
            o.ConstructorStringValue.ShouldEqual(value1);
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate4()
        {
            var activator = (Func<TestConstructorReflectionClass>)ReflectionManager.TryGetActivator(typeof(TestConstructorReflectionClass).GetConstructor(Array.Empty<Type>())!,
                typeof(Func<TestConstructorReflectionClass>))!;
            var o = activator();
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldBeNull();
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate5()
        {
            const string value = "Test";
            var activator = (Func<string, TestConstructorReflectionClass>)ReflectionManager
                .TryGetActivator(typeof(TestConstructorReflectionClass).GetConstructor(new[] { typeof(string) })!, typeof(Func<string, TestConstructorReflectionClass>))!;
            var o = activator(value);
            o.ConstructorIntValue.ShouldEqual(0);
            o.ConstructorStringValue.ShouldEqual(value);
        }

        [Fact]
        public void TryGetActivatorShouldGenerateCorrectDelegate6()
        {
            const string value1 = "Test";
            const int value2 = 2;
            var activator = (Func<string, int, TestConstructorReflectionClass>)ReflectionManager
                .TryGetActivator(typeof(TestConstructorReflectionClass).GetConstructor(new[] { typeof(string), typeof(int) })!,
                    typeof(Func<string, int, TestConstructorReflectionClass>))!;
            var o = activator(value1, value2);
            o.ConstructorIntValue.ShouldEqual(value2);
            o.ConstructorStringValue.ShouldEqual(value1);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateField1()
        {
            const string value = "Test";
            var target = new TestMemberGetterSetter();
            var field = target.GetType().GetField(nameof(target.Field), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (Func<TestMemberGetterSetter, string>)ReflectionManager.TryGetMemberGetter(field, typeof(Func<TestMemberGetterSetter, string>))!;
            var setter = (Action<TestMemberGetterSetter, string>)ReflectionManager.TryGetMemberSetter(field, typeof(Action<TestMemberGetterSetter, string>))!;
            getter(target).ShouldEqual(target.Field);

            setter.Invoke(target, value);
            target.Field.ShouldEqual(value);
            getter(target).ShouldEqual(target.Field);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateField2()
        {
            const string value = "Test";
            var target = new TestMemberGetterSetterValue();
            var field = target.GetType().GetField(nameof(target.Field), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (RefGetter<TestMemberGetterSetterValue, string>)ReflectionManager.TryGetMemberGetter(field, typeof(RefGetter<TestMemberGetterSetterValue, string>))!;
            var setter = (RefSetter<TestMemberGetterSetterValue, string>)ReflectionManager.TryGetMemberSetter(field, typeof(RefSetter<TestMemberGetterSetterValue, string>))!;
            getter(ref target).ShouldEqual(target.Field);

            setter.Invoke(ref target, value);
            target.Field.ShouldEqual(value);
            getter(ref target).ShouldEqual(target.Field);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateFieldStatic1()
        {
            const string value = "Test";
            var field = typeof(TestMemberGetterSetter).GetField(nameof(TestMemberGetterSetter.FieldStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<string>)ReflectionManager.TryGetMemberGetter(field, typeof(Func<string>))!;
            var setter = (Action<string>)ReflectionManager.TryGetMemberSetter(field, typeof(Action<string>))!;
            getter().ShouldEqual(TestMemberGetterSetter.FieldStatic);

            setter.Invoke(value);
            TestMemberGetterSetter.FieldStatic.ShouldEqual(value);
            getter().ShouldEqual(TestMemberGetterSetter.FieldStatic);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateFieldStatic2()
        {
            const string value = "Test";
            var field = typeof(TestMemberGetterSetter).GetField(nameof(TestMemberGetterSetter.FieldStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<object?, string>)ReflectionManager.TryGetMemberGetter(field, typeof(Func<object?, string>))!;
            var setter = (Action<object?, string>)ReflectionManager.TryGetMemberSetter(field, typeof(Action<object?, string>))!;
            getter(null).ShouldEqual(TestMemberGetterSetter.FieldStatic);

            setter.Invoke(null, value);
            TestMemberGetterSetter.FieldStatic.ShouldEqual(value);
            getter(null).ShouldEqual(TestMemberGetterSetter.FieldStatic);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateProperty1()
        {
            const string value = "Test";
            var target = new TestMemberGetterSetter();
            var property = target.GetType().GetProperty(nameof(target.Property), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (Func<TestMemberGetterSetter, string>)ReflectionManager.TryGetMemberGetter(property, typeof(Func<TestMemberGetterSetter, string>))!;
            var setter = (Action<TestMemberGetterSetter, string>)ReflectionManager.TryGetMemberSetter(property, typeof(Action<TestMemberGetterSetter, string>))!;
            getter(target).ShouldEqual(target.Property);

            setter.Invoke(target, value);
            target.Property.ShouldEqual(value);
            getter(target).ShouldEqual(target.Property);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegateProperty2()
        {
            const string value = "Test";
            var target = new TestMemberGetterSetterValue();
            var property = target.GetType().GetProperty(nameof(target.Property), BindingFlags.Instance | BindingFlags.Public)!;
            var getter = (RefGetter<TestMemberGetterSetterValue, string>)ReflectionManager.TryGetMemberGetter(property, typeof(RefGetter<TestMemberGetterSetterValue, string>))!;
            var setter = (RefSetter<TestMemberGetterSetterValue, string>)ReflectionManager.TryGetMemberSetter(property, typeof(RefSetter<TestMemberGetterSetterValue, string>))!;
            getter(ref target).ShouldEqual(target.Property);

            setter.Invoke(ref target, value);
            target.Property.ShouldEqual(value);
            getter(ref target).ShouldEqual(target.Property);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegatePropertyStatic1()
        {
            const string value = "Test";
            var property = typeof(TestMemberGetterSetter).GetProperty(nameof(TestMemberGetterSetter.PropertyStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<string>)ReflectionManager.TryGetMemberGetter(property, typeof(Func<string>))!;
            var setter = (Action<string>)ReflectionManager.TryGetMemberSetter(property, typeof(Action<string>))!;
            getter().ShouldEqual(TestMemberGetterSetter.PropertyStatic);

            setter.Invoke(value);
            TestMemberGetterSetter.PropertyStatic.ShouldEqual(value);
            getter().ShouldEqual(TestMemberGetterSetter.PropertyStatic);
        }

        [Fact]
        public void TryGetMemberGetterSetterShouldGenerateCorrectDelegatePropertyStatic2()
        {
            const string value = "Test";
            var property = typeof(TestMemberGetterSetter).GetProperty(nameof(TestMemberGetterSetter.PropertyStatic), BindingFlags.Static | BindingFlags.Public)!;
            var getter = (Func<object?, string>)ReflectionManager.TryGetMemberGetter(property, typeof(Func<object?, string>))!;
            var setter = (Action<object?, string>)ReflectionManager.TryGetMemberSetter(property, typeof(Action<object?, string>))!;
            getter(null).ShouldEqual(TestMemberGetterSetter.PropertyStatic);

            setter.Invoke(null, value);
            TestMemberGetterSetter.PropertyStatic.ShouldEqual(value);
            getter(null).ShouldEqual(TestMemberGetterSetter.PropertyStatic);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate1()
        {
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgs))!;
            var invoker = ReflectionManager.TryGetMethodInvoker(method)!;

            target.IsNoArgsInvoked.ShouldBeFalse();
            invoker.Invoke(target, Array.Empty<object>()).ShouldBeNull();
            target.IsNoArgsInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate2()
        {
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgsStatic))!;
            var invoker = ReflectionManager.TryGetMethodInvoker(method)!;

            TestMethodClass.IsNoArgsStaticInvoked = false;
            invoker.Invoke(null, Array.Empty<object>()).ShouldBeNull();
            TestMethodClass.IsNoArgsStaticInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate3()
        {
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgs))!;
            var invoker = (Action<object>)ReflectionManager.TryGetMethodInvoker(method, typeof(Action<object>))!;

            target.IsNoArgsInvoked.ShouldBeFalse();
            invoker.Invoke(target);
            target.IsNoArgsInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate4()
        {
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.NoArgsStatic))!;
            var invoker = (Action)ReflectionManager.TryGetMethodInvoker(method, typeof(Action))!;

            TestMethodClass.IsNoArgsStaticInvoked = false;
            invoker.Invoke();
            TestMethodClass.IsNoArgsStaticInvoked.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate5()
        {
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!;
            var invoker = ReflectionManager.TryGetMethodInvoker(method)!;

            var args = new object[] { 1, "test" };
            invoker.Invoke(target, args).ShouldEqual(args[1]);
            target.Args.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate6()
        {
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgsStatic))!;
            var invoker = ReflectionManager.TryGetMethodInvoker(method)!;

            var args = new object[] { 1, "test" };
            invoker.Invoke(null, args).ShouldEqual(args[1]);
            TestMethodClass.ArgsStatic.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate7()
        {
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgs))!;
            var invoker = (Func<TestMethodClass, int, string, string>)ReflectionManager.TryGetMethodInvoker(method, typeof(Func<TestMethodClass, int, string, string>))!;

            var args = new object[] { 1, "test" };
            invoker.Invoke(target, (int)args[0], (string)args[1]).ShouldEqual(args[1]);
            target.Args.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate8()
        {
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.MethodWithArgsStatic))!;
            var invoker = (Func<int, string, string>)ReflectionManager.TryGetMethodInvoker(method, typeof(Func<int, string, string>))!;

            var args = new object[] { 1, "test" };
            invoker.Invoke((int)args[0], (string)args[1]).ShouldEqual(args[1]);
            TestMethodClass.ArgsStatic.ShouldEqual(args);
        }

        [Fact]
        public void TryGetMethodInvokerShouldGenerateCorrectDelegate9()
        {
            var target = new TestMethodClass();
            var method = typeof(TestMethodClass).GetMethod(nameof(TestMethodClass.RefMethod))!;
            var invoker = (RefInvoker<TestMethodClass, int>)ReflectionManager.TryGetMethodInvoker(method, typeof(RefInvoker<TestMethodClass, int>))!;

            var value = 0;
            var resultValue = 10;
            invoker.Invoke(target, ref value, resultValue);
            value.ShouldEqual(resultValue);
        }

        protected override IReflectionManager GetReflectionManager()
        {
            var reflectionManager = new ReflectionManager(ComponentCollectionManager);
            reflectionManager.AddComponent(new ExpressionReflectionDelegateProvider());
            return reflectionManager;
        }

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
                ArgsStatic = new object[] { value1, value2 };
                return value2;
            }

            public static void HandleMethodStatic(object sender, object value) => ArgsStatic = new[] { sender, value };

            public void NoArgs() => IsNoArgsInvoked = true;

            public object MethodWithArgs(int value1, string value2)
            {
                Args = new object[] { value1, value2 };
                return value2;
            }

            public void RefMethod(ref int value, int result) => value = result;

            public void HandleMethod(object sender, object value) => Args = new[] { sender, value };

            public void HandleMethodGeneric<T>(object sender, T value) => Args = new[] { sender, value };
        }
    }
}