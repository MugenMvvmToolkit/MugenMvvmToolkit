#region Copyright

// ****************************************************************************
// <copyright file="ViewModelDataTemplateModule.cs">
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
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;

#if WPF
using MugenMvvmToolkit.WPF.Binding.Converters;

namespace MugenMvvmToolkit.WPF.Modules
#elif SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Binding.Converters;

namespace MugenMvvmToolkit.Silverlight.Modules
#endif
{
    /// <summary>
    /// Represents the module that creates view data templates for view models.
    /// </summary>
    public class ViewModelDataTemplateModule : ModuleBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleBase" /> class.
        /// </summary>
        public ViewModelDataTemplateModule()
            : base(true, LoadMode.Design | LoadMode.Runtime, int.MinValue)
        {
        }

        #endregion

        #region Methods

        internal static DataTemplate DefaultTemplateProvider(Type vmType)
        {
#if WPF
            var template = (DataTemplate)XamlReader.Parse(
                            @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
<ContentPresenter Content=""{Binding Converter={StaticResource ViewModelToViewConverterInternal}}""/>
</DataTemplate>");
#else
            var template = (DataTemplate)XamlReader.Load(
                            @"<DataTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
<ContentPresenter Content=""{Binding Converter={StaticResource ViewModelToViewConverterInternal}}""/>
</DataTemplate>");
#endif
            template.DataType = vmType;
            return template;
        }

        #endregion

        #region Overrides of ModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            Application application = Application.Current;
            if (application == null || application.Resources == null)
                return false;
            var dictionary = application.Resources;
            var converter = new ViewModelToViewConverter();
            SetItem(dictionary, "ViewModelToViewConverterInternal", converter);
            SetItem(dictionary, "ViewModelToViewConverter", converter);
            var container = IocContainer ?? ServiceProvider.IocContainer;
            IViewMappingProvider provider;
            if (container == null || !container.TryGet(out provider))
            {
                foreach (Assembly assembly in Context.Assemblies)
                    foreach (Type type in assembly.SafeGetTypes(!Mode.IsDesignMode()))
                        AddTemplate(dictionary, type);
            }
            else
            {
                foreach (var viewMappingItem in provider.ViewMappings)
                    AddTemplate(dictionary, viewMappingItem.ViewModelType);
            }
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion

        #region Methods

        private static void AddTemplate(ResourceDictionary dictionary, Type type)
        {
            if (!typeof(IViewModel).IsAssignableFrom(type))
                return;

            DataTemplate dataTemplate = PlatformExtensions.DefaultViewModelTemplateFactory(type);
            if (dataTemplate != null && SetItem(dictionary, new DataTemplateKey(type), dataTemplate))
                Tracer.Info("The DataTemplate with key type {0} was added.", type);
        }

        private static bool SetItem(ResourceDictionary dictionary, object key, object item)
        {
            if (dictionary.Contains(key))
                return false;
            dictionary.Add(key, item);
            return true;
        }

        #endregion
    }
}