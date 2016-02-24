using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Accessors;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Accessors
{
    [TestClass]
    public class MultiBindingSourceAccessorTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void GetValueShouldReturnActualValue()
        {
            bool isInvoked = false;
            var memberMock = new BindingMemberInfoMock();
            var sourceModel = new BindingSourceModel();
            string propertyName = GetMemberPath<BindingSourceModel>(model => model.IntProperty);
            var valueAccessor = new MultiBindingSourceAccessor(new[] { CreateSource(sourceModel, propertyName) },
                (context, list) =>
                {
                    isInvoked = true;
                    context.ShouldEqual(EmptyContext);
                    list.Single().ShouldEqual(sourceModel.IntProperty);
                    return sourceModel.IntProperty;
                }, EmptyContext);
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(sourceModel.IntProperty);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            sourceModel.IntProperty = int.MaxValue;
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(sourceModel.IntProperty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void GetValueShouldReturnActualValueDoubleSource()
        {
            const string value = "100";
            bool isInvoked = false;
            var memberMock = new BindingMemberInfoMock();
            var sourceModel = new BindingSourceModel();
            string propertyName1 = GetMemberPath<BindingSourceModel>(model => model.IntProperty);
            string propertyName2 = GetMemberPath<BindingSourceModel>(model => model["test"]);
            var valueAccessor = new MultiBindingSourceAccessor(new[]
            {
                CreateSource(sourceModel, propertyName1),
                CreateSource(sourceModel, propertyName2),
            },
                (context, list) =>
                {
                    list.Count.ShouldEqual(2);
                    isInvoked = true;
                    context.ShouldEqual(EmptyContext);
                    list[0].ShouldEqual(sourceModel.IntProperty);
                    list[1].ShouldEqual(sourceModel["test"]);
                    return value;
                }, EmptyContext);
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(value);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            sourceModel["test"] = propertyName1;
            sourceModel.IntProperty = int.MaxValue;
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(value);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void GetValueShouldThrowExceptionInvalidValueIfFlagTrue()
        {
            var memberMock = new BindingMemberInfoMock();
            var sourceModel = new BindingSourceModel();
            const string propertyName = "invalid";
            var valueAccessor = new MultiBindingSourceAccessor(new[] { CreateSource(sourceModel, propertyName) },
                (context, list) => null, EmptyContext);
            ShouldThrow(() => valueAccessor.GetValue(memberMock, EmptyContext, true));
        }

        [TestMethod]
        public void GetEventValueShouldAlwaysReturnBindingMemberValue()
        {
            var memberMock = new BindingMemberInfoMock { MemberType = BindingMemberType.Event };
            var source = new BindingSourceModel();
            var accessor = new MultiBindingSourceAccessor(new[] { CreateSource(source, BindingSourceModel.EventName) },
                    (context, list) => memberMock, EmptyContext);
            var memberValue = (BindingActionValue)accessor.GetValue(memberMock, EmptyContext, true);
            memberValue.GetValue(new object[] { null, null }).ShouldEqual(memberMock);
        }

        [TestMethod]
        public void GetValueShouldReturnValueUsingConverterSource()
        {
            bool converterInvoked = false;
            var memberMock = new BindingMemberInfoMock
            {
                Type = typeof(int)
            };
            CultureInfo culture = CultureInfo.InvariantCulture;
            var parameter = new object();
            var sourceModel = new BindingSourceModel();
            var converterMock = new ValueConverterCoreMock
            {
                Convert = (o, type, arg3, args) =>
                {
                    converterInvoked = true;
                    o.ShouldEqual(sourceModel.IntProperty);
                    type.ShouldEqual(typeof(int));
                    arg3.ShouldEqual(parameter);
                    args.ShouldEqual(culture);
                    return int.MaxValue;
                }
            };
            var dataContext = new DataContext
            {
                {BindingBuilderConstants.Converter, d => converterMock},
                {BindingBuilderConstants.ConverterCulture, d => culture},
                {BindingBuilderConstants.ConverterParameter, d => parameter}
            };

            string propertyName = GetMemberPath<BindingSourceModel>(model => model.IntProperty);
            var valueAccessor = new MultiBindingSourceAccessor(new[] { CreateSource(sourceModel, propertyName) },
                (context, list) => list.Single(), dataContext);
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(int.MaxValue);
            converterInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void GetValueShouldReturnValueUsingConverterSourceDoubleSource()
        {
            bool converterInvoked = false;
            var memberMock = new BindingMemberInfoMock
            {
                Type = typeof(int)
            };
            CultureInfo culture = CultureInfo.InvariantCulture;
            var parameter = new object();
            var sourceModel = new BindingSourceModel();
            var converterMock = new ValueConverterCoreMock
            {
                Convert = (o, type, arg3, arg4) =>
                {
                    converterInvoked = true;
                    o.ShouldEqual(sourceModel.IntProperty.ToString() + sourceModel.ObjectProperty);
                    type.ShouldEqual(typeof(int));
                    arg3.ShouldEqual(parameter);
                    arg4.ShouldEqual(culture);
                    return int.MaxValue;
                }
            };
            var dataContext = new DataContext
            {
                {BindingBuilderConstants.Converter, d => converterMock},
                {BindingBuilderConstants.ConverterCulture, d => culture},
                {BindingBuilderConstants.ConverterParameter, d => parameter}
            };

            string propertyName = GetMemberPath<BindingSourceModel>(model => model.IntProperty);
            var valueAccessor = new MultiBindingSourceAccessor(new[]
            {
                CreateSource(sourceModel, propertyName),
                CreateSource(sourceModel, GetMemberPath(sourceModel, model => model.ObjectProperty)),
            },
                (context, list) => list[0].ToString() + list[1], dataContext);
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(int.MaxValue);
            converterInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void GetValueShouldReturnValueUsingFallbackValueSource()
        {
            const int fallback = 0;
            var memberMock = new BindingMemberInfoMock();
            var sourceModel = new BindingSourceModel { ObjectProperty = BindingConstants.UnsetValue };
            var dataContext = new DataContext
            {
                {BindingBuilderConstants.Fallback, d => fallback},
            };

            string propertyName = GetMemberPath<BindingSourceModel>(model => model.ObjectProperty);
            var valueAccessor = new MultiBindingSourceAccessor(new[] { CreateSource(sourceModel, propertyName) },
                (context, list) => list.Single(), dataContext);
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(fallback);
        }

        [TestMethod]
        public void GetValueShouldReturnValueUsingFallbackValueSourceDoubleSource()
        {
            const int fallback = 0;
            var memberMock = new BindingMemberInfoMock();
            var sourceModel = new BindingSourceModel { ObjectProperty = BindingConstants.UnsetValue };
            var dataContext = new DataContext
            {
                {BindingBuilderConstants.Fallback, d => fallback},
            };

            string propertyName = GetMemberPath<BindingSourceModel>(model => model.ObjectProperty);
            var valueAccessor = new MultiBindingSourceAccessor(new[]
            {
                CreateSource(sourceModel, propertyName),
                CreateSource(sourceModel, GetMemberPath(sourceModel, model => model.IntProperty)),
            }, (context, list) => list[0], dataContext);
            valueAccessor.GetValue(memberMock, EmptyContext, true).ShouldEqual(fallback);
        }

        [TestMethod]
        public void GetValueShouldReturnValueUsingTargetNullValueSource()
        {
            const int targetNullValue = 0;
            var memberMock = new BindingMemberInfoMock();
            var sourceModel = new BindingSourceModel { ObjectProperty = null };
            var dataContext = new DataContext
            {
                {BindingBuilderConstants.TargetNullValue, targetNullValue},
            };

            string propertyName = GetMemberPath<BindingSourceModel>(model => model.ObjectProperty);
            var valueAccessor = new MultiBindingSourceAccessor(new[] { CreateSource(sourceModel, propertyName) },
               (context, list) => list.Single(), dataContext);
            valueAccessor.GetValue(memberMock, dataContext, true).ShouldEqual(targetNullValue);
        }

        private static IObserver CreateSource(object model, string path, bool hasStablePath = false, bool observable = true, bool optional = false)
        {
            return new MultiPathObserver(model, new BindingPath(path), false, hasStablePath, observable, optional);
        }

        #endregion
    }
}
