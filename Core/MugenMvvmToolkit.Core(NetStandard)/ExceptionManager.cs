#region Copyright

// ****************************************************************************
// <copyright file="ExceptionManager.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Linq.Expressions;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    internal static class ExceptionManager
    {
        #region Fields

        internal const string CommandCannotBeExecutedString = "The method Execute in RelayCommand cannot be executed because the CanExecute method returns a false value.";
        internal const string WindowClosedString = "The dialog is closed. Before close the dialog you should show it.";

        #endregion

        #region Methods

        internal static Exception ObjectNotInitialized(string objectName, object obj, string hint = null)
        {
            var type = obj as Type;
            string typeName;
            if (type == null)
                typeName = obj == null ? "empty" : obj.GetType().FullName;
            else
                typeName = type.FullName;
            return new InvalidOperationException($"The '{objectName}' is not initialized, type '{typeName}'. {hint}");
        }

        internal static Exception ObjectInitialized(string objectName, object obj, string hint = null)
        {
            string typeName = obj == null ? "empty" : obj.GetType().FullName;
            return new InvalidOperationException($"The '{objectName}' is already initialized, type '{typeName}'. {hint}");
        }

        internal static Exception EnumOutOfRange(string paramName, Enum @enum)
        {
            return new ArgumentOutOfRangeException(paramName, $"Unhandled enum - '{@enum}'");
        }

        internal static Exception ValidatorInitialized(object validator)
        {
            return ObjectInitialized("Validator", validator,
                "The validator already has a contex, use the Clone method to create a new validator.");
        }

        internal static Exception ValidatorNotInitialized(object validator)
        {
            return ObjectNotInitialized("Validator", validator,
                "Call the 'Initialize(IValidatorContext)' method to initialize the validator.");
        }

        internal static Exception EditorNotInitialized(object editor)
        {
            return ObjectNotInitialized("EditableViewModel", editor,
                "To initialize editor call the InitializeEntity method.");
        }

        internal static Exception ViewNotFound(Type viewModelType, Type viewType = null)
        {
            string viewName = viewType == null ? "view" : viewType.FullName;
            return new InvalidOperationException($@"Unable to find a suitable '{viewName}' for the '{viewModelType}'.");
        }

        internal static Exception ViewModelNotFound(Type viewType)
        {
            return new InvalidOperationException($@"Unable to find a suitable view model for the '{viewType}'.");
        }

        internal static Exception ViewModelCannotBeRestored()
        {
            return new InvalidOperationException("Unable to restore a view model.");
        }

        internal static Exception ResourceNotFound(string resourceName, Type resourceType)
        {
            return new InvalidOperationException($"Resource with the name '{resourceName}' was not found in the '{resourceType}'.");
        }

        internal static Exception ResourceNotString(string resourceName, Type resourceType)
        {
            return new InvalidOperationException($"Resource with the name '{resourceName}' in the '{resourceType}', is not a string.");
        }

        internal static Exception ResourceHasNotGetter(string resourceName, Type resourceType)
        {
            return new InvalidOperationException($"Resource with the name '{resourceName}' in the '{resourceType}', does not have a get method.");
        }

        internal static Exception DuplicateViewMapping(Type viewType, Type viewModelType, string name)
        {
            return new InvalidOperationException($"The mapping already exist for the '{viewType}' to the '{viewModelType}' with name '{name}'");
        }

        internal static Exception DuplicateInterface(string itemName, string interfaceName, Type type)
        {
            return new InvalidOperationException(
                    $"The '{itemName}' can implement an interface '{interfaceName}' only once. The '{itemName}' with type '{type}', implement it more that once.");
        }

        internal static Exception CommandCannotBeExecuted()
        {
            return new InvalidOperationException(CommandCannotBeExecutedString);
        }

        internal static Exception IntOutOfRangeCollection(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName, "Index must be within the bounds of the collection.");
        }

        internal static Exception CapacityLessThanCollection(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName, "The Capacity should be greater or equal than collection.");
        }

        internal static Exception DuplicateItemCollection(object item)
        {
            return new ArgumentException($"The item '{item}' already in the collection.");
        }

        internal static Exception MissingMetadataProperty(Type type, string propertyName, Type classType)
        {
            return new MissingMemberException($"The type '{type}' does not contain property with name '{propertyName}', declareted in MetadataType '{classType.FullName}'");
        }


        internal static Exception MissingMetadataField(Type type, string fieldName, Type classType)
        {
            return new MissingMemberException($"The type '{type}' does not contain field with name '{fieldName}', declareted in MetadataType '{classType.FullName}'");
        }

        internal static Exception DataConstantNotFound(DataConstant dataConstant)
        {
            return new InvalidOperationException($"The DataContext doesn't contain the DataConstant with id '{dataConstant.Id}'");
        }

        internal static Exception DataConstantCannotBeNull(DataConstant dataConstant)
        {
            return new InvalidOperationException($"The DataConstant cannot be null, id '{dataConstant.Id}'");
        }

        internal static Exception ObjectDisposed(Type type)
        {
            return new ObjectDisposedException(type.FullName, $@"Cannot perform the operation, because the current '{type}' is disposed.");
        }

        internal static Exception WrapperTypeShouldBeNonAbstract(Type wrapperType)
        {
            return new ArgumentException($"The wrapper type '{wrapperType}' must be non abstract", nameof(wrapperType));
        }


        internal static Exception WrapperTypeNotSupported(Type wrapperType)
        {
            return new ArgumentException($"There are no wrapper type for type '{wrapperType}'.", nameof(wrapperType));
        }

        internal static Exception PresenterCannotShowViewModel(Type presenterType, Type vmType)
        {
            return new ArgumentException($"The presenter '{presenterType}' cannot display the '{vmType}'.");
        }

        internal static Exception ExpressionShouldBeStaticValue(Expression expression, Exception exception)
        {
            return new InvalidOperationException($"The '{expression}' expression cannot be compiled in a static value.", exception);
        }

        #endregion
    }
}
