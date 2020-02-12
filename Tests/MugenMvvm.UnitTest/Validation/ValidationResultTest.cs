using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MugenMvvm.Validation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Validation
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
        public void ConstructorShouldInitializeValues()
        {
            var dictionary = new Dictionary<string, IReadOnlyList<object>?>();
            var result = new ValidationResult(dictionary, DefaultMetadata);
            result.Metadata.ShouldEqual(DefaultMetadata);
            result.ErrorsRaw.ShouldEqual(dictionary);
            result.HasResult.ShouldBeTrue();
        }

        [Fact]
        public void SingleResultShouldReturnSingleMemberValue()
        {
            var memberName = "test";
            var result = new object[] { "1", "2" };
            var singleResult = ValidationResult.SingleResult("test", DefaultMetadata, result);
            singleResult.Metadata.ShouldEqual(DefaultMetadata);
            singleResult.ErrorsRaw!.Count.ShouldEqual(1);
            singleResult.ErrorsRaw![memberName].SequenceEqual(result).ShouldBeTrue();
        }

        [Fact]
        public void GetErrorsNonReadOnlyShouldReturnNonReadonlyDictionary()
        {
            ValidationResult v = default;
            var errors = v.GetErrorsNonReadOnly();
            errors.Count.ShouldEqual(0);
            errors.IsReadOnly.ShouldBeFalse();

            v = new ValidationResult(new Dictionary<string, IReadOnlyList<object>?>());
            v.GetErrorsNonReadOnly().ShouldEqual((object)v.ErrorsRaw!);


            var readonlyDict = new ReadOnlyDictionary<string, IReadOnlyList<object>?>(new Dictionary<string, IReadOnlyList<object>?>
            {
                {"1", new[] {"1"}}
            });
            v = new ValidationResult(readonlyDict);
            errors = v.GetErrorsNonReadOnly();
            errors.SequenceEqual(readonlyDict).ShouldBeTrue();
            errors.IsReadOnly.ShouldBeFalse();
        }

        #endregion
    }
}