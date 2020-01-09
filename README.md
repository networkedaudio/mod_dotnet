# mod_coreclr

The future home of a new Core .NET 3.1 based language plugin module.


Build Notes:

- Install dotnet-sdk-3.1 package from Microsoft
- Add the currently missing ldconfig file to /etc/ld.so.conf.d/nethost.conf which points to the right spot and refresh ldconfig cache
- IE: /usr/share/dotnet/packs/Microsoft.NETCore.App.Host.linux-x64/3.1.0/runtimes/linux-x64/native
- If the path is different from above, update Makefile.am include correct paths
