#ifndef _MOD_CORECLR_H_
#define _MOD_CORECLR_H_

typedef int32_t (*test_callback_t)();

typedef struct interface_callbacks
{
	test_callback_t ontest;
} interface_callbacks_t;

typedef interface_callbacks_t (*loader_entry_fn)();

typedef int (*loader_test_fn)();

#endif
