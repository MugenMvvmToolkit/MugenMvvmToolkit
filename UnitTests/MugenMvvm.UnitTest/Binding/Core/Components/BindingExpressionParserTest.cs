using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using MugenMvvm.UnitTest.Components.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingExpressionParserTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryParseBindingExpressionShouldUseExpressionParser()
        {
            var count = 0;
            var st = "";
            var parserComponent = new TestExpressionParserComponent
            {
                TryParse = (o, type, arg3) =>
                {
                    ++count;
                    o.ShouldEqual(st);
                    type.ShouldEqual(st.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                    return default;
                }
            };
            var parser = new ExpressionParser();
            parser.AddComponent(parserComponent);
            var builder = new BindingExpressionParser(parser);
            builder.TryParseBindingExpression(null!, st, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            count.ShouldEqual(1);
        }

        [Fact]
        public void TryParseBindingExpressionShouldThrowUnsupportedExpression()
        {
            var parserComponent = new TestExpressionParserComponent
            {
                TryParse = (o, type, arg3) => new ExpressionParserResult(ConstantExpressionNode.Get(0), ConstantExpressionNode.Get(0), default)
            };
            var parser = new ExpressionParser();
            parser.AddComponent(parserComponent);
            var builder = new BindingExpressionParser(parser);
            var expression = builder.TryParseBindingExpression(null!, "", DefaultMetadata).Item!;
            expression.ShouldNotBeNull();
            ShouldThrow<InvalidOperationException>(() => expression.Build(this, this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1, 1, true, true)]
        [InlineData(1, 1, true, false)]
        [InlineData(1, 1, false, true)]
        [InlineData(1, 1, false, false)]

        [InlineData(10, 1, true, true)]
        [InlineData(10, 1, true, false)]
        [InlineData(10, 1, false, true)]
        [InlineData(10, 1, false, false)]

        [InlineData(1, 10, true, true)]
        [InlineData(1, 10, true, false)]
        [InlineData(1, 10, false, true)]
        [InlineData(1, 10, false, false)]

        [InlineData(10, 10, true, true)]
        [InlineData(10, 10, true, false)]
        [InlineData(10, 10, false, true)]
        [InlineData(10, 10, false, false)]
        public void TryParseBindingExpressionShouldBuildBinding(int expressionCount, int count, bool includeNullComponent, bool includeFactoryComponent)
        {
            var target = new object();
            var source = new object();
            var targetObserver = new TestMemberPathObserver();
            var sourceObserver = new TestMemberPathObserver();
            var results = new ExpressionParserResult[expressionCount];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new ExpressionParserResult(new TestBindingMemberExpressionNode("0"), new TestBindingMemberExpressionNode("0_"),
                    ItemOrList.FromRawValue<IExpressionNode, IReadOnlyList<IExpressionNode>>(ConstantExpressionNode.Get(0)));
            }
            var parser = new ExpressionParser();
            parser.AddComponent(new TestExpressionParserComponent
            {
                TryParse = (o, type, arg3) => results
            });
            var bindingManager = new BindingManager();
            var builder = new BindingExpressionParser(parser);
            bindingManager.AddComponent(builder);

            var components = new List<object>();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var rawComponent = new TestComponent<IBinding>();
                var factoryComponent = new TestComponent<IBinding>();
                components.Add(rawComponent);
                if (includeFactoryComponent)
                    components.Add(factoryComponent);
                var index = i;
                bindingManager.AddComponent(new TestBindingExpressionInitializerComponent
                {
                    Priority = -i,
                    Initialize = (_, context) =>
                    {
                        ++invokeCount;
                        context.Target.ShouldEqual(target);
                        context.Source.ShouldEqual(source);

                        context.TargetExpression.ShouldEqual(new TestBindingMemberExpressionNode($"{index}"));
                        context.SourceExpression.ShouldEqual(new TestBindingMemberExpressionNode(index + "_"));
                        context.Parameters.AsList().SequenceEqual(Enumerable.Range(0, index + 1).Select(ConstantExpressionNode.Get)).ShouldBeTrue();

                        context.TargetExpression = new TestBindingMemberExpressionNode($"{index + 1}")
                        {
                            GetBindingSource = (t, s, arg3) =>
                            {
                                t.ShouldEqual(target);
                                s.ShouldEqual(source);
                                arg3.ShouldEqual(DefaultMetadata);
                                return targetObserver;
                            }
                        };
                        context.SourceExpression = new TestBindingMemberExpressionNode($"{index + 1}_")
                        {
                            GetBindingSource = (t, s, arg3) =>
                            {
                                t.ShouldEqual(target);
                                s.ShouldEqual(source);
                                arg3.ShouldEqual(DefaultMetadata);
                                return sourceObserver;
                            }
                        };
                        var itemOrList = context.Parameters.Editor();
                        itemOrList.Add(ConstantExpressionNode.Get(index + 1));
                        context.Parameters = itemOrList.ToItemOrList();

                        if (includeNullComponent)
                            context.BindingComponents[$"{index}null"] = null;
                        context.BindingComponents[$"{index}_1"] = rawComponent;
                        if (includeFactoryComponent)
                        {
                            context.BindingComponents[$"{index}_2"] =
                                new DelegateBindingComponentProvider<object?>((o, b, arg3, arg4, arg5) =>
                                {
                                    b.ShouldNotBeNull();
                                    arg3.ShouldEqual(target);
                                    arg4.ShouldEqual(source);
                                    arg5.ShouldEqual(DefaultMetadata);
                                    return factoryComponent;
                                }, null);
                        }
                    }
                });
            }

            var expressions = builder.TryParseBindingExpression(null!, "", DefaultMetadata).AsList();
            expressions.Count.ShouldEqual(expressionCount);
            for (int i = 0; i < expressions.Count; i++)
            {
                invokeCount = 0;
                var result = results[i];
                var expression = (IHasTargetExpressionBindingBuilder)expressions[i];
                expression.ShouldNotBeNull();
                expression.TargetExpression.ShouldEqual(result.Target);
                invokeCount.ShouldEqual(0);

                var binding = expression.Build(target, source, DefaultMetadata);
                expression.TargetExpression.ShouldEqual(new TestBindingMemberExpressionNode($"{count}"));
                invokeCount.ShouldEqual(count);
                binding.Target.ShouldEqual(targetObserver);
                binding.Source.ShouldEqual(sourceObserver);
                binding.State.ShouldEqual(BindingState.Valid);
                binding.GetComponents().AsList().ShouldContain(components);

                binding = expression.Build(target, source, DefaultMetadata);
                invokeCount.ShouldEqual(count);
                binding.Target.ShouldEqual(targetObserver);
                binding.Source.ShouldEqual(sourceObserver);
                binding.State.ShouldEqual(BindingState.Valid);
                binding.GetComponents().AsList().ShouldContain(components);
            }
        }

        [Theory]
        [InlineData(1, 1, true, true)]
        [InlineData(1, 1, true, false)]
        [InlineData(1, 1, false, true)]
        [InlineData(1, 1, false, false)]

        [InlineData(10, 1, true, true)]
        [InlineData(10, 1, true, false)]
        [InlineData(10, 1, false, true)]
        [InlineData(10, 1, false, false)]

        [InlineData(1, 10, true, true)]
        [InlineData(1, 10, true, false)]
        [InlineData(1, 10, false, true)]
        [InlineData(1, 10, false, false)]

        [InlineData(10, 10, true, true)]
        [InlineData(10, 10, true, false)]
        [InlineData(10, 10, false, true)]
        [InlineData(10, 10, false, false)]
        public void TryParseBindingExpressionShouldBuildMultiBinding(int expressionCount, int count, bool includeNullComponent, bool includeFactoryComponent)
        {
            var target = new object();
            var source = new object();
            var exp = new TestCompiledExpression();
            var targetObserver = new TestMemberPathObserver();
            var sourceObserver1 = new TestMemberPathObserver();
            var sourceObserver2 = new TestMemberPathObserver();
            var results = new ExpressionParserResult[expressionCount];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = new ExpressionParserResult(new TestBindingMemberExpressionNode("0"),
                    GetBindingSourceExpression(0, out _, out _), ItemOrList.FromRawValue<IExpressionNode, IReadOnlyList<IExpressionNode>>(ConstantExpressionNode.Get(0)));
            }

            var compiler = new ExpressionCompiler();
            compiler.AddComponent(new TestExpressionCompilerComponent
            {
                TryCompile = (_, node, context) =>
                {
                    var expected = GetBindingSourceExpression(count, out var n1, out var n2);
                    n1.Index = 0;
                    n2.Index = 1;
                    node.ShouldEqual(expected);
                    context.ShouldEqual(DefaultMetadata);
                    return exp;
                }
            });
            var parser = new ExpressionParser();
            parser.AddComponent(new TestExpressionParserComponent
            {
                TryParse = (o, type, arg3) => results
            });
            var bindingManager = new BindingManager();
            var builder = new BindingExpressionParser(parser, compiler);
            bindingManager.AddComponent(builder);

            var components = new List<object>();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var rawComponent = new TestComponent<IBinding>();
                var factoryComponent = new TestComponent<IBinding>();
                components.Add(rawComponent);
                if (includeFactoryComponent)
                    components.Add(factoryComponent);
                var index = i;
                bindingManager.AddComponent(new TestBindingExpressionInitializerComponent
                {
                    Priority = -i,
                    Initialize = (m, context) =>
                    {
                        ++invokeCount;
                        context.Target.ShouldEqual(target);
                        context.Source.ShouldEqual(source);

                        context.TargetExpression.ShouldEqual(new TestBindingMemberExpressionNode($"{index}"));
                        context.SourceExpression.ShouldEqual(GetBindingSourceExpression(index, out _, out _));
                        context.Parameters.AsList().SequenceEqual(Enumerable.Range(0, index + 1).Select(ConstantExpressionNode.Get)).ShouldBeTrue();

                        context.TargetExpression = new TestBindingMemberExpressionNode($"{index + 1}")
                        {
                            GetBindingSource = (t, s, arg3) =>
                            {
                                t.ShouldEqual(target);
                                s.ShouldEqual(source);
                                arg3.ShouldEqual(DefaultMetadata);
                                return targetObserver;
                            }
                        };
                        context.SourceExpression = GetBindingSourceExpression(index + 1, out var n1, out var n2);
                        n1.GetBindingSource = (t, s, arg3) =>
                        {
                            t.ShouldEqual(target);
                            s.ShouldEqual(source);
                            arg3.ShouldEqual(DefaultMetadata);
                            return sourceObserver1;
                        };
                        n2.GetBindingSource = (t, s, arg3) =>
                        {
                            t.ShouldEqual(target);
                            s.ShouldEqual(source);
                            arg3.ShouldEqual(DefaultMetadata);
                            return sourceObserver2;
                        };

                        var itemOrList = context.Parameters.Editor();
                        itemOrList.Add(ConstantExpressionNode.Get(index + 1));
                        context.Parameters = itemOrList.ToItemOrList();

                        if (includeNullComponent)
                            context.BindingComponents[$"{index}null"] = null;
                        context.BindingComponents[$"{index}_1"] = rawComponent;
                        if (includeFactoryComponent)
                        {
                            context.BindingComponents[$"{index}_2"] =
                                new DelegateBindingComponentProvider<object?>((o, b, arg3, arg4, arg5) =>
                                {
                                    b.ShouldNotBeNull();
                                    arg3.ShouldEqual(target);
                                    arg4.ShouldEqual(source);
                                    arg5.ShouldEqual(DefaultMetadata);
                                    return factoryComponent;
                                }, null);
                        }
                    }
                });
            }

            var expressions = builder.TryParseBindingExpression(null!, "", DefaultMetadata).AsList();
            expressions.Count.ShouldEqual(expressionCount);
            for (int i = 0; i < expressions.Count; i++)
            {
                invokeCount = 0;
                var result = results[i];
                var expression = (IHasTargetExpressionBindingBuilder)expressions[i];
                expression.ShouldNotBeNull();
                expression.TargetExpression.ShouldEqual(result.Target);
                invokeCount.ShouldEqual(0);

                var binding = (MultiBinding)expression.Build(target, source, DefaultMetadata);
                binding.Expression.ShouldEqual(exp);
                expression.TargetExpression.ShouldEqual(new TestBindingMemberExpressionNode($"{count}"));
                invokeCount.ShouldEqual(count);
                binding.Target.ShouldEqual(targetObserver);
                binding.Source.AsList().SequenceEqual(new[] { sourceObserver1, sourceObserver2 }).ShouldBeTrue();
                binding.State.ShouldEqual(BindingState.Valid);
                binding.GetComponents().AsList().ShouldContain(components);

                binding = (MultiBinding)expression.Build(target, source, DefaultMetadata);
                binding.Expression.ShouldEqual(exp);
                invokeCount.ShouldEqual(count);
                binding.Target.ShouldEqual(targetObserver);
                binding.Source.AsList().SequenceEqual(new[] { sourceObserver1, sourceObserver2 }).ShouldBeTrue();
                binding.State.ShouldEqual(BindingState.Valid);
                binding.GetComponents().AsList().ShouldContain(components);
            }
        }

        private static IExpressionNode GetBindingSourceExpression(int index, out TestBindingMemberExpressionNode node1, out TestBindingMemberExpressionNode node2)
        {
            node1 = new TestBindingMemberExpressionNode($"{index}_");
            node2 = new TestBindingMemberExpressionNode($"{index}__");
            return new BinaryExpressionNode(BinaryTokenType.Addition, node1, node2);
        }

        #endregion
    }
}