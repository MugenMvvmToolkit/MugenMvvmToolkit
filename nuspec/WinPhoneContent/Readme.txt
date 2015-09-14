The steps to get this WinPhone project working are:

1. Add a reference to your PCL project.
2. Open Properties\WMAppManifest.xml and remove the Navigation Page value.
3. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection)
4. Open App.xaml.cs and replace the code to create IoC container:
	new Bootstrapper<Core.App>(RootFrame, new IIocContainer())
5. Remove any old pages