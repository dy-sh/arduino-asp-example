#include "IRSensor.h"


/*
‘ункционал с кодами RAW € не тестировал, его нужно доделать

¬ключить режим обучени€ с пульта (ожидани€ кода): ir/read
ќтменить режим обучени€ с пульта (ожидани€ кода): ir/clear
ѕослать код (тип 2, длина 30, код 1634545): ir/send/2/30/1634545

ѕо команде ir/info мы получаем JSON со следующими данными
irCodeStored - получен новый код в режиме обучени€
waitForIRCode - ожидание кода в режиме обучени€
type - тип кода
length - длина
raw - значение RAW
value - код
*/




IRSensor::IRSensor(byte pin)
{
	ir_recv = new IRrecv(pin);
	ir_recv->enableIRIn();
}




void IRSensor::Loop()
{
	if (ir_recv->decode(&ir_code)) {
		Serial.print("Received IR. Type: ");
		Serial.print(ir_code.decode_type);
		Serial.print(", len: ");
		Serial.print(ir_code.rawlen);
		Serial.print(", code: ");
		Serial.println(ir_code.value, DEC);
		ir_recv->resume(); // Receive the next value
		lastIrRecieveTime = millis();
		if (waitForIRCode) {
			StoreCode();
			irCodeReceived = true;
			irCodeStored = false;
		}
	}

	//store code
	if (irCodeReceived && millis() - lastIrRecieveTime > 500){

		waitForIRCode = false;
		irCodeReceived = false;
		irCodeStored = true;
	}
}







void IRSensor::StoreCode() {
	codeType = ir_code.decode_type;

	if (codeType == NEC && ir_code.value == REPEAT)
		// Don't record a NEC repeat value as that's useless.
		return;

	Serial.println("IR Stored");


	if (codeType == UNKNOWN) {
		codeLen = ir_code.rawlen - 1;
		// To store raw codes:
		// Drop first value (gap)
		// Convert from ticks to microseconds
		// Tweak marks shorter, and spaces longer to cancel out IR receiver distortion
		for (int i = 1; i <= codeLen; i++) {
			if (i % 2) {
				// Mark
				rawCodes[i - 1] = ir_code.rawbuf[i] * USECPERTICK - MARK_EXCESS;
			}
			else {
				// Space
				rawCodes[i - 1] = ir_code.rawbuf[i] * USECPERTICK + MARK_EXCESS;
			}
		}
	}
	else {
		Serial.println(ir_code.value, DEC);
		codeValue = ir_code.value;
		codeLen = ir_code.bits;
	}
}





void IRSensor::SendCode(bool repeat) {

	Serial.print("Send IR. Type: ");
	Serial.print(codeType);
	Serial.print(", len: ");
	Serial.print(codeLen);
	Serial.print(", code: ");
	Serial.println(codeValue, DEC);

	if (codeType == NEC) {
		if (repeat) {
			ir_send.sendNEC(REPEAT, codeLen);
		}
		else {
			ir_send.sendNEC(codeValue, codeLen);
		}
	}
	else if (codeType == SONY) {
		ir_send.sendSony(codeValue, codeLen);
	}
	else if (codeType == RC5 || codeType == RC6) {
		if (!repeat) {
			// Flip the toggle bit for a new button press
			toggle = 1 - toggle;
		}
		// Put the toggle bit into the code to send
		codeValue = codeValue & ~(1 << (codeLen - 1));
		codeValue = codeValue | (toggle << (codeLen - 1));
		if (codeType == RC5) {
			ir_send.sendRC5(codeValue, codeLen);
		}
		else {
			ir_send.sendRC6(codeValue, codeLen);
		}
	}
	else if (codeType == UNKNOWN) {// i.e. raw
		// Assume 38 KHz
		ir_send.sendRaw(rawCodes, codeLen, 38);
	}
}











void IRSensor::RenderIrInfoJSON(EthernetClient* client)
{
	RenderPageHeader(client);

	client->print("{ \"");
	client->print("irCodeStored");
	client->print("\" : \"");
	if (irCodeStored)
		client->print("true");
	else
		client->print("false");
	client->print("\" ");

	client->print(", \"");
	client->print("waitForIRCode");
	client->print("\" : \"");
	if (waitForIRCode)
		client->print("true");
	else
		client->print("false");
	client->print("\" ");

	if (irCodeStored){

		client->print(", \"");
		client->print("type");
		client->print("\" : \"");
		client->print(codeType);
		client->print("\" ");

		client->print(", \"");
		client->print("length");
		client->print("\" : \"");
		client->print(codeLen);
		client->print("\" ");


		if (codeType == UNKNOWN){

			client->print(", \"");
			client->print("raw");
			client->print("\" : \"");
			client->print(*rawCodes);
			client->print("\" ");


		}
		else
		{
			client->print(", \"");
			client->print("value");
			client->print("\" : \"");
			client->print(codeValue, DEC);
			client->print("\" ");
		}

	}
	client->print("}");

}





void IRSensor::StoreNewIRCode()
{
	ClearIrSample();
	waitForIRCode = true;
	ir_recv->enableIRIn();//если не перезапустить прием то он повторно не всегда стартует
}

void IRSensor::ClearIrSample()
{
	irCodeStored = false;
	irCodeReceived = false;
	waitForIRCode = false;

	codeValue = 0;
	codeLen = 0;
	codeType = 0;
}

void IRSensor::ParseIrCode(char* request, char* param)
{
	//2/30/90 > type=2,len=30,value=90
	//0/20/AAABBB > type=-1,len=20,raw=0AAABBB

	ParseNextParam(request, param);
	sscanf(param, "%d", &codeType);

	ParseNextParam(request, param);
	sscanf(param, "%d", &codeLen);

	ParseNextParam(request, param);

	if (codeType != -1){
		codeValue = atol(param);
	}
	else
	{
		//strcpy(rawCodes, param);
	}

	//codeType

	if (codeType == UNKNOWN) {
		//codeLen =

		for (int i = 1; i <= codeLen; i++) {
			//rawCodes[i] =
		}
	}
	else {
		//codeValue = ir_code.value;
		//codeLen = ir_code.bits;
	}
}


void IRSensor::RenderPageHeader(EthernetClient* client)
{
	client->println("HTTP/1.1 200 OK");
	client->println("Content-Type: text/html");
	client->println("Connnection: close");
	client->println();
}

// метод парсит запрос, выдел€€ из него первый параметр
// например запрос  "get/13/15/" 
// в param помещаетс€ get
// в request обратно помещаетс€ 13/15/
void IRSensor::ParseNextParam(char* request, char* param)
{
	param[0] = '\0';//иначе в param1 может остатьс€ старое значение, если в него нечего будет писать
	int reqWas = strlen(request);
	sscanf(request, "/%[^'/']%s", param, request);
	//если в функции sscanf нечего записывать в request то она его оставл€ет нетронутым
	//а нам нужно чтобы он стиралс€
	if (strlen(request) == reqWas) request[0] = '\0';
}




bool IRSensor::ExecuteRequest(EthernetClient* client, char* request, char* param)
{

	ParseNextParam(request, param);

	if (strcmp(param, "info") == 0){
		RenderIrInfoJSON(client);
	}

	else if (strcmp(param, "clear") == 0){
		ClearIrSample();
		RenderIrInfoJSON(client);
	}

	else if (strcmp(param, "read") == 0)
	{
		StoreNewIRCode();
		RenderIrInfoJSON(client);
	}

	else if (strcmp(param, "send") == 0)
	{
		ParseIrCode(request, param);
		SendCode(false);
		RenderIrInfoJSON(client);
	}
	else
	{
		RenderErrorPage(client, param);
	}
}

void  IRSensor::RenderErrorPage(EthernetClient* client, char* param)
{
	Serial.print("UNKNOWN PARAM: '");
	Serial.print(param);
	Serial.println("'");

	client->println("HTTP/1.1 404 Not Found");
	client->println("Content-Type: text/html");
	client->println("Connnection: close");
	client->println();
	client->println("<HTML><HEAD>");
	client->println("</HEAD><BODY><center>");
	client->println("<H1>Page Not Found</H1>");
	client->println("</BODY></HTML>");

}