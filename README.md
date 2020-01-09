# mod_coreclr

The future home of a new Core .NET 3.1 based language plugin module.


Build Notes:

- Install (or build) FreeSWITCH
- Install dotnet-sdk-3.1 package from Microsoft
- Add the currently missing ldconfig file to /etc/ld.so.conf.d/nethost.conf which points to the right spot for libnethost and refresh ldconfig cache
- IE: ```echo "/usr/share/dotnet/packs/Microsoft.NETCore.App.Host.linux-x64/3.1.0/runtimes/linux-x64/native" > /etc/ld.so.conf.d/nethost.conf && ldconfig```
- If the path is different from above, update Makefile.am include correct paths
- Run "./bootstrap.sh" to build configure script
- Run "./configure" to build the Makefile
- Temporary: Update LOADER_PATH and LOADER_RUNTIME_CONFIG_PATH in mod_coreclr.c to reflect where to find the compiled managed Loader
- Run "make" to build mod_coreclr
- Run "make install" to install mod_coreclr.so
- Run "dotnet build" from the Loader subdirectory to build the managed loader, which currently copies out to LoaderRuntime subdirectory
- Copy files from LoaderRuntime to the LOADER_PATH directory
