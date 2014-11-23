The steps to get this Android project working are:

1. Add a reference to your PCL project
2. Install the preferred IoC container from nuget (MugenMvvmToolkit - Autofac, MugenMvvmToolkit - Ninject, MugenMvvmToolkit - MugenInjection)
3. Open Setup.cs and replace the code to create IoC container:
protected override IIocContainer CreateIocContainer()
{
    return new IIocContainer();
}
4. Remove any old `MainLauncher` activities