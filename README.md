# KSC-2 C# API #
## KSC2.dll and Tester ##

----------
### To compile ###
In cmd.exe:

> C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:library \out:KSC2.dll KSC2.cs
>
> C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe /t:exe /reference:KSC2.dll /out:KSC2Tester.exe KSC2Tester.cs

### To run ###
In cmd.exe:

>./KSC2Tester.exe