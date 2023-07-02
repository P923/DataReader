
# Data Reader

[![#](https://img.shields.io/badge/-.NET%206.0-blueviolet)]()




A simple GUI for reading data from OPC Server, ModBus, AB Contrologix 5000 or XML, directly from the terminal.  <br /> 
There is also a Worker Service that writes the required data (initialized by the GUI) to a local QuestDB. <br />
![Image](./DataReader/image.PNG)

# OPC UA
For the OPC UA Servers you can insert Username:Password in the Path field. <br />
Remember to trust the certificate or you will receive the error 
```
Object reference not set an instance of object.<e>
```
For address of the tag, you need to insert the full path, for example <br />
```
ns=2;s=Channel1.Device1.Tag1
```



## Libraries 

* https://github.com/OPCFoundation/UA-.NETStandard
* https://github.com/zhaopeiym/IoTClient
* https://github.com/libplctag/libplctag.NET
* https://github.com/gui-cs/Terminal.Gui

## Authors

[@P923](https://www.github.com/P923)

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg)](https://choosealicense.com/licenses/mit/)


