#pragma once

#define RAW_LENGTH 32

#include "IRremote.h"
#include <Ethernet.h>

class IRSensor
{
public:
	IRSensor(byte pin);
	void Loop();
	bool ExecuteRequest(EthernetClient* client, char* request, char* param);
private:
	IRrecv *ir_recv;
	IRsend ir_send;
	decode_results ir_code;
	bool waitForIRCode = false;
	bool irCodeReceived = false;
	bool irCodeStored = false;
	int codeType = -1; // The type of code
	unsigned long codeValue; // The code value if not raw
	unsigned int rawCodes[RAW_LENGTH]; // The durations if raw
	byte codeLen; // The length of the code
	byte toggle = 0; // The RC5/6 toggle state
	unsigned long lastIrRecieveTime;
	void StoreCode();
	void RenderPageHeader(EthernetClient* client);
	void ParseNextParam(char* request, char* param);
	void RenderErrorPage(EthernetClient* client, char* param);
	void ParseIrCode(char* request, char* param);
	void ClearIrSample();
	void StoreNewIRCode();
	void SendCode(bool repeat);
	void RenderIrInfoJSON(EthernetClient* client);

};

