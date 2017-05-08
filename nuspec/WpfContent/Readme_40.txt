The steps to get this WPF project working are:

1. Open App.xaml.cs and replace the code to create IoC container:
	new Bootstrapper<MainViewModel>(this, new IIocContainer())
2. Remove any old windows