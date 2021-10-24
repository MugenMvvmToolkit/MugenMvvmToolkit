using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.Validation;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class MappingValidatorDecoratorTest : UnitTestBase
    {
        private const string FromName = "From";
        private const string ToName = "To";
        private const string Error = "Error";

        public MappingValidatorDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Validator.AddComponent(new ValidatorErrorManager());
        }

        [Fact]
        public void GetErrorsShouldReturnMappingErrors()
        {
            var errors = new ItemOrListEditor<ValidationErrorInfo>();
            var errorsObj = new ItemOrListEditor<object>();
            var error = new ValidationErrorInfo(this, FromName, Error);
            Validator.SetErrors(this, error);

            Validator.GetErrors(ToName, ref errors);
            Validator.GetErrors(ToName, ref errorsObj);
            Validator.HasErrors(ToName).ShouldBeFalse();
            errors.Count.ShouldEqual(0);
            errorsObj.Count.ShouldEqual(0);

            Validator.AddErrorMapping(FromName, ToName);
            Validator.GetErrors(ToName, ref errors);
            Validator.GetErrors(ToName, ref errorsObj);
            Validator.HasErrors(ToName).ShouldBeTrue();
            errors.ToItemOrList().ShouldEqual(new[] {error, new ValidationErrorInfo(this, ToName, Error)});
            errorsObj.ToItemOrList().ShouldEqual(Error);

            Validator.RemoveErrorMapping(FromName);
            errors.Clear();
            errorsObj.Clear();

            Validator.GetErrors(ToName, ref errors);
            Validator.GetErrors(ToName, ref errorsObj);
            errors.Count.ShouldEqual(0);
            errorsObj.Count.ShouldEqual(0);
        }

        [Fact]
        public void OnAsyncValidationShouldUseMapping()
        {
            int count = 0;
            int countFrom = 0;
            var task = Task.CompletedTask;
            Validator.AddComponent(new TestValidatorAsyncValidationListener
            {
                OnAsyncValidation = (v, e, t, m) =>
                {
                    if (e != ToName)
                        return;
                    v.ShouldEqual(Validator);
                    t.ShouldEqual(task);
                    m.ShouldEqual(Metadata);
                    ++count;
                }
            });
            Validator.AddComponent(new TestValidatorAsyncValidationListener
            {
                OnAsyncValidation = (v, e, t, m) =>
                {
                    if (e != FromName)
                        return;
                    v.ShouldEqual(Validator);
                    t.ShouldEqual(task);
                    m.ShouldEqual(Metadata);
                    ++countFrom;
                }
            });

            Validator.GetComponents<IValidatorAsyncValidationListener>().OnAsyncValidation(Validator, FromName, task, Metadata);
            countFrom.ShouldEqual(1);
            count.ShouldEqual(0);

            Validator.AddErrorMapping(FromName, ToName);
            Validator.GetComponents<IValidatorAsyncValidationListener>().OnAsyncValidation(Validator, FromName, task, Metadata);
            countFrom.ShouldEqual(2);
            count.ShouldEqual(1);

            Validator.RemoveErrorMapping(FromName);
            Validator.GetComponents<IValidatorAsyncValidationListener>().OnAsyncValidation(Validator, FromName, task, Metadata);
            countFrom.ShouldEqual(3);
            count.ShouldEqual(1);
        }

        [Fact]
        public void OnErrorsChangedShouldUseMapping()
        {
            int count = 0;
            int countFrom = 0;
            Validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (v, e, m) =>
                {
                    if (e.Item != ToName)
                        return;
                    v.ShouldEqual(Validator);
                    m.ShouldEqual(Metadata);
                    ++count;
                }
            });
            Validator.AddComponent(new TestValidatorErrorsChangedListener
            {
                OnErrorsChanged = (v, e, m) =>
                {
                    if (e.Item != FromName)
                        return;
                    v.ShouldEqual(Validator);
                    m.ShouldEqual(Metadata);
                    ++countFrom;
                }
            });

            Validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(Validator, FromName, Metadata);
            countFrom.ShouldEqual(1);
            count.ShouldEqual(0);

            Validator.AddErrorMapping(FromName, ToName);
            Validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(Validator, FromName, Metadata);
            countFrom.ShouldEqual(2);
            count.ShouldEqual(1);

            Validator.RemoveErrorMapping(FromName);
            Validator.GetComponents<IValidatorErrorsChangedListener>().OnErrorsChanged(Validator, FromName, Metadata);
            countFrom.ShouldEqual(3);
            count.ShouldEqual(1);
        }
    }
}