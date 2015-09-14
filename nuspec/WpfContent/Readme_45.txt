The steps to get this WPF project working are:

1. Add a reference to your PCL project
2. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection)
3. Open App.xaml.cs and replace the code to create IoC container:
	new Bootstrapper<Core.App>(this, new IIocContainer())
4. Remove any old windows