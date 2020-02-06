using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace rtz
{
    class Program
    {
        static int Usage()
        {
            Console.WriteLine("RTZ -zip rtz-filename [rtzp filename]");
            Console.WriteLine("\tMakes an RTZP container with the RTZ file and attachments");
            Console.WriteLine();
            Console.WriteLine("RTZ -unzip rtzp-filename [folder]");
            Console.WriteLine("\tExtracts contents of an RTZP to a given folder or defaults to filename of RTZP as folder name");
            Console.WriteLine();
            Console.WriteLine("rtz -check <rtz or rtzp filename>");
            Console.WriteLine("\tComing soon");
            return 1;
        }

        // Zero return means success
        static int Main(string[] args)
        {
            try
            {
                return SafeMain(args);
            }
            catch (Exception x)
            {
                Console.WriteLine(x.ToString());
                return 666;
            }
        }

        static int SafeMain(string[] args)
        {
            if (args.Length < 2) return Usage();
            string command = args[0];
            string target = args[1]; // 2nd param always a file

            if (!File.Exists(target))
            {
                Console.WriteLine($"File not found {target}");
                Console.WriteLine();
                return Usage();
            }

            string destination = args.Length >= 3 ? args[2] : string.Empty;

            if (command.Equals("-zip", StringComparison.InvariantCultureIgnoreCase))
            {
                Zipper.Zip(target, destination);
            }
            else if (command.Equals("-unzip", StringComparison.InvariantCultureIgnoreCase))
            {
                UnzipCommand(target, destination);
            }
            else
            {
                return Usage();
            }

            return 0;
        }

        private static void UnzipCommand(string target, string destination)
        {
            if (string.IsNullOrWhiteSpace(destination))
            {
                // Destination will be a folder same name as RTZP file
                var path = Path.GetDirectoryName(target);
                destination = Path.Combine(path, Path.GetFileNameWithoutExtension(target));
                if (Directory.Exists(destination))
                    Directory.Delete(destination, true);
            }

            Directory.CreateDirectory(destination);

            Console.WriteLine($"Extracting {target}");
            Console.WriteLine($"To destination {destination}");

            ZipFile.ExtractToDirectory(target, destination);

            Console.WriteLine("Successful");
        }
    }
}
