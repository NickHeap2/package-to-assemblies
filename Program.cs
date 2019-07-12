using System;

namespace package_to_assemblies
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Invalid parameters!");
                return 1;
            }
            string packageFile = args[0];
            string packagesDir = args[1];
            string assembliesDir = args[2];

            try
            {
                PackageToAssemblies packageToAssemblies = new PackageToAssemblies(packageFile, packagesDir);
                packageToAssemblies.CopyToAssemblies(assembliesDir);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }

            Console.Out.WriteLine("Copies complete.");
            return 0;
        }
    }
}
