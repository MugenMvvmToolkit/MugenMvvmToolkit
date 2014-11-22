The steps to get this WinForms project working are:

1. Add a reference to your PCL project
2. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection).
3. Open Program.cs and replace the code to create IoC container:
	new Bootstrapper<MainViewModel>(new IIocContainer())
4. In Solution Explorer select files MainView.cs and MainView.Designer.cs then click right mouse button and select 'Group Items' item. 
5. Remove any old `Form1` forms