using System.Collections.Generic;
using System.Collections.ObjectModel;
using MugenMvvm.Extensions;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation
{
    public class ValidationResultTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void HasResultShouldBeFalseDefault()
        {
            ValidationResult result = default;
            result.HasResult.ShouldBeFalse();
        }

        [Fact]
        public void ShouldInitializeValues()
        {
            var dictionary = new Dictionary<string, object?>();
            var result = ValidationResult.Get(dictionary, DefaultMetadata);
            result.Metadata.ShouldEqual(DefaultMetadata);
            result.Errors.ShouldEqual(dictionary);
            result.SingleMemberErrors.IsEmpty.ShouldBeTrue();
            result.SingleMemberName.ShouldBeNull();
            result.HasResult.ShouldBeTrue();
        }

        [Fact]
        public void ShouldReturnSingleMemberValue1()
        {
            var memberName = "test";
            var result = new object[] {"1", "2"};
            var singleResult = ValidationResult.Get("test", result, DefaultMetadata);
            singleResult.Metadata.ShouldEqual(DefaultMetadata);
            singleResult.Errors.ShouldBeNull();
            singleResult.SingleMemberName.ShouldEqual(memberName);
            singleResult.SingleMemberErrors.AsList().ShouldEqual(result);
            singleResult.HasResult.ShouldBeTrue();
        }

        [Fact]
        public void GetErrorsShouldReturnNonReadonlyDictionary()
        {
            ValidationResult v = default;
            var errors = v.GetErrors();
            errors.Count.ShouldEqual(0);
            errors.IsReadOnly.ShouldBeFalse();

            v = ValidationResult.Get(new Dictionary<string, object?>());
            v.GetErrors().ShouldEqual((object) v.Errors!);

            var readonlyDict = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>
            {
                {"1", new[] {"1"}}
            });
            v = ValidationResult.Get(readonlyDict);
            errors = v.GetErrors();
            errors.ShouldEqual(readonlyDict);
            errors.IsReadOnly.ShouldBeFalse();
        }

        #endregion
    }
}