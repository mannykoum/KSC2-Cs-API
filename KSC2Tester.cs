using System;
using System.IO.Ports;
using KSC2;

namespace KSCTester
{
    class KSCTester 
    {
        static void Main() 
        {
            Console.WriteLine("Hello World!");

            KSC2.KSC2 k = new KSC2.KSC2();
            string port = k.COM;
            // Keep the console window open in debug mode.
            Console.WriteLine("Port {0}", port);
            // Console.ReadKey();
        }
    }
}