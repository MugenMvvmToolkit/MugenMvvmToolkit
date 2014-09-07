![MugenMvvmToolkit](https://raw.githubusercontent.com/MugenMvvmToolkit/MugenMvvmToolkit/master/logo_horizontal.png)

----------
Mugen MVVM Toolkit makes it easier to develop cross-platform application using the Model-View-ViewModel design pattern.
The Mugen MVVM Toolkit provides a cross-platform MVVM development framework built on top of:

 - WinForms
 - WPF
 - Silverlight 5
 - Silverlight for WP7.1, WP8, WP8.1
 - Mono for Android
 - WinRT XAML framework for Windows 8 Store apps

The Mugen MVVM Toolkit makes extensive use of Portable Class Libraries to provide maintainable cross platform C# native applications.
      The core of toolkit contains a navigation system, windows management system, IocContainer, validation, etc.
      The Mugen MVVM Toolkit provides a powerful bindings that you can use on any platform, here are some features:

 - All of the features available for WPF platform available on all
   platforms and even more
 - Binding supports C# language expressions like Linq, Extension
   methods, method call, etc. example:  `(Text Items.Where(x => x ==
   Name)), (Text $string.Format('{0} {1}', Prop1, Prop2))`
 - Supports subscription to any control event. `(TextChanged
   ViewModelMethod($args))`
 - Built-in support for validation example: `(Text Prop1,
   Validate=True), (Text $GetErrors(Prop1).FirstOrDefault())`
 - You can easily write their own extensions for bindings, example:
   `(Text $i18.MyLocalizableString), (Text $MyCustomMethod(Prop1))`
 - Supports attached members (properties, events, methods), you can
   extend any object as you want
 - Special editor for WinForms, you do not need to write code to create
   bindings
 - Supports fluent syntax
 - Excellent performance
