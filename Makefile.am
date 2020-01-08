AUTOMAKE_OPTIONS = foreign subdir-objects

mod_LTLIBRARIES = mod_coreclr.la
mod_coreclr_la_SOURCES  = mod_coreclr.c
mod_coreclr_la_CFLAGS   = $(FREESWITCH_CFLAGS) -Wall
mod_coreclr_la_LDFLAGS  = -avoid-version -module -no-undefined -shared $(FREESWITCH_LDFLAGS)
