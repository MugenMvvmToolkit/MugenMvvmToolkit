#region Copyright

// ****************************************************************************
// <copyright file="BindingExceptionManager.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Linq;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.Exceptions;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding
{
    internal static class BindingExceptionManager
    {
        #region Fields

        internal const string ElementSourceNotFoundFormat2 =
            "The ElementSource cannot be found, the provider returns the null result for the source type '{0}' target element name '{1}'";

        internal const string RelativeSourceNotFoundFormat3 =
            "The RelativeSource cannot be found, the provider returns the null result for the source type '{0}' target type name '{1}' level '{2}'";

        internal const string CannotResolveInstanceFormat2 = "The {0} with name '{1}' is not registered in the '{2}'";

        #endregion

        #region Methods

        internal static Exception CannotResolveInstanceByName(object sender, string instanceName, string name)
        {
            return
                new InvalidOperationException(string.Format(CannotResolveInstanceFormat2, instanceName, name, sender));
        }

        internal static Exception ExpressionNodeCannotBeNull(Type ownerType)
        {
            throw new InvalidOperationException(string.Format("The expression node on type '{0}' cannot be null",
                ownerType));
        }

        internal static Exception InvalidEventSourceValue(IBindingMemberInfo member, object invalidValue)
        {
            return
                new InvalidOperationException(
                    string.Format("The event binding member supports only method or command values, if it uses the SetValue method, path '{0}', type '{1}', member type '{2}', underlying member '{3}', invalid value '{4}'",
                        member.Path, member.Type, member.MemberType, member.Member, invalidValue));
        }

        internal static Exception BindingMemberMustBeWriteable(IBindingMemberInfo member)
        {
            return
                new InvalidOperationException(
                    string.Format("The binding member must be writeable, if it uses the SetValue method, path '{0}', type '{1}', member type '{2}', underlying member '{3}'",
                        member.Path, member.Type, member.MemberType, member.Member));
        }

        internal static Exception BindingMemberMustBeReadable(IBindingMemberInfo member)
        {
            return
                new InvalidOperationException(
                    string.Format(
                        "The binding member must be readable, if it uses the GetValue method, path '{0}', type '{1}', member type '{2}', underlying member '{3}'",
                        member.Path, member.Type, member.MemberType, member.Member));
        }

        internal static Exception BehaviorInitialized(object behavior)
        {
            return ExceptionManager.ObjectInitialized("BindingBehavior", behavior);
        }

        internal static Exception DuplicateBindingRegistration(IDataBinding binding)
        {
            return
                new InvalidOperationException(
                    string.Format("The binding '{0}' has already been registered, dual registration is not possible.",
                        binding));
        }

        internal static Exception InvalidBindingTarget(string path)
        {
            return
                new InvalidOperationException(string.Format("The target cannot be obtained from the path '{0}'.", path));
        }

        internal static Exception InvalidMemberName(string source, int position)
        {
            return new ParseException(string.Format("Invalid member name while parsing '{0}' contents.", source),
                position);
        }

        internal static Exception InvalidBindingMember(Type sourceType, string path)
        {
            return new InvalidOperationException(
                string.Format("The binding member cannot be obtained from the path '{0}' on the '{1}'.", path,
                    sourceType));
        }

        internal static Exception DuplicateBehavior(IBindingBehavior oldBehavior, IBindingBehavior newBehavior)
        {
            return
                new InvalidOperationException(
                    string.Format(
                        "The binding behavior with id '{0}' is already in the collection, old value '{1}', new value '{2}'",
                        oldBehavior.Id.ToString(), oldBehavior, newBehavior));
        }

        internal static Exception BindingSourceNotFound(BindingMemberExpressionNode node)
        {
            return
                new InvalidOperationException(
                    string.Format("The source cannot be found parameter name '{0}', binding member '{1}'",
                        node.ParameterName, node.Path));
        }

        internal static Exception InvalidBindingMemberExpression()
        {
            return
                new ArgumentException(
                    "Member expression must be of the form 'x => x.SomeProperty.SomeOtherProperty' or 'x => x.SomeCollection[0].Property'");
        }

        internal static Exception WrapBindingException(IDataBinding binding, BindingAction action, Exception exception)
        {
            string message =
                string.Format("A binding error has occurred, when update {0}, path '{1}', inner exception '{2}'",
                    action == BindingAction.UpdateSource ? "source" : "target",
                    binding.TargetAccessor.Source.Path, exception.Message);
            return new InvalidOperationException(message, exception);
        }

        internal static Exception UnexpectedExpressionNode(IExpressionNode node)
        {
            return
                new InvalidOperationException(string.Format("Unexpected expression node type: '{0}', node: '{1}'",
                    node.NodeType, node));
        }

        internal static Exception UnexpectedCharacterParser(char value, ITokenizer tokenizer, string bindingExpression)
        {
            return
                new ParseException(string.Format("Unexpected character '{0}' while parsing '{1}' contents.",
                    value.ToString(), bindingExpression), tokenizer.Position);
        }

        internal static Exception UnknownIdentifierParser(string identifier, ITokenizer tokenizer,
            string bindingExpression, params string[] expected)
        {
            string expectedSt = null;
            if (expected != null && expected.Length != 0)
                expectedSt = " (expected " + string.Join(" or ", expected.Select(s => string.Format("'{0}'", s))) + ")";
            return new ParseException(string.Format("Unknown identifier '{0}'{2} while parsing '{1}' contents.",
                identifier, bindingExpression, expectedSt), tokenizer.Position);
        }

        internal static Exception DuplicateLambdaParameter(string parameterName)
        {
            return
                new InvalidOperationException(string.Format("The lambda parameter '{0}' was defined more than once.",
                    parameterName));
        }

        internal static Exception InvalidExpressionParser(string expression, ITokenizer tokenizer,
            string bindingExpression)
        {
            return
                new ParseException(
                    string.Format("Invalid expression '{0}' while parsing '{1}' contents.",
                        expression, bindingExpression), tokenizer.Position);
        }

        internal static Exception UnexpectedTokenParser(TokenType current, TokenType expected, ITokenizer tokenizer,
            string bindingExpression)
        {
            return
                new ParseException(
                    string.Format("Unexpected token '{0}', '{1}' expected, while parsing '{2}' contents.", current.Value,
                        expected.Value, bindingExpression), tokenizer.Position);
        }

        internal static Exception InvalidIntegerLiteral(string text, ITokenizer tokenizer)
        {
            return new ParseException(string.Format("Invalid integer literal '{0}'", text), tokenizer.Position);
        }

        internal static Exception InvalidRealLiteral(string text, ITokenizer tokenizer)
        {
            return new ParseException(string.Format("Invalid real literal '{0}'", text), tokenizer.Position);
        }

        internal static Exception UnterminatedStringLiteral(string expression, ITokenizer tokenizer,
            string bindingExpression)
        {
            return
                new ParseException(
                    string.Format("Unterminated string literal '{0}' while parsing '{1}' contents.",
                        expression, bindingExpression), tokenizer.Position);
        }

        internal static Exception DigitExpected(char value, ITokenizer tokenizer, string bindingExpression)
        {
            return
                new ParseException(
                    string.Format("Digit expected, current character '{0}' while parsing '{1}' contents.",
                        value.ToString(), bindingExpression), tokenizer.Position);
        }

        internal static Exception DuplicateDataConstant(DataConstant constant)
        {
            return new InvalidOperationException(string.Format("The data constant '{0}' was defined more than once.",
                    constant.Id));
        }

        internal static Exception DuplicateBindingMember(Type type, string path)
        {
            return
                new InvalidOperationException(string.Format("The member '{0}' on type '{1}' is already registered.",
                    type, path));
        }

        internal static Exception MissingEvent(object source, string eventName)
        {
            return
                new InvalidOperationException(string.Format(
                    "The source '{0}' does not contain an event with name '{1}'", source, eventName));
        }

        #endregion
    }
}
