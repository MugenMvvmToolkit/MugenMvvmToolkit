using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Validation;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class ValidatorErrorManagerTest : UnitTestBase
    {
        private const string TwoErrorSource = "t2";
        private const string SingleErrorSource = "t1";
        private const string NoErrorSource = "t0";

        private const string Member1 = "1";
        private const string Member2 = "2";
        private readonly Dictionary<object, List<ValidationErrorInfo>> _sourceErrors;
        private readonly List<ValidationErrorInfo> _allErrors;

        private readonly ValidationErrorInfo _member1Error;
        private readonly ValidationErrorInfo _member2Error;

        public ValidatorErrorManagerTest()
        {
            Validator.AddComponent(new ValidatorErrorManager());
            _member1Error = new ValidationErrorInfo(new object(), Member1, Member1);
            _member2Error = new ValidationErrorInfo(new object(), Member2, Member2);
            _sourceErrors = new Dictionary<object, List<ValidationErrorInfo>>
            {
                [TwoErrorSource] = new() {_member1Error, _member2Error},
                [SingleErrorSource] = new() {_member1Error},
                [NoErrorSource] = new()
            };
            _allErrors = new List<ValidationErrorInfo>();
            foreach (var sourceError in _sourceErrors)
                _allErrors.AddRange(sourceError.Value);
        }

        [Fact]
        public void ClearErrorsShouldNotifyListeners()
        {
            string[] members = {Member1, Member2};
            var invokeCount = 0;
            var ignore = true;
            Validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (validator, list, m) =>
                {
                    if (ignore)
                        return;
                    ++invokeCount;
                    validator.ShouldEqual(Validator);
                    list.ShouldEqual(members);
                    m.ShouldEqual(DefaultMetadata);
                }
            });

            ignore = true;
            AddDefaultErrors();
            ignore = false;

            Validator.ClearErrors(default, null, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            ignore = true;
            AddDefaultErrors();
            invokeCount = 0;
            ignore = false;
            members = new[] {Member1};
            Validator.ClearErrors(Member1, null, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            ignore = true;
            AddDefaultErrors();
            invokeCount = 0;
            ignore = false;
            Validator.ClearErrors(Member1, NoErrorSource, DefaultMetadata);
            invokeCount.ShouldEqual(0);
        }

        [Fact]
        public void ClearShouldBeValid()
        {
            var errors = new ItemOrListEditor<ValidationErrorInfo>();

            AddDefaultErrors();
            Validator.ClearErrors();
            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(0);

            AddDefaultErrors();
            errors.Clear();
            Validator.ClearErrors(new[] {Member1, Member2});
            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(0);

            AddDefaultErrors();
            errors.Clear();
            Validator.ClearErrors(Member1);
            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error);

            AddDefaultErrors();
            errors.Clear();
            Validator.ClearErrors(Member1, SingleErrorSource);
            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            AddDefaultErrors();
            errors.Clear();
            Validator.ClearErrors(Member1, NoErrorSource);
            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors);
        }

        [Fact]
        public void GetErrorsRawShouldBeValid()
        {
            AddDefaultErrors();
            var errors = new ItemOrListEditor<object>();

            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors.Select(info => info.Error));

            errors.Clear();
            Validator.GetErrors(default, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource].Select(info => info.Error));

            errors.Clear();
            Validator.GetErrors(default, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource].Select(info => info.Error));

            errors.Clear();
            Validator.GetErrors(default, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors.Select(info => info.Error));

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource].Select(info => info.Error));

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource].Select(info => info.Error));

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error.Error);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error.Error);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(2);
            errors[0].ShouldEqual(_member1Error.Error);
            errors[1].ShouldEqual(_member1Error.Error);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error.Error);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error.Error);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void GetErrorsShouldBeValid()
        {
            AddDefaultErrors();
            var errors = new ItemOrListEditor<ValidationErrorInfo>();

            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors);

            errors.Clear();
            Validator.GetErrors(default, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            Validator.GetErrors(default, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource]);

            errors.Clear();
            Validator.GetErrors(default, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors);
            errors.Count.ShouldEqual(3);
            errors.AsList().ShouldContain(_allErrors);

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors.AsList().ShouldContain(_sourceErrors[SingleErrorSource]);

            errors.Clear();
            Validator.GetErrors(new[] {Member1, Member2}, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member2Error);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(Member2, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(2);
            errors[0].ShouldEqual(_member1Error);
            errors[1].ShouldEqual(_member1Error);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors, TwoErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors, SingleErrorSource);
            errors.Count.ShouldEqual(1);
            errors[0].ShouldEqual(_member1Error);

            errors.Clear();
            Validator.GetErrors(Member1, ref errors, NoErrorSource);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void HasErrorsShouldBeValid()
        {
            AddDefaultErrors();
            Validator.HasErrors().ShouldBeTrue();
            Validator.HasErrors(default, TwoErrorSource).ShouldBeTrue();
            Validator.HasErrors(default, SingleErrorSource).ShouldBeTrue();
            Validator.HasErrors(default, NoErrorSource).ShouldBeFalse();

            Validator.HasErrors(new[] {Member1, Member2}).ShouldBeTrue();
            Validator.HasErrors(new[] {Member1, Member2}, TwoErrorSource).ShouldBeTrue();
            Validator.HasErrors(new[] {Member1, Member2}, SingleErrorSource).ShouldBeTrue();
            Validator.HasErrors(new[] {Member1, Member2}, NoErrorSource).ShouldBeFalse();

            Validator.HasErrors(Member2).ShouldBeTrue();
            Validator.HasErrors(Member2, TwoErrorSource).ShouldBeTrue();
            Validator.HasErrors(Member2, SingleErrorSource).ShouldBeFalse();
            Validator.HasErrors(Member2, NoErrorSource).ShouldBeFalse();

            Validator.HasErrors(Member1).ShouldBeTrue();
            Validator.HasErrors(Member1, TwoErrorSource).ShouldBeTrue();
            Validator.HasErrors(Member1, SingleErrorSource).ShouldBeTrue();
            Validator.HasErrors(Member1, NoErrorSource).ShouldBeFalse();
        }

        [Fact]
        public void SetErrorsShouldBeValid()
        {
            var errors = new ItemOrListEditor<ValidationErrorInfo>();
            Validator.SetErrors(TwoErrorSource, _sourceErrors[TwoErrorSource], DefaultMetadata);

            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            Validator.SetErrors(TwoErrorSource, _member1Error, DefaultMetadata);
            Validator.GetErrors(default, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_sourceErrors[TwoErrorSource]);

            errors.Clear();
            Validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member1Error}, DefaultMetadata);
            Validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(2);
            errors.AsList().ShouldContain(_member1Error, _member1Error);

            errors.Clear();
            Validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member1Error}, DefaultMetadata);
            Validator.SetErrors(TwoErrorSource, new ValidationErrorInfo(_member1Error.Target, _member1Error.Member, null), DefaultMetadata);
            Validator.GetErrors(Member1, ref errors);
            errors.Count.ShouldEqual(0);
        }

        [Fact]
        public void SetErrorsShouldNotifyListeners()
        {
            string[] members = {Member1, Member2};
            var invokeCount = 0;
            Validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (validator, list, m) =>
                {
                    ++invokeCount;
                    validator.ShouldEqual(Validator);
                    list.ShouldEqual(members);
                    m.ShouldEqual(DefaultMetadata);
                }
            });

            invokeCount = 0;
            Validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error, _member2Error}, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            Validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error, _member2Error, new ValidationErrorInfo(_member1Error.Target, _member1Error.Member, null)},
                DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            members = new[] {Member2};
            Validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error}, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            Validator.SetErrors(TwoErrorSource, new[] {_member1Error, _member2Error}, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            Validator.SetErrors(TwoErrorSource, _member1Error, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            Validator.SetErrors(TwoErrorSource, _member2Error, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            members = new[] {Member1};
            Validator.SetErrors(SingleErrorSource, _member1Error, DefaultMetadata);
            invokeCount.ShouldEqual(1);

            invokeCount = 0;
            Validator.SetErrors(SingleErrorSource, default, DefaultMetadata);
            invokeCount.ShouldEqual(0);

            invokeCount = 0;
            members = new[] {Member1};
            Validator.SetErrors(SingleErrorSource, new ValidationErrorInfo(_member1Error.Target, _member1Error.Member, null), DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        private void AddDefaultErrors()
        {
            Validator.ClearErrors();
            foreach (var sourceError in _sourceErrors)
                Validator.SetErrors(sourceError.Key, sourceError.Value);
        }
    }
}