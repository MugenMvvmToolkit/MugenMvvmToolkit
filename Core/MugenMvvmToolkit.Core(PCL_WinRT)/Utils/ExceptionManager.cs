#region Copyright
// ****************************************************************************
// <copyright file="ExceptionManager.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Utils
{
    /// <summary>
    ///     Represents the class that throws exceptions.
    /// </summary>
    public static class ExceptionManager
    {
        #region Fields

        internal const string CommandCannotBeExecutedString =
            "The method Execute in RelayCommand cannot be executed because the CanExecute method returns a false value.";

        #endregion

        #region Methods

        public static Exception ObjectNotInitialized(string objectName, object obj, string hint = null)
        {
            var type = obj as Type;
            if (type != null)
                obj = type.FullName;
            string typeName = obj == null ? "empty" : obj.GetType().FullName;
            return
                new InvalidOperationException(string.Format("The '{0}' is not initialized, type '{1}'. {2}", objectName,
                    typeName, hint));
        }

        public static Exception ObjectInitialized(string objectName, object obj, string hint = null)
        {
            string typeName = obj == null ? "empty" : obj.GetType().FullName;
            return
                new InvalidOperationException(string.Format("The '{0}' is already initialized, type '{1}'. {2}",
                    objectName,
                    typeName, hint));
        }

        /// <summary>
        ///     Returns an exception if enum is out of range.
        /// </summary>
        public static Exception EnumOutOfRange(string paramName, Enum @enum)
        {
            return new ArgumentOutOfRangeException(paramName, string.Format("Unhandled enum - '{0}'", @enum));
        }

        /// <summary>
        ///     Returns an exception if navigate method is not supported.
        /// </summary>
        public static Exception NavigateNotSupported(Type type, string hint = null)
        {
            return
                new InvalidOperationException(
                    string.Format("The '{0}' provider doesn't support the DataContext without navigation target, {1}", type, hint));
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
            if (viewType == null)
                viewType = typeof(IView);
            return new InvalidOperationException(
                string.Format(@"Unable to find a suitable '{0}' for the '{1}'. 
Make sure that you add a ViewModelAttribute over the desired View or registered it manually.",
                    viewType, viewModelType));
        }

        internal static Exception ViewModelNotFound(Type viewType)
        {
            return new InvalidOperationException(
                    string.Format(@"Unable to find a suitable view model for the '{0}'.", viewType));
        }

        internal static Exception ResourceNotFound(string resourceName, Type resourceType)
        {
            return new InvalidOperationException(
                string.Format("Resource with the name '{0}' is not found in the '{1}'.", resourceName,
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
                string.Format("Resource with the name '{0}' in the '{1}', is not have a get method.",
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
                "The dialog is open. Before create a new dialog you should to close the previous one.");
        }

        internal static Exception WindowClosed()
        {
            return new InvalidOperationException(
                "The dialog is closed. Before close the dialog you should show it.");
        }

        internal static Exception NotConvertableState(Enum from, Enum to)
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

        internal static Exception InvalidContexValidator(object validator)
        {
            return
                new ArgumentException(string.Format(
                    "The specified context cannot be used in the current validator '{0}'.", validator.GetType()));
        }

        internal static Exception DuplicateValidator(Type validatorType)
        {
            return new ArgumentException(
                string.Format(
                    "The validator with type '{0}' already registered, because validator is unique you can add only one instance of it.",
                    validatorType));
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
                new InvalidOperationException(
                    string.Format("The DataContext doesn't contain the DataConstant with id '{0}', type '{1}'",
                        dataConstant.Id, dataConstant.Type));
        }

        internal static Exception DataConstantCannotBeNull(DataConstant dataConstant)
        {
            return
                new InvalidOperationException(string.Format("The DataConstant cannot be null, id '{0}', type '{1}'",
                    dataConstant.Id, dataConstant.Type));
        }

        internal static Exception ObjectDisposed(Type type)
        {
            return new ObjectDisposedException(type.FullName,
                string.Format(@"Cannot perform the operation, because the current '{0}' is disposed.", type));
        }

        internal static Exception IocContainerDisposed(Type type)
        {
            return new ObjectDisposedException(type.FullName,
                string.Format(@"Cannot perform the operation, because the '{0}' is disposed.
If the container has a parent container, check that the parent container was not disposed before.", type));
        }

        internal static Exception WrapperTypeShouldBeNonAbstract(Type wrapperType)
        {
            return new ArgumentException(string.Format("The wrapper type '{0}' must be non abstract", wrapperType),
                "wrapperType");
        }


        internal static Exception WrapperTypeNotSupported(Type wrapperType)
        {
            return new ArgumentException(string.Format("There no wrapper type for type '{0}'.", wrapperType),
                "wrapperType");
        }

        internal static Exception PresenterCannotShowViewModel(Type presenterType, Type vmType)
        {
            return
                new ArgumentException(string.Format("The presenter '{0}' cannot display the '{1}'.",
                    presenterType, vmType));
        }

        #endregion
    }
}