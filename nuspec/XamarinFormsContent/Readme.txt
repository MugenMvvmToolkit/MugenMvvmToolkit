The steps to get this Xamarin.Forms project working are:

1. Add a reference to your PCL project
2. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection)
3. Open App.cs and replace the code to create IoC container:
	new Bootstrapper<MainViewModel>(new IIocContainer())
4. Change Build Action for MainView.xaml to 'Embedded Resource'