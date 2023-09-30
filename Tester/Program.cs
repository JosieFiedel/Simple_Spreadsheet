using System.Net;
using System.Net.Sockets;

namespace Tester;

public class test
{
    public static void Main(string[] args)
    {
        int Fn = 65537;
        int result = 3;
        int pow = 1;

        while (result != -1)
        {
            Console.WriteLine("3^" + pow + " = " + result);
            result = (result * result) % Fn;
            pow *= 2;
        }        
    }
}