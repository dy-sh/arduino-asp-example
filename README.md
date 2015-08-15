This is an example of using the Arduino and ASP.NET.<br>
You can use this project as the core of your Home automation / Internet of things.<br>
<br>
The Arduino connects to the network using an Ethernet Shield. <br>
ASP.NET project runs on a windows (IIS). <br>
This repository contains two projects.<br>
<br>
<b>IRControl</b> allows you to control any devices from the Internet. <br>
This requires the Arduino to connect the IR Reciever and IR Transmitter.  <br>
The Arduino can memorize any code of remote control in the learning mode.  <br>
<br>
<b>Temperature</b> refers to the Arduino every minute, requests the temperature and humidity from DTH11 sensor and stores the data in the database. <br>
Then you can see a nice graph of temperature and humidity. <br>
<br>
You can use two of these projects simultaneously with a single Arduino Board.<br>
<br>
Connect the IR Reciever to pin 5, IR Transmitter to pin 3, DTH11 to pin 2. Ethernet shield uses pins 10, 11,12,13.<br>
<br>
In ArduinoASP_Device.ino file you need to configure the ip address of the Arduino:<br>
IPAddress ip(192, 168, 1, 20);<br>
IPAddress gateway(192, 168, 1, 1);<br>
<br>
Then this IP address must be specified in the file ArduinoIRControl.cs and ArduinoTemperatureSensor.cs:<br>
private string arduinoAddress = "http://192.168.1.20";<br>
<br>
In Web.config files you need to specify the connection string to the SQL server:<br>
add name="DbConnection" ... User ID=username;Password=password"<br>
<br>
I am using hangfire to background task worked without user activity. <br>
You need to configure IIS, otherwise the web application will be unloaded from memory spontaneously. <br>
Instructions here: http://docs.hangfire.io/en/latest/deployment-to-production/making-aspnet-app-always-running.html
