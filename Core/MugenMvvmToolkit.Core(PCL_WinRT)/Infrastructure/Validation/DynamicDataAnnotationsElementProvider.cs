#region Copyright
// ****************************************************************************
// <copyright file="DynamicDataAnnotationsElementProvider.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Validation;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represents the dynamic data annotations provider to get the <see cref="IValidationElement" />s for the specified
    ///     instance.
    /// </summary>
    public class DynamicDataAnnotationsElementProvider : IValidationElementProvider
    {
        #region Nested types

        private interface IHasElementProvider
        {
            DynamicDataAnnotationsElementProvider ElementProvider { get; set; }
        }

        private sealed class DynamicValidatableObject : IValidationElement, IHasElementProvider
        {
            #region Fields

            private readonly Func<IValidationContext, object> _contexConverter;
            private readonly Func<DynamicDataAnnotationsElementProvider, string> _getDisplayName;
            private readonly Func<object, object[], object> _methodDelegate;
            private readonly Func<object, IEnumerable<IValidationResult>> _resultConverter;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="DynamicValidatableObject" /> class.
            /// </summary>
            public DynamicValidatableObject(Func<DynamicDataAnnotationsElementProvider, string> getDisplayName,
                Func<IValidationContext, object> contexConverter, Func<object, object[], object> methodDelegate,
                Func<object, IEnumerable<IValidationResult>> resultConverter)
            {
                _getDisplayName = getDisplayName;
                _contexConverter = contexConverter;
                _methodDelegate = methodDelegate;
                _resultConverter = resultConverter;
            }

            #endregion

            #region Implementation of IValidationElement

            /// <summary>
            ///     Determines whether the specified object is valid.
            /// </summary>
            /// <returns>
            ///     A collection that holds failed-validation information.
            /// </returns>
            /// <param name="validationContext">The context information about the validation operation.</param>
            public IEnumerable<IValidationResult> Validate(IValidationContext validationContext)
            {
                validationContext.MemberName = null;
                validationContext.DisplayName = _getDisplayName(ElementProvider);
                return _resultConverter(_methodDelegate(validationContext.ObjectInstance, new[] { _contexConverter(validationContext) }));
            }

            #endregion

            #region Implementation of IHasElementProvider

            public DynamicDataAnnotationsElementProvider ElementProvider { get; set; }

            #endregion
        }

        private sealed class DynamicValidationAttribute : IValidationElement, IHasElementProvider
        {
            #region Fields

            private readonly Func<DynamicDataAnnotationsElementProvider, string> _getDisplayName;
            private readonly Func<object, object> _getPropertyValue;
            private readonly Func<object, IValidationContext, IValidationResult> _getValidationResult;
            private readonly string _memberName;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="DynamicValidationAttribute" /> class.
            /// </summary>
            public DynamicValidationAttribute(string memberName, Func<DynamicDataAnnotationsElementProvider, string> getDisplayName,
                Func<object, object> getPropertyValue,
                Func<object, IValidationContext, IValidationResult> getValidationResult)
            {
                _memberName = memberName;
                _getDisplayName = getDisplayName;
                _getPropertyValue = getPropertyValue;
                _getValidationResult = getValidationResult;
            }

            #endregion

            #region Implementation of IValidationElement

            /// <summary>
            ///     Determines whether the specified object is valid.
            /// </summary>
            /// <returns>
            ///     A collection that holds failed-validation information.
            /// </returns>
            /// <param name="validationContext">The context information about the validation operation.</param>
            public IEnumerable<IValidationResult> Validate(IValidationContext validationContext)
            {
                validationContext.MemberName = _memberName;
                validationContext.DisplayName = _getDisplayName(ElementProvider);
                object value = _getPropertyValue(validationContext.ObjectInstance);
                IValidationResult validationResult = _getValidationResult(value, validationContext);
                if (validationResult == ValidationResult.Success)
                    return Enumerable.Empty<IValidationResult>();
                return new[] { validationResult };
            }

            #endregion

            #region Implementation of IHasElementProvider

            public DynamicDataAnnotationsElementProvider ElementProvider { get; set; }

            #endregion
        }

        private sealed class ValidatableObjectElement : IValidationElement, IHasElementProvider
        {
            #region Fields

            private readonly Func<DynamicDataAnnotationsElementProvider, string> _getDisplayName;

            #endregion

            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="ValidatableObjectElement" /> class.
            /// </summary>
            public ValidatableObjectElement(Func<DynamicDataAnnotationsElementProvider, string> getDisplayName)
            {
                _getDisplayName = getDisplayName;
            }

            #endregion

            #region Implementation of IValidationElement

            /// <summary>
            ///     Determines whether the specified object is valid.
            /// </summary>
            /// <returns>
            ///     A collection that holds failed-validation information.
            /// </returns>
            /// <param name="validationContext">The context information about the validation operation.</param>
            public IEnumerable<IValidationResult> Validate(IValidationContext validationContext)
            {
                validationContext.MemberName = null;
                validationContext.DisplayName = _getDisplayName(ElementProvider);
                var validatableObject = validationContext.ObjectInstance as IValidatableObject;
                if (validatableObject != null)
                {
                    if (validationContext.ServiceProvider != null)
                        validationContext.Items[ServiceProviderKey] = validationContext.ServiceProvider;
                    return validatableObject.Validate(validationContext);
                }
                return Enumerable.Empty<IValidationResult>();
            }

            #endregion

            #region Implementation of IHasElementProvider

            public DynamicDataAnnotationsElementProvider ElementProvider { get; set; }

            #endregion
        }

        #endregion

        #region Fields

        private const MemberFlags InstancePublicFlags = MemberFlags.Public | MemberFlags.Instance;
        public const string ServiceProviderKey = "_ServiceProviderKey_";
        private const string ValidationContextKey = "``````";
        private const string DataAnnotationsNamespace = "System.ComponentModel.DataAnnotations";
        private const string ValidationContextTypeName = DataAnnotationsNamespace + ".ValidationContext";
        private const string ValidationResultTypeName = DataAnnotationsNamespace + ".ValidationResult";

        private const string ValidatableObjectInterfaceShortName = "IValidatableObject";
        private const string MetadataTypeAttributeTypeShortName = "MetadataTypeAttribute";
        private const string ValidateMethodName = "Validate";
        private const string GetValidationResultMethodName = "GetValidationResult";
        private const string ErrorMessageProperty = "ErrorMessage";
        private const string MemberNamesProperty = "MemberNames";
        private const string DisplayNameProperty = "DisplayName";
        private const string MemberNameProperty = "MemberName";
        private const string MetadataClassTypeProperty = "MetadataClassType";

        private static readonly Dictionary<Type, Func<IValidationContext, object>> ValidationContextCache =
            new Dictionary<Type, Func<IValidationContext, object>>();

        private static readonly Dictionary<Type, Func<object, IValidationResult>> ValidationResultCache =
            new Dictionary<Type, Func<object, IValidationResult>>();

        private static readonly Dictionary<Type, IDictionary<string, IList<IValidationElement>>> ElementsCache =
            new Dictionary<Type, IDictionary<string, IList<IValidationElement>>>();

        private static readonly Dictionary<object, object> CheckDictionary = new Dictionary<object, object>();
        private static readonly Type[] ValidationContextThreeTypesConstructor =
        {
            typeof (object),
            typeof (IServiceProvider),
            typeof (IDictionary<object, object>)
        };

        private static readonly Type[] ValidationContextTwoTypesConstructor =
        {
            typeof (object),
            typeof (IDictionary<object, object>)
        };

        private readonly IDisplayNameProvider _displayNameProvider;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicDataAnnotationsElementProvider" /> class.
        /// </summary>
        public DynamicDataAnnotationsElementProvider([NotNull] IDisplayNameProvider displayNameProvider)
        {
            Should.NotBeNull(displayNameProvider, "displayNameProvider");
            _displayNameProvider = displayNameProvider;
        }

        #endregion

        #region Implementation of IValidationElementProvider

        /// <summary>
        ///     Gets the series of instances of <see cref="IValidationElement" /> for the specified instance.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <returns>A series of instances of <see cref="IValidationElement" />.</returns>
        public IDictionary<string, IList<IValidationElement>> GetValidationElements(object instance)
        {
            Should.NotBeNull(instance, "instance");
            Type type = instance.GetType();
            lock (ElementsCache)
            {
                IDictionary<string, IList<IValidationElement>> result;
                if (!ElementsCache.TryGetValue(type, out result))
                {
                    result = new Dictionary<string, IList<IValidationElement>>();
                    IList<IValidationElement> elements = TryGetValidatableObjectMethods(type);
                    if (elements.Count != 0)
                        result[string.Empty] = new List<IValidationElement>(elements);
                    if (instance is IValidatableObject)
                    {
                        if (!result.TryGetValue(string.Empty, out elements))
                        {
                            elements = new List<IValidationElement>();
                            result[string.Empty] = elements;
                        }
#if PCL_WINRT
                        var typeInfo = type.GetTypeInfo();
                        elements.Add(new ValidatableObjectElement(d => GetDisplayName(d, typeInfo)));
#else
                        elements.Add(new ValidatableObjectElement(d => GetDisplayName(d, type)));
#endif
                    }
                    FillValidationAttributes(type, result);
                    var toRemove = result.Where(pair => pair.Value.Count == 0).ToList();
                    for (int index = 0; index < toRemove.Count; index++)
                        result.Remove(toRemove[index]);
                    ElementsCache[type] = result;
                }
                foreach (var provider in result.Values.SelectMany(list => list).OfType<IHasElementProvider>())
                    provider.ElementProvider = this;
                return result;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds a series of instances of <see cref="IValidationElement" /> to the specified dictionary.
        /// </summary>
        protected virtual void FillValidationAttributes(Type type,
            IDictionary<string, IList<IValidationElement>> elements)
        {
            const MemberFlags flags = MemberFlags.Public | MemberFlags.Instance;
            Dictionary<MemberInfo, HashSet<MemberInfo>> originalMembers = type
                .GetPropertiesEx(flags)
                .Where(info => info.CanRead && info.GetGetMethod(true).GetParameters().Length == 0)
                .ToDictionary(info => (MemberInfo)info, info => new HashSet<MemberInfo> { info });

            foreach (var field in type.GetFieldsEx(flags))
                originalMembers.Add(field, new HashSet<MemberInfo> { field });
            ICollection<Type> metadataTypes = GetMetadataTypes(type);
            foreach (Type metadataType in metadataTypes)
            {
                foreach (PropertyInfo metadataProperty in metadataType.GetPropertiesEx(flags))
                {
                    KeyValuePair<MemberInfo, HashSet<MemberInfo>> valuePair = originalMembers
                        .FirstOrDefault(pair => pair.Key is PropertyInfo && pair.Key.Name == metadataProperty.Name);
                    if (valuePair.Value == null)
                        throw ExceptionManager.MissingMetadataProperty(type, metadataProperty.Name, metadataProperty.DeclaringType);
                    valuePair.Value.Add(metadataProperty);
                }

                foreach (FieldInfo metadataField in metadataType.GetFieldsEx(flags))
                {
                    KeyValuePair<MemberInfo, HashSet<MemberInfo>> valuePair = originalMembers
                        .FirstOrDefault(pair => pair.Key is FieldInfo && pair.Key.Name == metadataField.Name);
                    if (valuePair.Value == null)
                        throw ExceptionManager.MissingMetadataField(type, metadataField.Name, metadataField.DeclaringType);
                    valuePair.Value.Add(metadataField);
                }
            }

            foreach (var keyValuePair in originalMembers)
            {
                IList<IValidationElement> list;
                if (!elements.TryGetValue(keyValuePair.Key.Name, out list))
                {
                    list = new List<IValidationElement>();
                    elements[keyValuePair.Key.Name] = list;
                }
                foreach (MemberInfo member in keyValuePair.Value)
                {
                    list.AddRange(GetValidationAttributes(keyValuePair.Key, member));
                }
            }
        }

        /// <summary>
        ///     Tries to get a series of instances of <see cref="IValidationElement" /> that represents the IValidatableObject
        ///     interface.
        /// </summary>
        [NotNull]
        protected virtual IList<IValidationElement> TryGetValidatableObjectMethods(Type type)
        {
            IEnumerable<MethodInfo> methods = type
                .GetInterfaces()
                .Where(t => t.Name == ValidatableObjectInterfaceShortName)
                .SelectMany(t => t.GetMethodsEx(InstancePublicFlags))
                .Where(info =>
                {
#if PCL_WINRT
                    if (info.Name != ValidateMethodName || !info.ReturnType.GetTypeInfo().IsGenericType
                                            || info.ReturnType.GetGenericArguments()[0].FullName != ValidationResultTypeName)
                        return false;
#else
                    if (info.Name != ValidateMethodName || !info.ReturnType.IsGenericType
                                            || info.ReturnType.GetGenericArguments()[0].FullName != ValidationResultTypeName)
                        return false;
#endif

                    ParameterInfo[] parameterInfos = info.GetParameters();
                    return parameterInfos.Length == 1 &&
                           parameterInfos[0].ParameterType.FullName == ValidationContextTypeName;
                });
            var elements = new List<IValidationElement>();
            foreach (MethodInfo methodInfo in methods)
            {
                Type validatioResultType = methodInfo.ReturnType.GetGenericArguments()[0];
                Type validationContextType = methodInfo.GetParameters()[0].ParameterType;
                Func<IValidationContext, object> contextConverter =
                    TryGetValidationContextConverter(validationContextType);
                Func<object, IValidationResult> validationResultConverter =
                    TryGetValidationResultConverter(validatioResultType);
                if (contextConverter == null || validationResultConverter == null)
                    continue;
                Func<object, object[], object> methodDelegate = ServiceProvider.ReflectionManager.GetMethodDelegate(methodInfo);
#if PCL_WINRT
                var typeInfo = type.GetTypeInfo();
                Func<DynamicDataAnnotationsElementProvider, string> displayNameAccessor = d => GetDisplayName(d, typeInfo);
#else
                Func<DynamicDataAnnotationsElementProvider, string> displayNameAccessor = d => GetDisplayName(d, type);
#endif
                Tracer.Info("Type {0} implements IValidatableObject", type);
                elements.Add(new DynamicValidatableObject(displayNameAccessor, contextConverter, methodDelegate,
                    o => ConvertValidationResults(o, validationResultConverter)));
            }
            return elements;
        }

        /// <summary>
        ///     Tries to get converter to convert a dynamic validation result to the IValidationResult.
        /// </summary>
        protected virtual Func<object, IValidationResult> TryGetValidationResultConverterInternal(Type type)
        {
            PropertyInfo errorProperty = type.GetPropertyEx(ErrorMessageProperty, InstancePublicFlags);
            PropertyInfo memberNamesProperty = type.GetPropertyEx(MemberNamesProperty, InstancePublicFlags);
            if (errorProperty != null && memberNamesProperty != null &&
                memberNamesProperty.PropertyType.Equals(typeof(IEnumerable<string>)) &&
                errorProperty.PropertyType.Equals(typeof(string)))
            {
                Func<object, string> errorAccess = ServiceProvider.ReflectionManager.GetMemberGetter<string>(errorProperty);
                Func<object, IEnumerable<string>> namesAccess = ServiceProvider.ReflectionManager.GetMemberGetter<IEnumerable<string>>(memberNamesProperty);
                return o =>
                {
                    if (o == null)
                        return ValidationResult.Success;
                    return new ValidationResult(errorAccess(o), namesAccess(o));
                };
            }
            return null;
        }

        /// <summary>
        ///     Tries to get converter to convert the IValidationContext to a dynamic validation context.
        /// </summary>
        protected virtual Func<IValidationContext, object> TryGetValidationContextConverterInternal(Type type)
        {
            bool isThree = true;
            ConstructorInfo constructor = type.GetConstructor(ValidationContextThreeTypesConstructor);
            if (constructor != null)
            {
                //NOTE WinRT allow to get the constructor with three args but it fails on create.
                try
                {
                    constructor.InvokeEx(this, ServiceProvider.IocContainer, CheckDictionary);
                }
                catch
                {
                    constructor = null;
                }
            }

            if (constructor == null)
            {
                constructor = type.GetConstructor(ValidationContextTwoTypesConstructor);
                isThree = false;
            }
            if (constructor == null)
                return null;

            PropertyInfo displayNameProp = type.GetPropertyEx(DisplayNameProperty, InstancePublicFlags);
            PropertyInfo memberNameProp = type.GetPropertyEx(MemberNameProperty, InstancePublicFlags);
            if (displayNameProp == null || memberNameProp == null ||
                !displayNameProp.PropertyType.Equals(typeof(string)) ||
                !memberNameProp.PropertyType.Equals(typeof(string)))
                return null;
            Action<object, string> displayNameSetter = ServiceProvider.ReflectionManager.GetMemberSetter<string>(displayNameProp);
            Action<object, string> memberNameSetter = ServiceProvider.ReflectionManager.GetMemberSetter<string>(memberNameProp);
            Func<object[], object> activatorDelegate = ServiceProvider.ReflectionManager.GetActivatorDelegate(constructor);
            return context => ConverterValidationContext(context, type, activatorDelegate, displayNameSetter, memberNameSetter, isThree);
        }

        /// <summary>
        ///     Gets validation attributes for the specified member.
        /// </summary>
        protected ICollection<IValidationElement> GetValidationAttributes(MemberInfo originalMember,
            MemberInfo metadataMember)
        {
            var validationAttributes = new List<IValidationElement>();
            Attribute[] attributes = metadataMember.GetAttributes();
            foreach (Attribute attribute in attributes)
            {
                Type type = attribute.GetType();
                MethodInfo method = type
                    .GetMethodsEx(InstancePublicFlags)
                    .FirstOrDefault(info =>
                    {
                        if (info.Name != GetValidationResultMethodName ||
                            info.ReturnType.FullName != ValidationResultTypeName)
                            return false;
                        ParameterInfo[] parameters = info.GetParameters();
                        return parameters.Length == 2 && parameters[0].ParameterType.Equals(typeof(object)) &&
                               parameters[1].ParameterType.FullName == ValidationContextTypeName;
                    });
                if (method == null)
                    continue;
                ParameterInfo[] @params = method.GetParameters();
                Func<object, IValidationResult> resultConverter = TryGetValidationResultConverter(method.ReturnType);
                Func<IValidationContext, object> validationContextConverter =
                    TryGetValidationContextConverter(@params[1].ParameterType);
                if (resultConverter == null || validationContextConverter == null)
                    continue;
                Func<object, object[], object> methodDelegate = ServiceProvider.ReflectionManager.GetMethodDelegate(method);
                Func<object, object> getPropertyValue = ServiceProvider.ReflectionManager.GetMemberGetter<object>(originalMember);
                Attribute attr = attribute;
                Func<DynamicDataAnnotationsElementProvider, string> displayNameAccessor = d => GetDisplayName(d, originalMember);
                var element = new DynamicValidationAttribute(originalMember.Name, displayNameAccessor, getPropertyValue,
                    (o, context) =>
                        resultConverter(methodDelegate(attr, new[] { o, validationContextConverter(context) })));
                Tracer.Info("Added a '{0}' validation attribute for type: '{1}', member: '{2}'", attr,
                    originalMember.DeclaringType, originalMember);
                validationAttributes.Add(element);
            }
            return validationAttributes;
        }

        /// <summary>
        ///     Tries to get converter to convert a dynamic validation result to the IValidationResult.
        /// </summary>
        protected Func<object, IValidationResult> TryGetValidationResultConverter(Type type)
        {
            lock (ValidationResultCache)
            {
                Func<object, IValidationResult> value;
                if (!ValidationResultCache.TryGetValue(type, out value))
                {
                    value = TryGetValidationResultConverterInternal(type);
                    ValidationResultCache[type] = value;
                }
                return value;
            }
        }

        /// <summary>
        ///     Tries to get converter to convert the IValidationContext to a dynamic validation context.
        /// </summary>
        protected Func<IValidationContext, object> TryGetValidationContextConverter(Type type)
        {
            lock (ValidationContextCache)
            {
                Func<IValidationContext, object> value;
                if (!ValidationContextCache.TryGetValue(type, out value))
                {
                    value = TryGetValidationContextConverterInternal(type);
                    ValidationContextCache[type] = value;
                }
                return value;
            }
        }

        /// <summary>
        ///     Gets the metatata types for the specified type.
        /// </summary>
        protected internal static ICollection<Type> GetMetadataTypes(Type type)
        {
            Attribute[] attributes = type.GetAttributes();
            var provider = ServiceProvider.EntityMetadataTypeProvider;
            IEnumerable<Type> types = null;
            if (provider != null)
                types = provider(type);
            if (types == null)
                types = Enumerable.Empty<Type>();
            else
            {
                foreach (var meta in types)
                    TraceMeta(type, meta);
            }
            var result = new HashSet<Type>(types);
            foreach (Attribute attribute in attributes)
            {
                var metadataTypeAttribute = attribute as MetadataTypeAttribute;
                if (metadataTypeAttribute != null)
                {
                    foreach (var meta in metadataTypeAttribute.GetTypes(type))
                    {
                        if (result.Add(meta))
                            TraceMeta(type, meta);
                    }
                    continue;
                }

                Type attrType = attribute.GetType();
                if (attrType.Name != MetadataTypeAttributeTypeShortName)
                    continue;
                PropertyInfo property = attrType.GetPropertyEx(MetadataClassTypeProperty, InstancePublicFlags);
                if (property == null || !property.PropertyType.Equals(typeof(Type)))
                    continue;
                var metaType = property.GetValueEx<Type>(attribute);
                if (metaType != null)
                {
                    if (result.Add(metaType))
                        TraceMeta(type, metaType);
                }
            }
            return result;
        }

        private static void TraceMeta(Type defType, Type metaType)
        {
            if (metaType != null)
                Tracer.Info("Added MetadataTypeAttribute for type: {0}, MetadataClassType: {1}", defType, metaType);
        }

        private static object ConverterValidationContext(IValidationContext context, Type contextType,
            Func<object[], object> activatorDelegate, Action<object, string> displayNameSetter,
            Action<object, string> memberNameSetter, bool isThree)
        {
            if (context.ServiceProvider != null)
                context.Items[ServiceProviderKey] = context.ServiceProvider;
            object contextResult;
            if (context.Items != null)
            {
                string key = contextType.FullName + ValidationContextKey;
                if (!context.Items.TryGetValue(key, out contextResult))
                {
                    contextResult = activatorDelegate(isThree
                        ? new[] { context.ObjectInstance, context.ServiceProvider, context.Items }
                        : new[] { context.ObjectInstance, context.Items });
                    context.Items[key] = contextResult;
                }
            }
            else
            {
                contextResult = activatorDelegate(isThree
                    ? new[] { context.ObjectInstance, context.ServiceProvider, context.Items }
                    : new[] { context.ObjectInstance, context.Items });
            }
            displayNameSetter(contextResult, context.DisplayName);
            memberNameSetter(contextResult, context.MemberName);
            return contextResult;
        }

        private static IEnumerable<IValidationResult> ConvertValidationResults(object results,
            Func<object, IValidationResult> converter)
        {
            var enumerable = results as IEnumerable;
            if (enumerable == null)
                return Enumerable.Empty<IValidationResult>();
            return enumerable.Cast<object>().Select(converter).ToList();
        }

        private static string GetDisplayName(DynamicDataAnnotationsElementProvider elementProvider, MemberInfo member)
        {
            if (elementProvider._displayNameProvider == null)
                return member.Name;
            return elementProvider._displayNameProvider.GetDisplayNameAccessor(member).Invoke();
        }

        #endregion
    }
}