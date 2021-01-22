using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Validation.Internal;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Validation.Components
{
    public class RuleValidationManagerTest : UnitTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldAddResolveRulesBasedOnCondition(bool useCache)
        {
            var target1 = this;
            var target2 = "1";
            var validator = new Validator();
            var rule1 = new TestValidationRule();
            var rule2 = new TestValidationRule();
            var rule3 = new TestValidationRule();

            var validationManager = new ValidationManager();
            var component = new RuleValidationManager(useCache);
            validationManager.AddComponent(component);
            validationManager.AddComponent(new TestValidatorProviderComponent
            {
                TryGetValidator = (o, context) => validator
            });

            component.AddRule(rule1, (v, o, arg3) => v == validator && o == target1);
            component.AddRule(rule2, (v, o, arg3) => v == validator && o == target1);
            component.AddRule(rule3, (v, o, arg3) => v == validator && ReferenceEquals(o, target2));


            validationManager.TryGetValidator(target1).ShouldEqual(validator);
            var ruleValidatorComponent = validator.GetComponents<RuleValidationHandler>().Single();
            ruleValidatorComponent.UseCache.ShouldEqual(useCache);
            var rules = ruleValidatorComponent.Rules.AsList().ToList();
            rules.Count.ShouldEqual(2);
            rules.Remove(rule1).ShouldBeTrue();
            rules.Remove(rule2).ShouldBeTrue();

            validator.ClearComponents();
            validationManager.TryGetValidator(target2).ShouldEqual(validator);
            ruleValidatorComponent = validator.GetComponents<RuleValidationHandler>().Single();
            ruleValidatorComponent.UseCache.ShouldEqual(useCache);
            rules = ruleValidatorComponent.Rules.AsList().ToList();
            rules.Count.ShouldEqual(1);
            rules.Remove(rule3).ShouldBeTrue();

            validator.ClearComponents();
            validationManager.TryGetValidator(validationManager).ShouldEqual(validator);
            validator.GetComponents<RuleValidationHandler>().Count.ShouldEqual(0);


            validationManager.TryGetValidator(new object[] {target1, target2}).ShouldEqual(validator);
            var components = validator.GetComponents<RuleValidationHandler>();
            components.Count.ShouldEqual(2);

            ruleValidatorComponent = components.AsList().Single(validatorComponent => validatorComponent.Target == target1);
            ruleValidatorComponent.UseCache.ShouldEqual(useCache);
            rules = ruleValidatorComponent.Rules.AsList().ToList();
            rules.Count.ShouldEqual(2);
            rules.Remove(rule1).ShouldBeTrue();
            rules.Remove(rule2).ShouldBeTrue();

            ruleValidatorComponent = components.AsList().Single(validatorComponent => ReferenceEquals(validatorComponent.Target, target2));
            ruleValidatorComponent.UseCache.ShouldEqual(useCache);
            rules = ruleValidatorComponent.Rules.AsList().ToList();
            rules.Count.ShouldEqual(1);
            rules.Remove(rule3).ShouldBeTrue();
        }
    }
}