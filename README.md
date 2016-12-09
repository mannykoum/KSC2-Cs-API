# KSC-2 C# API #
## KSC2.dll and Tester ##
### Emmanuel Koumandakis ###

----------
### To compile the dll and the Tester ###
In cmd.exe:

	C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:library \out:KSC2.dll KSC2.cs
	C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /reference:KSC2.dll /out:KSC2Tester.exe KSC2Tester.cs

### To run the Tester ###
In cmd.exe:

	./KSC2Tester.exe

----------
### API ###

This dll provides an object-oriented interface for the Kulite Signal 
Conditioner (KSC-2). 

#### Instantiation ####

Creating a new KSC2 object is as easy as connecting the KSC2 via USB, 
including the library in your code

	using KSC2;

and typing

	KSC2.KSC2 k = new KSC2.KSC2();

where k is the variable name of our new object.

This constructor will pick the first COM port in use. If you would rather
specify the COM port your KSC2 is using you can use

	KSC2.KSC2 k = new KSC2.KSC2("COM0");

where COM0 is the desired port.

#### Methods ####

The interface is divided in two types of methods: Mutator (setter) and 
Accessor (getter) methods.

At the lower level, settings can be accessed directly using set/get.

	public bool set(int channel, string cmd, string param)
	public string get(int channel, string cmd)

set returns true when successful and false when there is an error.
cmd needs to be a valid command. channel needs to be either 1 or 2.
If channel is not passed as a parameter the method will apply to both channels.
Both methods are case-insensitive when it comes to cmd and param.

Valid commands:

**"SN", "COUPLING", "SHIELD", "MODE", "FILTER", "FC", "POSTGAIN", "PREGAIN",
"EXC", "EXCTYPE", "SENSE", "COMPFILT", "COMPFILTFC", "COMPFILTQ", "INOVLD",
"OUTOVLD", "OUTOVLDLIM"**


However, higher level methods are provided covering most use cases effectively.
All of the following methods can be run on both channels if int channel is not
specified

    public void configure(int channel, string coupling, string shield, string mode)
sets the coupling, shield mode, and operation mode for a channel

	public void filter(int channel, int freq_cut, string type)
sets the cutoff frequency and type of the filter on a channel

	public void excitation(int channel, double voltage, string type, string sense)
