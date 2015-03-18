using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Sources;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Parse;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Bindings.Parse
{
    [TestClass]
    public class BindingParserTest : BindingTestBase
    {
        #region Methods

        [TestMethod]
        public void ParserShouldUseTargetBindingContextForSource()
        {
            const string targetPath = "Text";
            const string sourcePath = "IntProperty";
            const string binding = "Text IntProperty";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);

            IBindingSource source = sources.Single().Invoke(context);
            BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            var sourceObj = new BindingSourceModel();
            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = sourceObj;
            BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
        }

        [TestMethod]
        public void ParserShouldParseSingleExpression()
        {
            const string targetPath = "Text";
            const string sourcePath = "SourceText";
            const string binding = "Text SourceText";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath targetBPath = context.GetData(BindingBuilderConstants.TargetPath);
            targetBPath.Path.ShouldEqual(targetPath);

            var target = new object();
            context.Add(BindingBuilderConstants.Target, target);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            BindingSourceShouldBeValidDataContext(target, source, sourcePath);
        }

        [TestMethod]
        public void ParserShouldParseRelativeSourceSelfExpression1()
        {
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string binding = "Text {RelativeSource Self, Path=StringProperty}";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var targetObj = new BindingSourceModel();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            BindingSourceShouldBeValid(source, sourcePath, targetObj);
        }

        [TestMethod]
        public void ParserShouldParseRelativeSourceSelfExpression2()
        {
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string binding = "Text {RelativeSource Self}.StringProperty";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var targetObj = new BindingSourceModel();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            BindingSourceShouldBeValid(source, sourcePath, targetObj);
        }

        [TestMethod]
        public void ParserShouldParseRelativeSourceExpression1()
        {
            bool isInvoked = false;
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string relativeSourceType = "type";
            const uint level = 10;
            const string binding = "Text {RelativeSource type, Path=StringProperty, Level=10}";
            var targetObj = new BindingSourceModel();
            var relativeObj = new BindingSourceModel();
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var providerMock = new ObserverProviderMock
            {
                Observe = (o, path, arg3) => new MultiPathObserver(o, path, arg3)
            };
            var treeManagerMock = new VisualTreeManagerMock
            {
                FindRelativeSource = (o, s, arg3) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(relativeSourceType);
                    arg3.ShouldEqual(level);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };


            IBindingParser bindingParser = CreateBindingParser(treeManagerMock, null, providerMock);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            source.Path.Path.ShouldContain(sourcePath);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ParserShouldParseRelativeSourceExpression2()
        {
            bool isInvoked = false;
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string relativeSourceType = "type";
            const uint level = 10;
            const string binding = "Text {RelativeSource type, Level=10}.StringProperty";
            var targetObj = new BindingSourceModel();
            var relativeObj = new BindingSourceModel();
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var providerMock = new ObserverProviderMock
            {
                Observe = (o, path, arg3) => new MultiPathObserver(o, path, arg3)
            };
            var treeManagerMock = new VisualTreeManagerMock
            {
                FindRelativeSource = (o, s, arg3) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(relativeSourceType);
                    arg3.ShouldEqual(level);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };


            IBindingParser bindingParser = CreateBindingParser(treeManagerMock, null, providerMock);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            source.Path.Path.ShouldContain(sourcePath);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ParserShouldParseRelativeSourceExpression3()
        {
            bool isInvoked = false;
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string relativeSourceType = "type";
            const uint level = 10;
            const string binding = "Text $Relative(type, 10).StringProperty";
            var targetObj = new BindingSourceModel();
            var relativeObj = new BindingSourceModel();
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var providerMock = new ObserverProviderMock
            {
                Observe = (o, path, arg3) => new MultiPathObserver(o, path, arg3)
            };
            var treeManagerMock = new VisualTreeManagerMock
            {
                FindRelativeSource = (o, s, arg3) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(relativeSourceType);
                    arg3.ShouldEqual(level);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };


            IBindingParser bindingParser = CreateBindingParser(treeManagerMock, null, providerMock);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            source.Path.Path.ShouldContain(sourcePath);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ParserShouldParseElementSourceExpression1()
        {
            bool isInvoked = false;
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string relativeSourceName = "type";
            const string binding = "Text {ElementSource type, Path=StringProperty}";

            var targetObj = new BindingSourceModel();
            var relativeObj = new BindingSourceModel();
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var providerMock = new ObserverProviderMock
            {
                Observe = (o, path, arg3) => new MultiPathObserver(o, path, arg3)
            };
            var treeManagerMock = new VisualTreeManagerMock
            {
                FindByName = (o, s) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(relativeSourceName);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };

            IBindingParser bindingParser = CreateBindingParser(treeManagerMock, null, providerMock);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            source.Path.Path.ShouldContain(sourcePath);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ParserShouldParseElementSourceExpression2()
        {
            bool isInvoked = false;
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string relativeSourceName = "type";
            const string binding = "Text {ElementSource type}.StringProperty";

            var targetObj = new BindingSourceModel();
            var relativeObj = new BindingSourceModel();
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var providerMock = new ObserverProviderMock
            {
                Observe = (o, path, arg3) => new MultiPathObserver(o, path, arg3)
            };
            var treeManagerMock = new VisualTreeManagerMock
            {
                FindByName = (o, s) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(relativeSourceName);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };

            IBindingParser bindingParser = CreateBindingParser(treeManagerMock, null, providerMock);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            source.Path.Path.ShouldContain(sourcePath);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ParserShouldParseElementSourceExpression3()
        {
            bool isInvoked = false;
            const string targetPath = "Text";
            const string sourcePath = "StringProperty";
            const string relativeSourceName = "type";
            const string binding = "Text $Element(type).StringProperty";

            var targetObj = new BindingSourceModel();
            var relativeObj = new BindingSourceModel();
            IEventListener eventListener = null;
            var memberMock = new BindingMemberInfoMock
            {
                TryObserveMember = (o, listener) =>
                {
                    eventListener = listener;
                    return null;
                }
            };
            var providerMock = new ObserverProviderMock
            {
                Observe = (o, path, arg3) => new MultiPathObserver(o, path, arg3)
            };
            var treeManagerMock = new VisualTreeManagerMock
            {
                FindByName = (o, s) =>
                {
                    o.ShouldEqual(targetObj);
                    s.ShouldEqual(relativeSourceName);
                    isInvoked = true;
                    return relativeObj;
                },
                GetRootMember = type => memberMock
            };

            IBindingParser bindingParser = CreateBindingParser(treeManagerMock, null, providerMock);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            IBindingSource source = sources.Single().Invoke(context);
            source.Path.Path.ShouldContain(sourcePath);
            isInvoked.ShouldBeTrue();

            isInvoked = false;
            eventListener.ShouldNotBeNull();
            eventListener.Handle(this, EventArgs.Empty);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithFormatMethod1()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string format = "{0} - {1}";
            const string binding = "Text $Format('{0} - {1}', SourceText1, SourceText2)";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { 1, 2 }).ShouldEqual(string.Format(format, 1, 2));
            expression(context, new object[] { sourcePath1, sourcePath2 })
                .ShouldEqual(string.Format(format, sourcePath1, sourcePath2));

            var objTarget = new object();
            context.Add(BindingBuilderConstants.Target, objTarget);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(objTarget, sources[0].Invoke(context),
                sourcePath1);
            BindingSourceShouldBeValidDataContext(objTarget, sources[1].Invoke(context),
                sourcePath2);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithFormatMethod2()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string format = "{0} - {1}";
            const string binding = "Text $Format('{0} - {1}', SourceText1, SourceText2).Length";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { 1, 2 }).ShouldEqual(string.Format(format, 1, 2).Length);
            expression(context, new object[] { sourcePath1, sourcePath2 })
                .ShouldEqual(string.Format(format, sourcePath1, sourcePath2).Length);

            var objTarget = new object();
            context.Add(BindingBuilderConstants.Target, objTarget);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(objTarget, sources[0].Invoke(context),
                sourcePath1);
            BindingSourceShouldBeValidDataContext(objTarget, sources[1].Invoke(context),
                sourcePath2);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression0()
        {
            const string targetPath = "Text";
            const string binding = @"Text SourceText1 == 'a'";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { 'a' }).ShouldEqual(true);
            expression(context, new object[] { 'b' }).ShouldEqual(false);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), "SourceText1");
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression1()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string binding = "Text SourceText1 + SourceText2";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { 1, 2 }).ShouldEqual(3);
            expression(context, new object[] { sourcePath1, sourcePath2 }).ShouldEqual(sourcePath1 + sourcePath2);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression2()
        {
            Func<object[], object> expressionFunc = objects => ((string)objects[0]).IndexOf("1") == 0
                ? string.Format("{0} - {1}", objects[1], objects[2])
                : ((int)objects[1]) >= 10 ? "test" : null ?? "value";

            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string sourcePath3 = "SourceText3";
            const string binding =
                "Text SourceText1.IndexOf('1') == 0 ? $Format('{0} - {1}', SourceText2, SourceText3) : SourceText2 >= 10 ? 'test' : null ?? 'value'";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            var args = new object[] { "1", 1, 2 };
            expression(context, args).ShouldEqual(expressionFunc(args));

            args = new object[] { "0", 1, 2 };
            expression(context, args).ShouldEqual(expressionFunc(args));

            args = new object[] { "0", 11, 2 };
            expression(context, args).ShouldEqual(expressionFunc(args));

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
            BindingSourceShouldBeValidDataContext(targetObj, sources[2].Invoke(context), sourcePath3);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression3()
        {
            Func<object[], object> expressionFunc = objects => ((((int)objects[0]) - (((int)objects[1]) / ((int)objects[2]))) * ((int)objects[0])) + ((1000 * ((int)objects[0])) - 1) >= 100;

            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string sourcePath3 = "SourceText3";
            const string binding =
                "Text ((SourceText1 - (SourceText2/SourceText3))*SourceText1) + ((1000*SourceText1) - 1) >= 100";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            var args = new object[] { 1, 1, 2 };
            expression(context, args).ShouldEqual(expressionFunc(args));

            args = new object[] { 2345, 234, 4234 };
            expression(context, args).ShouldEqual(expressionFunc(args));

            args = new object[] { 2352345, 212, 234 };
            expression(context, args).ShouldEqual(expressionFunc(args));

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
            BindingSourceShouldBeValidDataContext(targetObj, sources[2].Invoke(context), sourcePath3);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression4()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "a";
            const string sourcePath2 = "b";
            const string sourcePath3 = "c";
            const string sourcePath4 = "d";
            const string binding = "Text a < b && c < d && (a == c || b == d)";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { 1, 2, 3, 4 }).ShouldEqual(1 < 2 && 3 < 4 && (1 == 3 || 2 == 4));
            expression(context, new object[] { 1, 4, 3, 4 }).ShouldEqual(1 < 4 && 3 < 4 && (1 == 3 || 4 == 4));

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
            BindingSourceShouldBeValidDataContext(targetObj, sources[2].Invoke(context), sourcePath3);
            BindingSourceShouldBeValidDataContext(targetObj, sources[3].Invoke(context), sourcePath4);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression5()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "a";
            const string sourcePath2 = "b";
            const string sourcePath3 = "c";
            const string sourcePath4 = "d";
            const string binding = "Text a + b + c == d ? false : true || true";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { 0, 1, 0, 1 }).ShouldEqual(0 + 1 + 0 == 1 ? false : true || true);
            expression(context, new object[] { 0, 1, 1, 0 }).ShouldEqual(0 + 1 + 1 == 1 ? false : true || true);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
            BindingSourceShouldBeValidDataContext(targetObj, sources[2].Invoke(context), sourcePath3);
            BindingSourceShouldBeValidDataContext(targetObj, sources[3].Invoke(context), sourcePath4);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression6()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "x";
            const string binding = "Text x ?? 0 + 1 == 1 ? false : true || true";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            object x = null;
            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { x }).ShouldEqual(x ?? (0 + 1 == 1 ? false : true || true));
            x = new object();
            expression(context, new object[] { x }).ShouldEqual(x ?? (0 + 1 == 1 ? false : true || true));

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression7()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "x";
            const string binding = "Text x.Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            IList<string> x = new string[0];
            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { x }).ShouldEqual(x.Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true);
            x = new string[] { null };
            expression(context, new object[] { x }).ShouldEqual(x.Select(s => s == null ? 10 + 4 : 3 + 10).FirstOrDefault() == 0 ? false : true || true);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression8()
        {
            const string targetPath = "Text";
            const string binding = "Text 1 == 0 ? $param1 : $param2";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath targetBPath = context.GetData(BindingBuilderConstants.TargetPath);
            targetBPath.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { true, false }).ShouldEqual(false);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpression9()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "x";
            const string binding = "Text x/2-10";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { 20 }).ShouldEqual(0);
            expression(context, new object[] { 40 }).ShouldEqual(10);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithCustomMethod()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string methodName = "TestMethod";
            const string binding = "Text $TestMethod(SourceText1, SourceText2).IntProperty";
            var args = new object[] { "tset", 1 };
            var sourceModel = new BindingSourceModel { IntProperty = int.MaxValue };

            var provider = new BindingProvider();
            var resolver = new BindingResourceResolver();
            BindingServiceProvider.ResourceResolver = resolver;
            var method = new BindingResourceMethod((list, objects, c) =>
            {
                objects[0].ShouldEqual(args[0]);
                objects[1].ShouldEqual(args[1]);
                return sourceModel;
            }, typeof(BindingSourceModel));
            resolver.AddMethod(methodName, method, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: provider);


            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, args).ShouldEqual(sourceModel.IntProperty);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithCustomObjectValueStatic1()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string objectName = "TestObject";
            const string binding = "Text SourceText1 + SourceText2 == $$TestObject";
            var args = new object[] { 9, 1 };

            var provider = new BindingProvider();
            var resolver = new BindingResourceResolver();
            BindingServiceProvider.ResourceResolver = resolver;
            resolver.AddObject(objectName, new BindingResourceObject(10), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: provider);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, args).ShouldEqual(true);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithCustomObjectValueStatic2()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string objectName = "TestObject";
            const string binding = "Text (SourceText1 + SourceText2).GetHashCode() == $$TestObject.GetHashCode()";
            var args = new object[] { 9, 1 };

            var provider = new BindingProvider();
            var resolver = new BindingResourceResolver();
            BindingServiceProvider.ResourceResolver = resolver;
            resolver.AddObject(objectName, new BindingResourceObject(10), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: provider);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, args).ShouldEqual(true);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithCustomObjectValueDynamic1()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string sourcePath3 = "Item1";
            const string objectName = "TestObject";
            const string binding = "Text SourceText1 + SourceText2 == $TestObject.Item1";
            var tuple = new Tuple<int>(10);
            var expressionObject = new BindingResourceObject(tuple);
            var args = new object[] { 9, 1, tuple.Item1 };

            var provider = new BindingProvider();
            var resolver = new BindingResourceResolver();
            BindingServiceProvider.ResourceResolver = resolver;
            resolver.AddObject(objectName, expressionObject, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: provider);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, args).ShouldEqual(true);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
            BindingSourceShouldBeValid(sources[2].Invoke(context), sourcePath3, tuple);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithCustomObjectValueDynamic2()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string objectName = "TestObject";
            const string binding = "Text SourceText1 + SourceText2 == $TestObject.IntProperty";
            var model = new BindingSourceModel { IntProperty = 10 };
            var args = new object[] { 9, 1, model.IntProperty };

            var provider = new BindingProvider();
            var resolver = new BindingResourceResolver();
            BindingServiceProvider.ResourceResolver = resolver;
            resolver.AddObject(objectName, new BindingResourceObject(model), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: provider);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, args).ShouldEqual(true);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
            BindingSourceShouldBeValid(sources[2].Invoke(context),
                GetMemberPath(model, sourceModel => sourceModel.IntProperty), model);
        }

        [TestMethod]
        public void ParserShouldParseMultiExpressionWithConverter()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "SourceText1";
            const string sourcePath2 = "SourceText2";
            const string converterName = "TestConverter";
            const string binding = "Text $TestConverter(SourceText1, SourceText2)";
            var args = new object[] { "tset", 1 };
            var sourceModel = new BindingSourceModel { IntProperty = int.MaxValue };
            var bindingMock = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock
                    {
                        GetPathMembers = b => new BindingPathMembersMock(this, BindingPath.Create("IntProperty"), new BindingMemberInfo("IntProperty", BindingSourceModel.IntPropertyInfo, typeof(BindingSourceModel)))
                    }
                }
            };

            var converterMock = new ValueConverterCoreMock
            {
                Convert = (o, type, arg3, arg4) =>
                {
                    type.ShouldEqual(BindingSourceModel.IntPropertyInfo.PropertyType);
                    o.ShouldEqual(args[0]);
                    arg3.ShouldEqual(args[1]);
                    return sourceModel;
                }
            };
            var provider = new BindingProvider();
            var resolver = new BindingResourceResolver();
            BindingServiceProvider.ResourceResolver = resolver;
            resolver.AddConverter(converterName, converterMock, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: provider);

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.Add(BindingConstants.Binding, bindingMock);

            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, args).ShouldEqual(sourceModel);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
            BindingSourceShouldBeValidDataContext(targetObj, sources[1].Invoke(context), sourcePath2);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithLambda1()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "param";
            const string binding = "Text param.Where(x => x == 'test').FirstOrDefault()";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { new[] { "test" } }).ShouldEqual("test");
            expression(context, new object[] { new[] { string.Empty } }).ShouldBeNull();

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithLambda2()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "param";
            const string binding = "Text param.Where(x => x == 'test').Aggregate('seed', (s1, s2) => s1 + s2, s1 => s1.Length)";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { new[] { "test" } }).ShouldEqual(8);
            expression(context, new object[] { new[] { string.Empty } }).ShouldEqual(4);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithLambda3()
        {
            const string targetPath = "Text";
            const string sourcePath1 = "param";
            const string binding = "Text param.Where<string>(x => x == 'test').Aggregate<string, string, int>('seed', (s1, s2) => s1 + s2, s1 => s1.Length)";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { new[] { "test" } }).ShouldEqual(8);
            expression(context, new object[] { new[] { string.Empty } }).ShouldEqual(4);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);
            BindingSourceShouldBeValidDataContext(targetObj, sources[0].Invoke(context), sourcePath1);
        }

        [TestMethod]
        public void ParserShouldParseBindingMode()
        {
            const string binding = "Text SourceText, Mode={0}";
            var modes = new Dictionary<string, Action<IList<IBindingBehavior>, IDataContext>>
            {
                {"Default", (list, dataContext) => list.ShouldBeEmpty()},
                {"TwoWay", (list, dataContext) => list.Single().ShouldBeType<TwoWayBindingMode>()},
                {"OneWay", (list, dataContext) => list.Single().ShouldBeType<OneWayBindingMode>()},
                {"OneTime", (list, dataContext) => list.Single().ShouldBeType<OneTimeBindingMode>()},
                {"OneWayToSource", (list, dataContext) => list.Single().ShouldBeType<OneWayToSourceBindingMode>()},
                {"None", (list, dataContext) => list.Single().ShouldBeType<NoneBindingMode>()},
            };
            IBindingParser bindingParser = CreateBindingParser();
            foreach (var mode in modes)
            {
                var context = new BindingBuilder(bindingParser.Parse(string.Format(binding, mode.Key), EmptyContext).Single());
                var behaviors = context.GetData(BindingBuilderConstants.Behaviors) ?? new List<IBindingBehavior>();
                mode.Value(behaviors, context);
            }
        }

        [TestMethod]
        public void ParserShouldParseCustomBehaviorByName1()
        {
            const string behaviorName = "TestBehavior";
            const string binding = "Text SourceText, TestBehavior=true";
            var value = new TwoWayBindingMode();
            var resolver = new BindingResourceResolver();
            resolver.AddBehavior(behaviorName, (dataContext, list) =>
            {
                list.Count.ShouldEqual(1);
                list[0].ShouldEqual(true);
                return value;
            }, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Behaviors).Single().ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseCustomBehaviorByName2()
        {
            const string behaviorName = "TestBehavior";
            const string binding = "Text SourceText, TestBehavior=100";
            var value = new TwoWayBindingMode();
            var resolver = new BindingResourceResolver();
            resolver.AddBehavior(behaviorName, (dataContext, list) =>
            {
                list.Count.ShouldEqual(1);
                list[0].ShouldEqual(100);
                return value;
            }, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Behaviors).Single().ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseCustomBehaviorByName3()
        {
            const string behaviorName = "TestBehavior";
            const string binding = "Text SourceText, TestBehavior=stringvalue, Validate=true";
            var value = new TwoWayBindingMode();
            var resolver = new BindingResourceResolver();
            resolver.AddBehavior(behaviorName, (dataContext, list) =>
            {
                list.Count.ShouldEqual(1);
                list[0].ShouldEqual("stringvalue");
                return value;
            }, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Behaviors).OfType<TwoWayBindingMode>().Single().ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseCustomBehaviorMultiParams()
        {
            const string behaviorName = "TestBehavior";
            const string binding = "Text SourceText, Behavior=TestBehavior(stringvalue, 10, null, true, part1.part2), Validate=true";
            var value = new TwoWayBindingMode();
            var resolver = new BindingResourceResolver();
            resolver.AddBehavior(behaviorName, (dataContext, list) =>
            {
                list.Count.ShouldEqual(5);
                list[0].ShouldEqual("stringvalue");
                list[1].ShouldEqual(10);
                list[2].ShouldBeNull();
                list[3].ShouldEqual(true);
                list[4].ShouldEqual("part1.part2");
                return value;
            }, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Behaviors).OfType<TwoWayBindingMode>().Single().ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseValidatesOnNotifyDataErrors()
        {
            const string binding = "Text SourceText, ValidatesOnNotifyDataErrors=true";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Behaviors)
                .Single()
                .ShouldBeType<ValidatesOnNotifyDataErrorsBehavior>();
        }

        [TestMethod]
        public void ParserShouldParseValidatesOnExceptions()
        {
            const string binding = "Text SourceText, ValidatesOnExceptions=true";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Behaviors)
                .Single()
                .ShouldBeType<ValidatesOnExceptionsBehavior>();
        }

        [TestMethod]
        public void ParserShouldParseDefaultValueOnException1()
        {
            const string binding = "Text SourceText, DefaultValueOnException=true";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            var behavior = (DefaultValueOnExceptionBehavior)context
                .GetData(BindingBuilderConstants.Behaviors)
                .Single();
            behavior.Value.ShouldEqual(true);
        }

        [TestMethod]
        public void ParserShouldParseDefaultValueOnException2()
        {
            const string binding = "Text SourceText, DefaultValueOnException=10";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            var behavior = (DefaultValueOnExceptionBehavior)context
                .GetData(BindingBuilderConstants.Behaviors)
                .Single();
            behavior.Value.ShouldEqual(10);
        }

        [TestMethod]
        public void ParserShouldParseDefaultValueOnException3()
        {
            const string binding = "Text SourceText, DefaultValueOnException=stringvalue, Mode=TwoWay";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            var behavior = context
                .GetData(BindingBuilderConstants.Behaviors)
                .OfType<DefaultValueOnExceptionBehavior>()
                .Single();
            behavior.Value.ShouldEqual("stringvalue");
        }

        [TestMethod]
        public void ParserShouldParseDelaySource1()
        {
            const int delayValue = 500;
            const string binding = "Text SourceText, Delay=500";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            var bindingBehavior = (DelayBindingBehavior)context.GetData(BindingBuilderConstants.Behaviors).Single();
            bindingBehavior.Delay.ShouldEqual(delayValue);
            bindingBehavior.IsTarget.ShouldBeFalse();
        }

        [TestMethod]
        public void ParserShouldParseDelaySource2()
        {
            const int delayValue = 500;
            const string binding = "Text SourceText, SourceDelay=500";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            var bindingBehavior = (DelayBindingBehavior)context.GetData(BindingBuilderConstants.Behaviors).Single();
            bindingBehavior.Delay.ShouldEqual(delayValue);
            bindingBehavior.IsTarget.ShouldBeFalse();
        }

        [TestMethod]
        public void ParserShouldParseDelayTarget()
        {
            const int delayValue = 500;
            const string binding = "Text SourceText, TargetDelay=500";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            var bindingBehavior = (DelayBindingBehavior)context.GetData(BindingBuilderConstants.Behaviors).Single();
            bindingBehavior.Delay.ShouldEqual(delayValue);
            bindingBehavior.IsTarget.ShouldBeTrue();
        }

        [TestMethod]
        public void ParserShouldParseConverterName()
        {
            const string converterName = "test";
            const string binding = "Text SourceText, Converter=test";
            var value = new InverseBooleanValueConverter();
            var resolver = new BindingResourceResolver();
            resolver.AddConverter(converterName, value, true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Converter).Invoke(EmptyContext).ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseConverterFromDataContext()
        {
            const string binding = "Text Text, Converter={ObjectProperty}";
            var sourceObj = new BindingSourceModel { ObjectProperty = new ValueConverterCoreMock() };
            var target = new object();
            var dataBinding = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock { GetSource = b => target }
                }
            };
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context
                .GetData(BindingBuilderConstants.Behaviors)
                .ForEach(behavior => behavior.Attach(dataBinding));
            context.GetData(BindingBuilderConstants.Converter)
                .Invoke(EmptyContext)
                .ShouldBeNull();
            BindingServiceProvider.ContextManager.GetBindingContext(target).Value = sourceObj;
            context.GetData(BindingBuilderConstants.Converter)
                .Invoke(EmptyContext)
                .ShouldEqual(sourceObj.ObjectProperty);
        }

        [TestMethod]
        public void ParserShouldParseConverterFromResource()
        {
            const string binding = "Text Text, Converter=$param";
            var value = new InverseBooleanValueConverter();
            var resolver = new BindingResourceResolver();
            resolver.AddObject("param", new BindingResourceObject(value), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Converter)
                .Invoke(EmptyContext)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseConverterParameterFromDataContext1()
        {
            const string binding = "Text Text, ConverterParameter={ObjectProperty}";
            var sourceObj = new BindingSourceModel { ObjectProperty = new object() };
            var target = new object();
            var dataBinding = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock { GetSource = b => target }
                }
            };
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context
                .GetData(BindingBuilderConstants.Behaviors)
                .ForEach(behavior => behavior.Attach(dataBinding));
            context.GetData(BindingBuilderConstants.ConverterParameter)
                .Invoke(EmptyContext)
                .ShouldBeNull();
            BindingServiceProvider.ContextManager.GetBindingContext(target).Value = sourceObj;
            context.GetData(BindingBuilderConstants.ConverterParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(sourceObj.ObjectProperty);
        }

        [TestMethod]
        public void ParserShouldParseConverterParameterFromDataContext2()
        {
            const string binding = "Text Text, ConverterParameter=ObjectProperty";
            var sourceObj = new BindingSourceModel { ObjectProperty = new object() };
            var target = new object();
            var dataBinding = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock { GetSource = b => target }
                }
            };
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context
                .GetData(BindingBuilderConstants.Behaviors)
                .ForEach(behavior => behavior.Attach(dataBinding));
            context.GetData(BindingBuilderConstants.ConverterParameter)
                .Invoke(EmptyContext)
                .ShouldBeNull();
            BindingServiceProvider.ContextManager.GetBindingContext(target).Value = sourceObj;
            context.GetData(BindingBuilderConstants.ConverterParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(sourceObj.ObjectProperty);
        }

        [TestMethod]
        public void ParserShouldParseConverterParameterFromValue()
        {
            const string value = "value";
            const string binding = "Text Text, ConverterParameter='value'";
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.ConverterParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseConverterParameterFromResource()
        {
            const string value = "value";
            const string binding = "Text Text, ConverterParameter=$param";
            var resolver = new BindingResourceResolver();
            resolver.AddObject("param", new BindingResourceObject(value), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.ConverterParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseFallbackFromDataContext()
        {
            const string binding = "Text Text, Fallback={ObjectProperty}";
            var sourceObj = new BindingSourceModel { ObjectProperty = new object() };
            var target = new object();
            var dataBinding = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock { GetSource = b => target }
                }
            };
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context
                .GetData(BindingBuilderConstants.Behaviors)
                .ForEach(behavior => behavior.Attach(dataBinding));
            context.GetData(BindingBuilderConstants.Fallback)
                .Invoke(EmptyContext)
                .ShouldBeNull();
            BindingServiceProvider.ContextManager.GetBindingContext(target).Value = sourceObj;
            context.GetData(BindingBuilderConstants.Fallback)
                .Invoke(EmptyContext)
                .ShouldEqual(sourceObj.ObjectProperty);
        }

        [TestMethod]
        public void ParserShouldParseFallbackFromValue()
        {
            const string value = "value";
            const string binding = "Text Text, Fallback=value";
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Fallback)
                .Invoke(EmptyContext)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseFallbackFromResource()
        {
            const string value = "value";
            const string binding = "Text Text, Fallback=$param";
            var resolver = new BindingResourceResolver();
            resolver.AddObject("param", new BindingResourceObject(value), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.Fallback)
                .Invoke(EmptyContext)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseTargetNullValueFromValue()
        {
            const string value = "value";
            const string binding = "Text Text, TargetNullValue=value";
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.TargetNullValue)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseTargetNullValueFromResource()
        {
            const string value = "value";
            const string binding = "Text Text, TargetNullValue=$param";
            var resolver = new BindingResourceResolver();
            resolver.AddObject("param", new BindingResourceObject(value), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.TargetNullValue)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseCommandParameterFromDataContext1()
        {
            const string binding = "Text Text, CommandParameter={ObjectProperty}";
            var sourceObj = new BindingSourceModel { ObjectProperty = new object() };
            var target = new object();
            var dataBinding = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock { GetSource = b => target }
                }
            };
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context
                .GetData(BindingBuilderConstants.Behaviors)
                .ForEach(behavior => behavior.Attach(dataBinding));
            context.GetData(BindingBuilderConstants.CommandParameter)
                .Invoke(EmptyContext)
                .ShouldBeNull();
            BindingServiceProvider.ContextManager.GetBindingContext(target).Value = sourceObj;
            context.GetData(BindingBuilderConstants.CommandParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(sourceObj.ObjectProperty);
        }

        [TestMethod]
        public void ParserShouldParseCommandParameterFromDataContext2()
        {
            const string binding = "Text Text, CommandParameter=ObjectProperty";
            var sourceObj = new BindingSourceModel { ObjectProperty = new object() };
            var target = new object();
            var dataBinding = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock { GetSource = b => target }
                }
            };
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context
                .GetData(BindingBuilderConstants.Behaviors)
                .ForEach(behavior => behavior.Attach(dataBinding));
            context.GetData(BindingBuilderConstants.CommandParameter)
                .Invoke(EmptyContext)
                .ShouldBeNull();
            BindingServiceProvider.ContextManager.GetBindingContext(target).Value = sourceObj;
            context.GetData(BindingBuilderConstants.CommandParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(sourceObj.ObjectProperty);
        }

        [TestMethod]
        public void ParserShouldParseCommandParameterFromValue()
        {
            const string value = "value";
            const string binding = "Text Text, CommandParameter='value'";
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.CommandParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseCommandParameterFromResource()
        {
            const string value = "value";
            const string binding = "Text Text, CommandParameter=$param";
            var resolver = new BindingResourceResolver();
            resolver.AddObject("param", new BindingResourceObject(value), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.CommandParameter)
                .Invoke(EmptyContext)
                .ShouldEqual(value);
        }

        [TestMethod]
        public void ParserShouldParseConverterCultureFromDataContext()
        {
            const string binding = "Text Text, ConverterCulture={ObjectProperty}";
            var sourceObj = new BindingSourceModel { ObjectProperty = CultureInfo.InvariantCulture };
            var target = new object();
            var dataBinding = new DataBindingMock
            {
                TargetAccessor = new BindingSourceAccessorMock
                {
                    Source = new BindingSourceMock { GetSource = b => target }
                }
            };
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context
                .GetData(BindingBuilderConstants.Behaviors)
                .ForEach(behavior => behavior.Attach(dataBinding));
            context.GetData(BindingBuilderConstants.ConverterCulture)
                .Invoke(EmptyContext)
                .ShouldBeNull();
            BindingServiceProvider.ContextManager.GetBindingContext(target).Value = sourceObj;
            context.GetData(BindingBuilderConstants.ConverterCulture)
                .Invoke(EmptyContext)
                .ShouldEqual(sourceObj.ObjectProperty);
        }

        [TestMethod]
        public void ParserShouldParseConverterCultureFromValue()
        {
            var cultureInfo = new CultureInfo("ru-RU");
            const string binding = "Text Text, ConverterCulture='ru-RU'";
            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.ConverterCulture)
                .Invoke(EmptyContext)
                .Name
                .ShouldEqual(cultureInfo.Name);
        }

        [TestMethod]
        public void ParserShouldParseConverterCultureFromResource()
        {
            const string binding = "Text Text, ConverterCulture=$param";
            var cultureInfo = new CultureInfo("ru-RU");
            var resolver = new BindingResourceResolver();
            resolver.AddObject("param", new BindingResourceObject(cultureInfo), true);
            IBindingParser bindingParser = CreateBindingParser(bindingProvider: new BindingProvider());
            BindingServiceProvider.ResourceResolver = resolver;

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            context.GetData(BindingBuilderConstants.ConverterCulture)
                .Invoke(EmptyContext)
                .ShouldEqual(cultureInfo);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithDynamicObjectMemberAccess()
        {
            const string targetPath = "Text";
            const string sourcePath = "TestValue";
            const string binding = "Text TestValue";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            bool invokedObserve = false;
            var dynamicObject = new DynamicObjectMock
            {
                GetMember = (s, list) =>
                {
                    list.IsNullOrEmpty().ShouldBeTrue();
                    s.ShouldEqual(sourcePath);
                    return context;
                },
                TryObserve = (s, listener) =>
                {
                    s.ShouldEqual(sourcePath);
                    invokedObserve = true;
                    return null;
                }
            };
            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var source = context.GetData(BindingBuilderConstants.Sources)[0].Invoke(context);
            BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);

            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = dynamicObject;
            BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            invokedObserve.ShouldBeTrue();

            var pathMembers = source.GetPathMembers(true);
            pathMembers.AllMembersAvailable.ShouldBeTrue();
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(context);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithDynamicObjectIndexerAccessConstant()
        {
            const string targetPath = "Text";
            const string sourcePath = "[value]";
            const string binding = "Text ['value']";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            bool invokedObserve = false;
            var dynamicObject = new DynamicObjectMock
            {
                GetMember = (s, list) =>
                {
                    throw new NotSupportedException();
                },
                TryObserve = (s, listener) =>
                {
                    s.ShouldEqual(sourcePath);
                    invokedObserve = true;
                    return null;
                },
                GetIndex = (list, dataContext) =>
                {
                    list.Count.ShouldEqual(1);
                    list[0].ShouldEqual("value");
                    return context;
                }
            };
            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var source = context.GetData(BindingBuilderConstants.Sources)[0].Invoke(context);
            BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);

            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = dynamicObject;
            BindingSourceShouldBeValidDataContext(targetObj, source, sourcePath);
            invokedObserve.ShouldBeTrue();

            var pathMembers = source.GetPathMembers(true);
            pathMembers.AllMembersAvailable.ShouldBeTrue();
            pathMembers.LastMember.GetValue(pathMembers.PenultimateValue, null).ShouldEqual(context);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithDynamicObjectIndexerAccessExpression()
        {
            const string targetPath = "Text";
            const string binding = "Text $context[TestValue]";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var dynamicObject = new DynamicObjectMock
            {
                GetIndex = (list, dataContext) =>
                {
                    dataContext.ShouldEqual(context);
                    list.Count.ShouldEqual(1);
                    return list[0];
                }
            };

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { dynamicObject, expression }).ShouldEqual(expression);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithDynamicObjectMethodAccessExpression()
        {
            const string targetPath = "Text";
            const string binding = "Text $context.InvokeMethod(TestValue)";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var dynamicObject = new DynamicObjectMock
            {
                InvokeMember = (name, list, types, dataContext) =>
                {
                    types.IsNullOrEmpty().ShouldBeTrue();
                    name.ShouldEqual("InvokeMethod");
                    dataContext.ShouldEqual(context);
                    list.Count.ShouldEqual(1);
                    return list[0];
                }
            };

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { dynamicObject, expression }).ShouldEqual(expression);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithDynamicObjectMethodGenericAccessExpression()
        {
            const string targetPath = "Text";
            const string binding = "Text $context.InvokeMethod<string>(TestValue)";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var dynamicObject = new DynamicObjectMock
            {
                InvokeMember = (name, list, types, dataContext) =>
                {
                    types.Count.ShouldEqual(1);
                    types[0].ShouldEqual(typeof(string));
                    name.ShouldEqual("InvokeMethod");
                    dataContext.ShouldEqual(context);
                    list.Count.ShouldEqual(1);
                    return list[0];
                }
            };

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, new object[] { dynamicObject, expression }).ShouldEqual(expression);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithStringLiteral1()
        {
            const string targetPath = "Text";
            const string binding = "Text 'Test'";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, Empty.Array<object>()).ShouldEqual("Test");
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithStringLiteral2()
        {
            const string targetPath = "Text";
            const string binding = @"Text '\'T\''";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, Empty.Array<object>()).ShouldEqual("'T'");
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithStringLiteral3()
        {
            const string targetPath = "Text";
            const string binding = @"Text '\""T\""'";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, Empty.Array<object>()).ShouldEqual("\"T\"");
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithCharLiteral()
        {
            const string targetPath = "Text";
            const string binding = @"Text 'T'";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, Empty.Array<object>()).ShouldEqual('T');
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithEventArgs()
        {
            const string targetPath = "Text";
            const string binding = @"Text $args";
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            var target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            context.Add(BindingConstants.CurrentEventArgs, EventArgs.Empty);
            var expression = context.GetData(BindingBuilderConstants.MultiExpression);
            expression(context, Empty.Array<object>()).ShouldEqual(EventArgs.Empty);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithBindingSource1()
        {
            const string targetPath = "Text";
            string binding = "Text $" + BindingServiceProvider.ResourceResolver.BindingSourceResourceName;
            var src = new object();
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, new DataContext(BindingBuilderConstants.RawSources.ToValue(new[] { src }))).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);

            IBindingSource source = sources.Single().Invoke(context);
            var members = source.GetPathMembers(true);
            members.LastMember.GetValue(members.PenultimateValue, null).ShouldEqual(src);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithBindingSource2()
        {
            const string targetPath = "Text";
            string binding = "Text $" + BindingServiceProvider.ResourceResolver.BindingSourceResourceName;
            var src = new object();
            IBindingParser bindingParser = CreateBindingParser();

            var context = new BindingBuilder(bindingParser.Parse(binding, EmptyContext).Single());
            IBindingPath target = context.GetData(BindingBuilderConstants.TargetPath);
            target.Path.ShouldEqual(targetPath);

            var targetObj = new object();
            context.Add(BindingBuilderConstants.Target, targetObj);
            var sources = context.GetData(BindingBuilderConstants.Sources);

            IBindingSource source = sources.Single().Invoke(context);
            var members = source.GetPathMembers(true);
            members.LastMember.GetValue(members.PenultimateValue, null).ShouldBeNull();

            BindingServiceProvider.ContextManager.GetBindingContext(targetObj).Value = src;
            members.LastMember.GetValue(members.PenultimateValue, null).ShouldEqual(src);
        }

        [TestMethod]
        public void ParserShouldParseExpressionWithBindingSource3()
        {
            string binding = "Text Text, CommandParameter=$" + BindingServiceProvider.ResourceResolver.BindingSourceResourceName;
            var src = new object();

            IBindingParser bindingParser = CreateBindingParser();
            var context = new BindingBuilder(bindingParser.Parse(binding, new DataContext(BindingBuilderConstants.RawSources.ToValue(new[] { src }))).Single());
            context.GetData(BindingBuilderConstants.CommandParameter)
                .Invoke(context)
                .ShouldEqual(src);
        }

        private static void BindingSourceShouldBeValidDataContext(object target, IBindingSource bindingSource, string path)
        {
            BindingSourceShouldBeValid(bindingSource, path,
                BindingServiceProvider.ContextManager.GetBindingContext(target).Value);
        }

        private static void BindingSourceShouldBeValid(IBindingSource bindingSource, string path, object source)
        {
            var src = bindingSource.GetSource(true);
            var resourceObject = src as ISourceValue;
            if (resourceObject == null)
            {
                src.ShouldEqual(source);
                bindingSource.Path.Path.ShouldEqual(path);
            }
            else
            {
                resourceObject.Value.ShouldEqual(source);
                bindingSource.Path.Path.ShouldEqual(path);
            }
        }

        protected virtual IBindingParser CreateBindingParser(IVisualTreeManager treeManager = null, IBindingProvider bindingProvider = null, IObserverProvider observerProvider = null)
        {
            if (bindingProvider == null)
                bindingProvider = new BindingProvider();
            BindingServiceProvider.BindingProvider = bindingProvider;
            if (treeManager != null)
                BindingServiceProvider.VisualTreeManager = treeManager;
            if (observerProvider != null)
                BindingServiceProvider.ObserverProvider = observerProvider;
            return new BindingParser();
        }

        #endregion
    }
}