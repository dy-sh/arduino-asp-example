#include "DHTSensor.h"



DHTSensor::DHTSensor(byte pin)
{
	dht = new DHT(pin, DHT11);
	dht->begin();
}



void DHTSensor::Loop()
{
	if (millis() - readSensorTimer < DHT_REQUEST_DELAY) return;

	readSensorTimer = millis();

	ReadTemperatureSensor();

}

void DHTSensor::ReadTemperatureSensor(){
	// Reading temperature or humidity takes about 250 milliseconds!
	// Sensor readings may also be up to 2 seconds 'old' (its a very slow sensor)
	humidity = dht->readHumidity();
	temperature = dht->readTemperature();

	// Check if any reads failed and exit early (to try again).
	if (isnan(humidity) || isnan(temperature)) {
		Serial.println("DHT faild!");
		return;
	}

	Serial.print("Temp: ");
	Serial.print(temperature);
	Serial.println(" C ");
}



void DHTSensor::RenderJSON(EthernetClient* client)
{
	RenderPageHeader(client);

	client->print("{ \"");
	client->print("temperature");
	client->print("\" : \"");
	client->print(temperature);
	client->print("\" ");

	client->print(", \"");
	client->print("humidity");
	client->print("\" : \"");
	client->print(humidity);
	client->print("\" }");
}


void DHTSensor::RenderPageHeader(EthernetClient* client)
{
	client->println("HTTP/1.1 200 OK");
	client->println("Content-Type: text/html");
	client->println("Connnection: close");
	client->println();
}
