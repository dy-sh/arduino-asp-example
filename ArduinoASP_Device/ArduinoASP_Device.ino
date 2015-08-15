/*
CONNECTION:

ETHERNET SHIELD:
10 (SPI SELECT),
11,12,13 (SPI),
4(SD Card)

DTH11:
S > 2
MID > +5
- > GND

IR Reciever
S > 5
MID > +5
- > GND

IR Transmitter
S > 3
MID > NONE
- > GND
*/



//ETHERNET

#include <SPI.h>
#include <Ethernet.h>
byte mac[] = { 0x90, 0xA2, 0xDA, 0x0D, 0x48, 0xD3 };
IPAddress ip(192, 168, 1, 20);
IPAddress gateway(192, 168, 1, 1);
IPAddress subnet(255, 255, 255, 0);
EthernetServer server(80);


#include "DHTSensor.h"
DHTSensor *dhtSensor;
#define DHTSensor_PIN 2

#include "IRSensor.h"
IRSensor *irSensor;
#define IRSensor_PIN 5


//VARIABLES
#define MAX_REQUEST_SIZE 64
char request[MAX_REQUEST_SIZE] = "";
char param[MAX_REQUEST_SIZE] = "";

unsigned int totalRequests = 0;




void setup()   {

	Serial.begin(57600);

	dhtSensor = new DHTSensor(DHTSensor_PIN);
	irSensor = new IRSensor(IRSensor_PIN);



	Ethernet.begin(mac, ip, gateway, subnet);
	server.begin();

	Serial.print("IP: ");
	Serial.println(Ethernet.localIP());
	Serial.println();

	dhtSensor->ReadTemperatureSensor();
}



void loop() {
	dhtSensor->Loop();
	irSensor->Loop();
	ProcessEthernet();

}






void RenderPageHeader(EthernetClient* client)
{
	client->println("HTTP/1.1 200 OK");
	client->println("Content-Type: text/html");
	client->println("Connnection: close");
	client->println();
}



void RenderIndexPage(EthernetClient* client)
{
	RenderPageHeader(client);

	client->println("<HTML><HEAD>");
	//	client->println("<meta http-equiv=\"refresh\" content=\"10\">");
	//	client->println("<link rel=\"icon\" href=\"data:;base64,=\">");
	client->println("</HEAD><BODY><center>");
	client->println("<H1>Temperature</H1><hr/>");

	client->print("Temp: ");
	client->print(dhtSensor->temperature);
	client->println(" C <br/>");

	client->print("Hum: ");
	client->print(dhtSensor->humidity);
	client->println(" % <br/>");

	client->print("Req: ");
	client->print(totalRequests);
	//	client->println("<br/><br/>");

	//	client->println("<a href=\"/sensor1\"\">Sensor 1</a> | ");

	client->println("</BODY></HTML>");
}


void RenderErrorPage(EthernetClient* client)
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


void RenderServerSatusJSON(EthernetClient* client)
{
	RenderPageHeader(client);

	client->print("{ \"");
	client->print("timeOnline");
	client->print("\" : \"");
	client->print(millis() / 1000);
	client->print("\" ");

	client->print(", \"");
	client->print("totalRequests");
	client->print("\" : \"");
	client->print(totalRequests);
	client->print("\" }");
}


void ProcessEthernet()
{

	EthernetClient client = server.available();
	if (!client)
		return;

	Serial.println("Client connected");

	ReadRequest(&client);
	Serial.print("REQEST: ");
	Serial.println(request);
	ParseRequest(request);
	ExecuteRequest(&client, request);

	// даем браузеру подгрузить данные
	delay(1);
	// закрываем соединение
	client.stop();


	Serial.println("Client disonnected");

}



char* ReadRequest(EthernetClient* client)
{
	//очищаем строку
	request[0] = '\0';

	// зациклено пока юзер подключен
	while (client->connected())
	{
		while (client->available())
		{
			char c = client->read();

			// выходим из лупа, если втретили конец строки
			if ('\n' == c)
				return request;

			if (strlen(request) == sizeof(request)){
				//переполнение буфера MAX_REQUEST_SIZE
				request[0] = '\0';
				return request;
			}

			//добавляем символ к строке
			strncat(request, &c, 1);
		}
	}
	return request;
}



void ExecuteRequest(EthernetClient* client, char* request)
{

	totalRequests++;

	if (strcmp(request, "/") == 0){
		RenderIndexPage(client);
		return;
	}

	ParseNextParam(request, param);

	if (strcmp(param, "status") == 0)
		RenderServerSatusJSON(client);

	else if (strcmp(param, "temp") == 0) //сравниваем строку
		dhtSensor->RenderJSON(client);

	else if (strcmp(param, "ir") == 0){
		irSensor->ExecuteRequest(client, request, param);
	}

	else
	{
		RenderErrorPage(client);
	}
}




// метод читает GET запрос типа  "GET /get/13/ HTTP/1.1" 
// и выделяет из него полезные данные: /get/13/
// результат сохраняется обратно в request
// если запрос не был прочитан то в request в первый байт помещяется знак окончания строки '\0'
void ParseRequest(char* request)
{

	//если не GET запрос, то не обрабатываем
	if (!strncmp(request, "GET ", 4) == 0) {
		request[0] = '\0';
		return;
	}

	//находим начало и конец запроса
	char* startChar = &request[4]; //strstr(request, "GET ") + 4
	char* endChar = strstr(request, " HTTP/");

	//если не найден конец запроса ( HTTP/) или отсутсвует тело запроса, то выходим
	if (endChar == NULL) {
		request[0] = '\0';
		return;
	}

	memcpy(request, startChar, endChar - startChar);
	request[endChar - startChar] = '\0';
}


// метод парсит запрос, выделяя из него первый параметр
// например запрос  "get/13/15/" 
// в param помещается get
// в request обратно помещается 13/15/
void ParseNextParam(char* request, char* param)
{
	param[0] = '\0';//иначе в param1 может остаться старое значение, если в него нечего будет писать
	int reqWas = strlen(request);
	sscanf(request, "/%[^'/']%s", param, request);
	//если в функции sscanf нечего записывать в request то она его оставляет нетронутым
	//а нам нужно чтобы он стирался
	if (strlen(request) == reqWas) request[0] = '\0';
}








