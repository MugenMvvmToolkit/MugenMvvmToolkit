![MugenMvvmToolkit](https://raw.githubusercontent.com/MugenMvvmToolkit/MugenMvvmToolkit/master/logo_horizontal.png)

----------
Mugen MVVM Toolkit makes it easier to develop cross-platform application using the Model-View-ViewModel design pattern. The Mugen MVVM Toolkit makes extensive use of Portable Class Libraries to provide maintainable cross platform C# native applications.
The Mugen MVVM Toolkit provides a cross-platform MVVM development framework built on top of:

 - WinForms
 - WPF
 - Silverlight 5
 - Silverlight for WP7.1, WP8, WP8.1
 - Mono for Android
 - WinRT XAML framework for Windows 8 Store apps

#Features:
The MVVM framework includes the following features that differs this project from other frameworks:
 - Deep integration with each platform.
 - Supports save/restore state for mobile platforms.
 - Full Fragment support for Android, you do not need to worry about activity, fragments, sub-fragments or their state all this makes the framework.
 - Solves the nested user controls problem in MVVM, all view models are dynamically created by you, using the `GetViewModel` and `GetViewModel<TViewModel>` methods and you do not need to use the `ViewModelLocator`.
 - Supports all kinds of navigation like modal window, page navigation, tab navigation, back stack fragment navigation for android. You can also easily add a new kind of navigation.
 - Navigation system works with view models and allows to expect the completion of the operation. You can pass any parameters between view models. 
 An example of how to navigate looks in other frameworks:
 `Navigate<DetailViewModel>(new DetailParameters() { Index = 2 });`
 An example of how to navigate looks in MugenMvvmToolkit:
 `using (var editorVm = GetViewModel<ProductEditorViewModel>())            
{
    var productModel = new ProductModel { Id = Guid.NewGuid() };
    editorVm.InitializeEntity(productModel, true);
    if (!await editorVm.ShowAsync())
        return;
    //Code that will be called after the completion of navigation, and yes, this code will be executed even if the application had been tombstoned and then restored.
}`
*For WinRT and WP you should install the [MugenMvvmToolkit.Fody](https://github.com/MugenMvvmToolkit/MugenMvvmToolkit.Fody) plugin to support async operation restore.
 - Good design mode support, for xaml platforms supports the creation of design view model with any constructor parameters.
 - Supports bindings on all platforms, all of the native binding features available for WPF platform available on all platforms and even more.
 - Binding supports C# language expressions like Linq, Extension methods, method call, ternary operator(?:), coalescing operator(??), etc.  
`(Text Items.First(x => x == Name).Value), (Text $string.Format('{0} {1}', Prop1, Prop2))`.
 - Supports subscription to any control event.
 `(TextChanged ViewModelMethod($args)), (DoulbeClick Command)`.
 - Built-in support for validation. 
`(Text Prop1, Validate=True), (Text $GetErrors(Prop1).FirstOrDefault())`
 - You can easily write their own extensions for bindings.
  `(Text $i18.MyLocalizableString), (Text $MyCustomMethod(Prop1))`.
 - Supports attached members (properties, events, methods), you can extend any object as you want.
 - Special editor for WinForms, you do not need to write code to create bindings.
 - Binding parser builds syntax tree that allows you to easily change or extend the bindings, without manipulation of the raw text.
 - Binding supports fluent syntax.
 - Excellent performance.
