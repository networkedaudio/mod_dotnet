/*
 * mod_coreclr.c -- Core .NET Plugin Interface
 *
 * Copyright (c) 2019 SignalWire, Inc
 *
 * Author: Shane Bryldt <shane@signalwire.com>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#include <switch.h>
#include <unistd.h>
#include <limits.h>

#include <mod_coreclr.h>
#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>

#ifdef WINDOWS
#include <Windows.h>

#define STR(s) L ## s
#define CH(c) L ## c
#define DIR_SEPARATOR L'\\'

#else
#include <dlfcn.h>
#include <limits.h>

#define STR(s) s
#define CH(c) c
#define DIR_SEPARATOR '/'
#define MAX_PATH PATH_MAX

#endif

// TODO: The base path needs to come in from FS configuration, and need a Loader install concept to copy/publish it to the right location
#define LOADER_PATH "/home/shane/mod_coreclr/LoaderRuntime/Loader.dll"
#define LOADER_RUNTIME_CONFIG_PATH "/home/shane/mod_coreclr/LoaderRuntime/Loader.runtimeconfig.json"

typedef int32_t (*test_callback_t)();

typedef struct interface_callbacks
{
	test_callback_t ontest;
} interface_callbacks_t;

typedef interface_callbacks_t (*loader_entry_fn)();

typedef int (*loader_test_fn)();


SWITCH_MODULE_LOAD_FUNCTION(mod_coreclr_load);
SWITCH_MODULE_SHUTDOWN_FUNCTION(mod_coreclr_shutdown);
SWITCH_MODULE_DEFINITION(mod_coreclr, mod_coreclr_load, mod_coreclr_shutdown, NULL);

void *load_library(const char_t *);
void *get_export(void *, const char *);

#ifdef WINDOWS
void *load_library(const char_t *path)
{
	HMODULE h = ::LoadLibraryW(path);
	assert(h != nullptr);
	return (void*)h;
}
void *get_export(void *h, const char *name)
{
	void *f = ::GetProcAddress((HMODULE)h, name);
	assert(f != nullptr);
	return f;
}
#else
void *load_library(const char_t *path)
{
	void *h = dlopen(path, RTLD_LAZY | RTLD_LOCAL);
	assert(h);
	return h;
}
void *get_export(void *h, const char *name)
{
	void *f = dlsym(h, name);
	assert(f);
	return f;
}
#endif

switch_bool_t load_runtime(interface_callbacks_t *callbacks)
{
	// TODO: a dynamically obtained base path for where the Loader is loaded from
	char_t hostfxr_path[MAX_PATH];
	size_t hostfxr_path_size = sizeof(hostfxr_path) / sizeof(char_t);

	if (get_hostfxr_path(hostfxr_path, &hostfxr_path_size, NULL)) {
		switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_ERROR, "Unable to locate Core HostFXR\n");
		return SWITCH_FALSE;
	}

	// Load hostfxr and get desired exports
	void *lib = load_library(hostfxr_path);
	hostfxr_initialize_for_runtime_config_fn hostfxr_initialize_for_runtime_config_fptr =
		(hostfxr_initialize_for_runtime_config_fn)get_export(lib, "hostfxr_initialize_for_runtime_config");
	hostfxr_get_runtime_delegate_fn hostfxr_get_runtime_delegate_fptr =
		(hostfxr_get_runtime_delegate_fn)get_export(lib, "hostfxr_get_runtime_delegate");
	hostfxr_close_fn hostfxr_close_fptr =
		(hostfxr_close_fn)get_export(lib, "hostfxr_close");

	if (!hostfxr_initialize_for_runtime_config_fptr || !hostfxr_get_runtime_delegate_fptr || !hostfxr_close_fptr) {
		switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_ERROR, "Unable to get Core HostFXR exports: %s\n", hostfxr_path);
		return SWITCH_FALSE;
	}

	switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_INFO, "Loaded Core HostFXR: %s\n", hostfxr_path);

	hostfxr_handle handle = NULL;
	if (hostfxr_initialize_for_runtime_config_fptr(LOADER_RUNTIME_CONFIG_PATH, NULL, &handle) || !handle)
	{
		switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_ERROR, "Unable to initialize handle\n");
		hostfxr_close_fptr(handle);
		return SWITCH_FALSE;
	}

	load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer;

	if (hostfxr_get_runtime_delegate_fptr(handle, hdt_load_assembly_and_get_function_pointer, (void**)&load_assembly_and_get_function_pointer) || !load_assembly_and_get_function_pointer) {
		switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_ERROR, "Unable to get runtime delegate\n");
		hostfxr_close_fptr(handle);
		return SWITCH_FALSE;
	}

	hostfxr_close_fptr(handle);

	switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_INFO, "Initialized Core HostFXR\n");

    loader_entry_fn load = NULL;
    if (load_assembly_and_get_function_pointer(LOADER_PATH, "FreeSWITCH.Loader, Loader", "Load", "FreeSWITCH.Loader+LoadDelegate, Loader", NULL, (void**)&load) ||
		!load) {
		switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_ERROR, "Unable to load loader assembly and get loader entry function pointer\n");
		return SWITCH_FALSE;
	}

    *callbacks = load();

	switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_INFO, "Loaded Core HostFXR Loader: %s\n", LOADER_PATH);

    if (callbacks->ontest) {
		switch_log_printf(SWITCH_CHANNEL_LOG, SWITCH_LOG_INFO, "Uber success: %d\n", callbacks->ontest());
	}

	return SWITCH_TRUE;
}

SWITCH_MODULE_LOAD_FUNCTION(mod_coreclr_load)
{
	interface_callbacks_t interface_callbacks;
	switch_api_interface_t *api_interface;

	/* connect my internal structure to the blank pointer passed to me */
	*module_interface = switch_loadable_module_create_module_interface(pool, modname);

	if (!load_runtime(&interface_callbacks)) {
		return SWITCH_STATUS_FALSE;
	}

	//	if (interface_callbacks.api) {
	//	SWITCH_ADD_API(api_interface, "coreclr", "Run a coreclr api", coreclr_api_function, "<api> [<args>]");
	//}
	
	/* indicate that the module should continue to be loaded */
	return SWITCH_STATUS_NOUNLOAD;
}

SWITCH_MODULE_SHUTDOWN_FUNCTION(mod_coreclr_shutdown)
{
	// TODO: unload hostfxr, so a new one could be loaded?
	return SWITCH_STATUS_UNLOAD;
}

/* For Emacs:
 * Local Variables:
 * mode:c
 * indent-tabs-mode:t
 * tab-width:4
 * c-basic-offset:4
 * End:
 * For VIM:
 * vim:set softtabstop=4 shiftwidth=4 tabstop=4 noet:
 */
