using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Metadata.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing
{
    public class ExpressionParserResultTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var target = MemberExpressionNode.Source;
            var source = MemberExpressionNode.EventArgs;
            var parameter = new[] {MemberExpressionNode.Self, MemberExpressionNode.Binding};
            var memberManagerRequest = new ExpressionParserResult(target, source, parameter, DefaultMetadata);
            memberManagerRequest.Target.ShouldEqual(target);
            memberManagerRequest.Source.ShouldEqual(source);
            memberManagerRequest.IsEmpty.ShouldBeFalse();
            memberManagerRequest.Parameters.ShouldEqual(parameter);
            memberManagerRequest.Metadata.ShouldEqual(DefaultMetadata);
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var target = MemberExpressionNode.Source;
            var source = MemberExpressionNode.EventArgs;
            var parameter = new[] {MemberExpressionNode.Self, MemberExpressionNode.Binding};
            var memberManagerRequest = new ExpressionParserResult(target, source, parameter,
                new TestMetadataOwner<IReadOnlyMetadataContext> {Metadata = DefaultMetadata, HasMetadata = true});
            memberManagerRequest.Target.ShouldEqual(target);
            memberManagerRequest.Source.ShouldEqual(source);
            memberManagerRequest.IsEmpty.ShouldBeFalse();
            memberManagerRequest.Parameters.ShouldEqual(parameter);
            memberManagerRequest.Metadata.ShouldEqual(DefaultMetadata);
        }

        [Fact]
        public void DefaultShouldBeEmpty() => default(ExpressionParserResult).IsEmpty.ShouldBeTrue();
    }
}