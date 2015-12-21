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
            throw new InvalidOperationException($"The expression node on type '{ownerType}' cannot be null");
        }

        internal static Exception InvalidEventSourceValue(IBindingMemberInfo member, object invalidValue)
        {
            return new InvalidOperationException(
                    $"The event binding member supports only method or command values, if it uses the SetValue method, path '{member.Path}', type '{member.Type}', member type '{member.MemberType}', underlying member '{member.Member}', invalid value '{invalidValue}'");
        }

        internal static Exception BindingMemberMustBeWriteable(IBindingMemberInfo member)
        {
            return new InvalidOperationException(
                    $"The binding member must be writeable, if it uses the SetValue method, path '{member.Path}', type '{member.Type}', member type '{member.MemberType}', underlying member '{member.Member}'");
        }

        internal static Exception BindingMemberMustBeReadable(IBindingMemberInfo member)
        {
            return new InvalidOperationException(
                    $"The binding member must be readable, if it uses the GetValue method, path '{member.Path}', type '{member.Type}', member type '{member.MemberType}', underlying member '{member.Member}'");
        }

        internal static Exception BehaviorInitialized(object behavior)
        {
            return ExceptionManager.ObjectInitialized("BindingBehavior", behavior);
        }

        internal static Exception DuplicateBindingRegistration(IDataBinding binding)
        {
            return new InvalidOperationException($"The binding '{binding}' has already been registered, dual registration is not possible.");
        }

        internal static Exception InvalidBindingTarget(string path)
        {
            return new InvalidOperationException($"The target cannot be obtained from the path '{path}'.");
        }

        internal static Exception InvalidMemberName(string source, int position)
        {
            return new ParseException($"Invalid member name while parsing '{source}' contents.", position);
        }

        internal static Exception InvalidBindingMember(Type sourceType, string path)
        {
            return new InvalidOperationException($"The binding member cannot be obtained from the path '{path}' on the '{sourceType}'.");
        }

        internal static Exception DuplicateBehavior(IBindingBehavior oldBehavior, IBindingBehavior newBehavior)
        {
            return new InvalidOperationException($"The binding behavior with id '{oldBehavior.Id}' is already in the collection, old value '{oldBehavior}', new value '{newBehavior}'");
        }

        internal static Exception BindingSourceNotFound(BindingMemberExpressionNode node)
        {
            return new InvalidOperationException($"The source cannot be found parameter name '{node.ParameterName}', binding member '{node.Path}'");
        }

        internal static Exception InvalidBindingMemberExpression()
        {
            return new ArgumentException("Member expression must be of the form 'x => x.SomeProperty.SomeOtherProperty' or 'x => x.SomeCollection[0].Property'");
        }

        internal static Exception WrapBindingException(IDataBinding binding, BindingAction action, Exception exception)
        {
            string message =
                $"A binding error has occurred, when update {(action == BindingAction.UpdateSource ? "source" : "target")}, path '{binding.TargetAccessor.Source.Path}', inner exception '{exception.Message}'";
            return new InvalidOperationException(message, exception);
        }

        internal static Exception UnexpectedExpressionNode(IExpressionNode node)
        {
            return new InvalidOperationException($"Unexpected expression node type: '{node.NodeType}', node: '{node}'");
        }

        internal static Exception UnexpectedCharacterParser(char value, ITokenizer tokenizer, string bindingExpression)
        {
            return new ParseException($"Unexpected character '{value}' while parsing '{bindingExpression}' contents.", tokenizer.Position);
        }

        internal static Exception UnknownIdentifierParser(string identifier, ITokenizer tokenizer,
            string bindingExpression, params string[] expected)
        {
            string expectedSt = null;
            if (expected != null && expected.Length != 0)
                expectedSt = " (expected " + string.Join(" or ", expected.Select(s => "'" + s + "'")) + ")";
            return new ParseException(string.Format("Unknown identifier '{0}'{2} while parsing '{1}' contents.",
                identifier, bindingExpression, expectedSt), tokenizer.Position);
        }

        internal static Exception DuplicateLambdaParameter(string parameterName)
        {
            return new InvalidOperationException($"The lambda parameter '{parameterName}' was defined more than once.");
        }

        internal static Exception InvalidExpressionParser(string expression, ITokenizer tokenizer,
            string bindingExpression)
        {
            return new ParseException($"Invalid expression '{expression}' while parsing '{bindingExpression}' contents.", tokenizer.Position);
        }

        internal static Exception UnexpectedTokenParser(TokenType current, TokenType expected, ITokenizer tokenizer,
            string bindingExpression)
        {
            return new ParseException($"Unexpected token '{current.Value}', '{expected.Value}' expected, while parsing '{bindingExpression}' contents.", tokenizer.Position);
        }

        internal static Exception InvalidIntegerLiteral(string text, ITokenizer tokenizer)
        {
            return new ParseException($"Invalid integer literal '{text}'", tokenizer.Position);
        }

        internal static Exception InvalidRealLiteral(string text, ITokenizer tokenizer)
        {
            return new ParseException($"Invalid real literal '{text}'", tokenizer.Position);
        }

        internal static Exception UnterminatedStringLiteral(string expression, ITokenizer tokenizer,
            string bindingExpression)
        {
            return new ParseException($"Unterminated string literal '{expression}' while parsing '{bindingExpression}' contents.", tokenizer.Position);
        }

        internal static Exception DigitExpected(char value, ITokenizer tokenizer, string bindingExpression)
        {
            return new ParseException($"Digit expected, current character '{value}' while parsing '{bindingExpression}' contents.", tokenizer.Position);
        }

        internal static Exception DuplicateDataConstant(DataConstant constant)
        {
            return new InvalidOperationException($"The data constant '{constant.Id}' was defined more than once.");
        }

        internal static Exception DuplicateBindingMember(Type type, string path)
        {
            return new InvalidOperationException($"The member '{type}' on type '{path}' is already registered.");
        }

        internal static Exception MissingEvent(object source, string eventName)
        {
            return new InvalidOperationException($"The source '{source}' does not contain an event with name '{eventName}'");
        }

        #endregion
    }
}
