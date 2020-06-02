using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
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
        public void FromErrorsShouldInitializeValues()
        {
            var dictionary = new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>();
            var result = ValidationResult.FromErrors(dictionary, DefaultMetadata);
            result.Metadata.ShouldEqual(DefaultMetadata);
            result.Errors.ShouldEqual(dictionary);
            result.SingleMemberErrors.IsNullOrEmpty().ShouldBeTrue();
            result.SingleMemberName.ShouldBeNull();
            result.HasResult.ShouldBeTrue();
        }

        [Fact]
        public void FromMemberErrorsShouldReturnSingleMemberValue1()
        {
            var memberName = "test";
            var result = new object[] {"1", "2"};
            var singleResult = ValidationResult.FromMemberErrors("test", result, DefaultMetadata);
            singleResult.Metadata.ShouldEqual(DefaultMetadata);
            singleResult.Errors.ShouldBeNull();
            singleResult.SingleMemberName.ShouldEqual(memberName);
            singleResult.SingleMemberErrors.AsList().SequenceEqual(result).ShouldBeTrue();
            singleResult.HasResult.ShouldBeTrue();
        }

        [Fact]
        public void GetErrorsNonReadOnlyShouldReturnNonReadonlyDictionary()
        {
            ValidationResult v = default;
            var errors = v.GetErrorsNonReadOnly();
            errors.Count.ShouldEqual(0);
            errors.IsReadOnly.ShouldBeFalse();

            v = ValidationResult.FromErrors(new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>());
            v.GetErrorsNonReadOnly().ShouldEqual((object) v.Errors!);

            var readonlyDict = new ReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>(new Dictionary<string, ItemOrList<object, IReadOnlyList<object>>>
            {
                {"1", new[] {"1"}}
            });
            v = ValidationResult.FromErrors(readonlyDict);
            errors = v.GetErrorsNonReadOnly();
            errors.SequenceEqual(readonlyDict).ShouldBeTrue();
            errors.IsReadOnly.ShouldBeFalse();
        }

        #endregion
    }
}