using System;
using System.IO;
using BPS_Sharp;
using System.Text;
using System.Threading.Tasks;

namespace Tester_1
{
    class Program
    {
        static void Main(string[] args)
        {
            string rom = "";
            string patch = "";
            string path = "";

            Console.WriteLine("BPS-Sharp (Just patcher.) - Ported from Alcaro's \"CPS\" JS-based BPS patcher." +
                "\nAsynch is the future, somebody said in a Discord guild. They weren't wrong." +
                "\n-GlitchyPSI");

            do
            {
                Console.WriteLine("Insert the path to the base ROM file.");
                rom = Console.ReadLine();
            }
            while (!File.Exists(rom));
            do
            {
                Console.WriteLine("Insert the path to the patch for this ROM file.");
                patch = Console.ReadLine();
            }
            while (!File.Exists(patch));
            Console.WriteLine("Insert the path where the patched ROM is going to be saved to.");
            path = Console.ReadLine();
            if (File.Exists(path))
            {
                Console.WriteLine("Oh, hold on... this file already exists, and will be overwritten.");
                Console.ReadKey(true);
                }
            TryPatch(rom, patch, path).Wait();
        }

        static async Task TryPatch(string rom, string patch, string path)
        {
            try
            {
                await BPS.TryBPS(rom, patch, path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.ToString());
            }
            finally
            {
                Console.WriteLine("We're done here.");
                Console.ReadKey();
            }
        }
        }
    }
