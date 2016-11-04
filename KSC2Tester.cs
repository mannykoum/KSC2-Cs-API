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
            string sn = k.SN;
            // Keep the console window open in debug mode.
            Console.WriteLine("Port {0}", port);
            Console.WriteLine("Serial num {0}", sn);
            //Console.WriteLine(k.set("coupling", "ac"));
            //Console.WriteLine(k.get(1, "coupling"));            
            //Console.WriteLine(k.get(2, "coupling"));
            //Console.WriteLine(k.set("coupling", "dc"));
            //Console.WriteLine(k.get(1, "coupling"));            
            //Console.WriteLine(k.get(2, "coupling"));
            //Console.WriteLine(k.get(2, "Shield"));

            k.configure(2, "ac", "Ground", "operate");
            k.printAll();
            // Console.ReadKey();
        }
    }
}