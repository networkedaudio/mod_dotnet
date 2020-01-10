%module Native

%{
#include "switch.h"
#include "switch_cpp.h"
%}

%typemap(csclassmodifiers) ManagedSession "public partial class"
%typemap(csclassmodifiers) Event "public partial class"
%typemap(csclassmodifiers) Stream "public partial class"
%newobject EventConsumer::pop;
%newobject Session;
%newobject CoreSession;
%newobject Event;
%newobject Stream;
%newobject API::execute;
%newobject API::executeString;
%newobject CoreSession::playAndDetectSpeech;

// These methods need a bit of wrapping help
%csmethodmodifiers CoreSession::originate "protected";

// Rename some things to make them more .NET-like
%rename (Answer) CoreSession::answer;
%rename (Hangup) CoreSession::hangup;
%rename (Ready) CoreSession::ready;
%rename (Transfer) CoreSession::transfer;
%rename (SetVariable) CoreSession::setVariable;
%rename (GetVariable) CoreSession::getVariable;
%rename (SetPrivate) CoreSession::setPrivate;
%rename (GetPrivate) CoreSession::getPrivate;
%rename (Say) CoreSession::say;
%rename (SayPhrase) CoreSession::sayPhrase;
%rename (RecordFile) CoreSession::recordFile;
%rename (SetCallerData) CoreSession::setCallerData;
%rename (CollectDigits) CoreSession::collectDigits;
%rename (GetDigits) CoreSession::getDigits;
%rename (PlayAndGetDigits) CoreSession::playAndGetDigits;
%rename (StreamFile) CoreSession::streamFile;
%rename (Execute) CoreSession::execute;
%rename (GetUuid) CoreSession::get_uuid;
%rename (HookState) CoreSession::hook_state;
%rename (InternalSession) CoreSession::session;
%rename (Speak) CoreSession::speak;
%rename (SetTtsParameters) CoreSession::set_tts_parms;
%rename (SetAutoHangup) CoreSession::setAutoHangup;

%rename (Serialize) Event::serialize;
%rename (SetPriority) Event::setPriority;
%rename (GetHeader) Event::getHeader;
%rename (GetBody) Event::getBody;
%rename (GetEventType) Event::getType;
%rename (AddBody) Event::addBody;
%rename (AddHeader) Event::addHeader;
%rename (DeleteHeader) Event::delHeader;
%rename (Fire) Event::fire;
%rename (InternalEvent) Event::event;

%rename (Write) Stream::write;
%rename (GetData) Stream::getData;

%rename (Api) API;
%rename (Execute) API::execute;
%rename (ExecuteString) API::executeString;

%rename (IvrMenu) IVRMenu;
%rename (Execute) IVRMenu::execute;
%rename (ExecuteString) API::executeString;

#define SWITCH_DECLARE(type) type
#define SWITCH_DECLARE_NONSTD(type) type
#define SWITCH_MOD_DECLARE(type) type
#define SWITCH_MOD_DECLARE_NONSTD(type) type
#define SWITCH_DECLARE_DATA
#define SWITCH_MOD_DECLARE_DATA
#define SWITCH_THREAD_FUNC
#define SWITCH_DECLARE_CONSTRUCTOR SWITCH_DECLARE_DATA

#define _In_
#define _In_z_
#define _In_opt_z_
#define _In_opt_
#define _Printf_format_string_
#define _Ret_opt_z_
#define _Ret_z_
#define _Out_opt_
#define _Out_
#define _Check_return_
#define _Inout_
#define _Inout_opt_
#define _In_bytecount_(x)
#define _Out_opt_bytecapcount_(x)
#define _Out_bytecapcount_(x)
#define _Ret_
#define _Post_z_
#define _Out_cap_(x)
#define _Out_z_cap_(x)
#define _Out_ptrdiff_cap_(x)
#define _Out_opt_ptrdiff_cap_(x)
#define _Post_count_(x)


%include switch_cpp.h
