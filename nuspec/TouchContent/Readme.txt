The steps to get this iOS project working are:

1. Add a reference to your PCL project
2. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection)
3. Open AppDelegate.cs and replace the code to create IoC container:
	new Bootstrapper<MainViewModel>(_window, new IIocContainer())