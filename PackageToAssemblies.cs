using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace package_to_assemblies
{
    internal class PackageToAssemblies
    {
        private string packageFile;
        private string packagesDir;
        private string assembliesDir;

        public PackageToAssemblies(string packageFile, string packagesDir)
        {
            this.packageFile = packageFile;
            this.packagesDir = packagesDir;

            if (!File.Exists(packageFile))
            {
                throw new ArgumentException($"Error: File {packageFile} does not exist!");
            }
            if (!Directory.Exists(packagesDir))
            {
                throw new ArgumentException($"Error: Directory {packagesDir} does not exist!");
            }
        }

        internal void CopyToAssemblies(string assembliesDir)
        {
            this.assembliesDir = assembliesDir;

            if (!Directory.Exists(assembliesDir))
            {
                try
                {
                    Directory.CreateDirectory(assembliesDir);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error: Couldn't create missing directory {assembliesDir} ({ex.Message})!");
                }
            }
            if (!Directory.Exists(assembliesDir))
            {
                throw new ArgumentException($"Error: Directory {assembliesDir} does not exist!");
            }

            //parse the packages file
            Packages packages = GetPackages();

            //for each package copy the files
            foreach (Package package in packages.Items)
            {
                Console.Out.WriteLine($"Processing package {package.id} {package.version}...");
                ProcessPackage(package);
            }
        }

        private void ProcessPackage(Package package)
        {
            string packageDirName = $"{package.id}.{package.version}";

            string thisPackageDir = Path.Combine(packagesDir, packageDirName);

            if (!Directory.Exists(thisPackageDir))
            {
                throw new ArgumentException($"Error: Directory {thisPackageDir} does not exist!");
            }

            string packageLibDir = Path.Combine(thisPackageDir, "lib");
            if (Directory.Exists(packageLibDir))
            {
                string packagePlatformDir = Path.Combine(packageLibDir, package.targetFramework);
                if (!Directory.Exists(packagePlatformDir))
                {
                    throw new ArgumentException($"Error: Directory {packagePlatformDir} does not exist!");
                }

                // this files means skip
                if (File.Exists(Path.Combine(packagePlatformDir, "_._")))
                {
                    return;
                }

                string[] dirFiles = Directory.GetFiles(packagePlatformDir, "*");
                foreach (var sourceFile in dirFiles)
                {
                    string destinationFile = Path.Combine(assembliesDir, Path.GetFileName(sourceFile));
                    CopyIfDifferent(sourceFile, destinationFile);
                    //Console.WriteLine(file);
                }
            }
            else
            {
                string runtimesWin64Native = Path.Combine(thisPackageDir, "runtimes", "win-x64", "native");
                if (!Directory.Exists(runtimesWin64Native))
                {
                    throw new ArgumentException($"Error: Directory {runtimesWin64Native} does not exist!");
                }

                string packageIdName = package.id;
                if (packageIdName.EndsWith(".redist"))
                {
                    packageIdName = packageIdName.Replace(".redist", "");
                }
                string packageIdDir = Path.Combine(assembliesDir, packageIdName, "x64");
                if (!Directory.Exists(packageIdDir))
                {
                    try
                    {
                        Directory.CreateDirectory(packageIdDir);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException($"Error: Couldn't create missing directory {packageIdDir} ({ex.Message})!");
                    }
                }

                string[] dirFiles = Directory.GetFiles(runtimesWin64Native, "*");
                foreach (var sourceFile in dirFiles)
                {
                    string destinationFile = Path.Combine(packageIdDir, Path.GetFileName(sourceFile));

                    CopyIfDifferent(sourceFile, destinationFile);
                    //Console.WriteLine(file);
                }
            }
        }

        private void CopyIfDifferent(string sourceFile, string destinationFile)
        {
            FileInfo sourceFileInfo = new FileInfo(sourceFile);
            FileInfo destinationFileInfo = new FileInfo(destinationFile);

            if (File.Exists(destinationFile))
            {
                if (sourceFileInfo.Length == destinationFileInfo.Length)
                {
                    string sourceHash = GetFileHash(sourceFile);
                    string destinationHash = GetFileHash(destinationFile);
                    if (sourceHash == destinationHash)
                    {
                        return;
                    }
                }
            }

            try
            {
                Console.Out.WriteLine($"    Copying {sourceFile.Replace(Environment.CurrentDirectory,"")} to {destinationFile.Replace(Environment.CurrentDirectory, "")}");
                File.Copy(sourceFile, destinationFile, true);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error: Couldn't copy from {sourceFile} to {destinationFile} ({ex.Message})!");
            }

        }

        private string GetFileHash(string fileName)
        {
            var stringBuilder = new StringBuilder();

            using (MD5 md5 = MD5.Create())
            {
                byte[] sourceHash = md5.ComputeHash(File.ReadAllBytes(fileName));
                for (int i = 0; i < sourceHash.Length; i++)
                {
                    stringBuilder.Append(sourceHash[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }

        private Packages GetPackages()
        {
            Packages packages = new Packages();
            //XmlSerializer xmLserializer = new XmlSerializer(typeof(List<Package>));
            XmlSerializer xmLserializer = new XmlSerializer(typeof(Packages));
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Document,

            };
            XmlReader xmlReader = XmlReader.Create(packageFile, xmlReaderSettings);
            packages = (Packages)xmLserializer.Deserialize(xmlReader);

            return packages;
        }
    }

}