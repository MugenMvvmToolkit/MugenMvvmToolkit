The steps to get this WPF project working are:

1. Add a reference to your PCL project
2. Open App.xaml.cs and replace the code to create IoC container:
	new Bootstrapper<Core.App>(this, new IIocContainer())
3. Remove any old windows