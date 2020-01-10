#include <switch.h>
#include <switch_cpp.h>
#include "ManagedSession.h"

ManagedSession::ManagedSession() : CoreSession() { }

ManagedSession::ManagedSession(char *uuid) : CoreSession(uuid) { }

ManagedSession::ManagedSession(switch_core_session_t *session) : CoreSession(session) { }

ManagedSession::~ManagedSession()
{
	// TODO: review this
	// Do auto-hangup ourselves because CoreSession can't call check_hangup_hook
	// after ManagedSession destruction (cause at point it's pure virtual)
	if (session) {
		if (switch_test_flag(this, S_HUP) && !switch_channel_test_flag(channel, CF_TRANSFER)) {
			switch_channel_hangup(channel, SWITCH_CAUSE_NORMAL_CLEARING);
			setAutoHangup(0);
		}
		// Don't let any callbacks use this CoreSession anymore
		switch_channel_set_private(channel, "CoreSession", NULL);
	}
}

bool ManagedSession::begin_allow_threads()
{
	return true;
}

bool ManagedSession::end_allow_threads()
{
	return true;
}

void ManagedSession::check_hangup_hook()
{
}

switch_status_t ManagedSession::run_dtmf_callback(void *input, switch_input_type_t itype)
{
	return SWITCH_STATUS_SUCCESS;
}
