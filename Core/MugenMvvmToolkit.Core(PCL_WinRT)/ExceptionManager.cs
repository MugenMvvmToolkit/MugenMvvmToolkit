#region Copyright

// ****************************************************************************
// <copyright file="ExceptionManager.cs">
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
using System.Linq.Expressions;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    internal static class ExceptionManager
    {
        #region Fields

        internal const string CommandCannotBeExecutedString =
            "The method Execute in RelayCommand cannot be executed because the CanExecute method returns a false value.";

        #endregion

        #region Methods

        internal static Exception ObjectNotInitialized(string objectName, object obj, string hint = null)
        {
            var type = obj as Type;
            if (type != null)
                obj = type.FullName;
            string typeName = obj == null ? "empty" : obj.GetType().FullName;
            return
                new InvalidOperationException(string.Format("The '{0}' is not initialized, type '{1}'. {2}", objectName,
                    typeName, hint));
        }

        internal static Exception ObjectInitialized(string objectName, object obj, string hint = null)
        {
            string typeName = obj == null ? "empty" : obj.GetType().FullName;
            return
                new InvalidOperationException(string.Format("The '{0}' is already initialized, type '{1}'. {2}",
                    objectName, typeName, hint));
        }

        internal static Exception EnumOutOfRange(string paramName, Enum @enum)
        {
            return new ArgumentOutOfRangeException(paramName, string.Format("Unhandled enum - '{0}'", @enum));
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
            return new InvalidOperationException(string.Format(@"Unable to find a suitable '{0}' for the '{1}'.", viewName,
                    viewModelType));
        }

        internal static Exception ViewModelNotFound(Type viewType)
        {
            return new InvalidOperationException(
                    string.Format(@"Unable to find a suitable view model for the '{0}'.", viewType));
        }

        internal static Exception ViewModelCannotBeRestored()
        {
            return new InvalidOperationException("Unable to restore a view model.");
        }

        internal static Exception ResourceNotFound(string resourceName, Type resourceType)
        {
            return new InvalidOperationException(
                string.Format("Resource with the name '{0}' was not found in the '{1}'.", resourceName,
                    resourceType));
        }

        internal static Exception ResourceNotString(string resourceName, Type resourceType)
        {
            return new InvalidOperationException(
                string.Format("Resource with the name '{0}' in the '{1}', is not a string.", resourceName,
                    resourceType));
        }

        internal static Exception ResourceHasNotGetter(string resourceName, Type resourceType)
        {
            return new InvalidOperationException(
                string.Format("Resource with the name '{0}' in the '{1}', does not have a get method.",
                    resourceName, resourceType));
        }

        internal static Exception DuplicateViewMapping(Type viewType, Type viewModelType, string name)
        {
            return new InvalidOperationException(
                string.Format("The mapping already exist for the '{0}' to the '{1}' with name '{2}'", viewType,
                    viewModelType, name));
        }

        internal static Exception DuplicateInterface(string itemName, string interfaceName, Type type)
        {
            return new InvalidOperationException(
                string.Format(
                    "The '{0}' can implement an interface '{1}' only once. The '{0}' with type '{2}', implement it more that once.",
                    itemName, interfaceName, type));
        }

        internal static Exception CommandCannotBeExecuted()
        {
            return new InvalidOperationException(CommandCannotBeExecutedString);
        }

        internal static Exception WindowOpened()
        {
            return new InvalidOperationException(
                "The dialog is open. Before create a new dialog you should close the previous one.");
        }

        internal static Exception WindowClosed()
        {
            return new InvalidOperationException(
                "The dialog is closed. Before close the dialog you should show it.");
        }

        internal static Exception NotConvertableState(EntityState from, EntityState to)
        {
            return
                new InvalidOperationException(string.Format("The '{0}' cannot be converted to the '{1}'.",
                    from, to));
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
            return new ArgumentException(string.Format("The item '{0}' already in the collection.", item));
        }

        internal static Exception MissingMetadataProperty(Type type, string propertyName, Type classType)
        {
            return new MissingMemberException(
                string.Format(
                    "The type '{0}' does not contain property with name '{1}', declareted in MetadataType '{2}'", type,
                    propertyName, classType.FullName));
        }


        internal static Exception MissingMetadataField(Type type, string fieldName, Type classType)
        {
            return new MissingMemberException(
                string.Format(
                    "The type '{0}' does not contain field with name '{1}', declareted in MetadataType '{2}'", type,
                    fieldName, classType.FullName));
        }

        internal static Exception DataConstantNotFound(DataConstant dataConstant)
        {
            return
                new InvalidOperationException(string.Format("The DataContext doesn't contain the DataConstant with id '{0}'", dataConstant.Id));
        }

        internal static Exception DataConstantCannotBeNull(DataConstant dataConstant)
        {
            return new InvalidOperationException(string.Format("The DataConstant cannot be null, id '{0}'", dataConstant.Id));
        }

        internal static Exception ObjectDisposed(Type type)
        {
            return new ObjectDisposedException(type.FullName,
                string.Format(@"Cannot perform the operation, because the current '{0}' is disposed.", type));
        }

        internal static Exception WrapperTypeShouldBeNonAbstract(Type wrapperType)
        {
            return new ArgumentException(string.Format("The wrapper type '{0}' must be non abstract", wrapperType),
                "wrapperType");
        }


        internal static Exception WrapperTypeNotSupported(Type wrapperType)
        {
            return new ArgumentException(string.Format("There are no wrapper type for type '{0}'.", wrapperType),
                "wrapperType");
        }

        internal static Exception PresenterCannotShowViewModel(Type presenterType, Type vmType)
        {
            return
                new ArgumentException(string.Format("The presenter '{0}' cannot display the '{1}'.",
                    presenterType, vmType));
        }

        internal static Exception ExpressionShouldBeStaticValue(Expression expression, Exception exception)
        {
            return
                new InvalidOperationException(
                    string.Format("The '{0}' expression cannot be compiled in a static value.", expression), exception);
        }

        #endregion
    }
}
