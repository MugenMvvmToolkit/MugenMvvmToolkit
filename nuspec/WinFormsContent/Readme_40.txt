The steps to get this WinForms project working are:

1. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection).
2. Open Program.cs and replace the code to create IoC container:
	new Bootstrapper<MainViewModel>(new IIocContainer())