The steps to get this WPF project working are:

1. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection).
2. Open App.xaml.cs and replace the code to create IoC container:
	new Bootstrapper<MainViewModel>(this, new IIocContainer())