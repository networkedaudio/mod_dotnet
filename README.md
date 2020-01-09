# mod_coreclr

The future home of a new Core .NET 3.1 based language plugin module.


Build Notes:

- Install (or build) FreeSWITCH
- Install dotnet-sdk-3.1 package from Microsoft
- Add the currently missing ldconfig file to /etc/ld.so.conf.d/nethost.conf which points to the right spot for libnethost and refresh ldconfig cache
  - IE: ```echo "/usr/share/dotnet/packs/Microsoft.NETCore.App.Host.linux-x64/3.1.0/runtimes/linux-x64/native" > /etc/ld.so.conf.d/nethost.conf && ldconfig```
  - If the path is different from above, update Makefile.am include correct paths
- Run ```./bootstrap.sh``` to build configure script
- Run ```./configure``` to build the Makefile
- Temporary: Update LOADER_PATH and LOADER_RUNTIME_CONFIG_PATH in mod_coreclr.c to reflect where to find the compiled managed Loader.dll, Loader.runtimeconfig.json and Loader.deps.json files
- Run ```make``` to build mod_coreclr
- Run ```make install``` to install mod_coreclr.so to the FreeSWITCH modules directory
- Run ```dotnet build Loader``` to build the managed Loader.dll and produce required files to the LoaderRuntime subdirectory
- Copy files from LoaderRuntime to the LOADER_PATH directory

Todo Build Notes:

- Dependency check for dotnet-sdk-3.1, export include and libs path for libnethost and nethost.h (issue to dotnet core team to include entry in /usr/lib/pkg-config so paths can be pulled with pkg-config)
- Update Makefile.am to use exported libnethost paths
- Injecting a base LOADER_PATH with directory only (somewhere near FS installed modules), the Loader is special and should not be stored with other User assemblies that are loaded later nor with the actual FS modules
- Have default "make" target call "dotnet build Loader" and "make install" target also copy the LoaderRuntime files to the injected LOADER_PATH
