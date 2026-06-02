using System;
using System.IO;
using System.IO.Compression;

class Program
{
    private static readonly byte[] kKittensSecretKey = new byte[] {
        0x4B, 0x49, 0x54, 0x54, 0x45, 0x4E, 0x53, 0x5F, // "KITTENS_"
        0x53, 0x45, 0x43, 0x52, 0x45, 0x54, 0x5F, 0x4B, // "SECRET_K"
        0x45, 0x59, 0x5F, 0x32, 0x30, 0x32, 0x36, 0x5F, // "EY_2026_"
        0x41, 0x44, 0x4D, 0x49, 0x4E, 0x5F, 0x39, 0x39  // "ADMIN_99"
    };

    private static readonly string kReadmeContent =
        "This is a .kittens file compressed using KittenZip.\n" +
        "To extract this archive, please install KittenZip.\n\n" +
        "Credits: Made by TSun & 1Shot (Script Kittens)\n";

    static void Main(string[] args)
    {
        Console.Title = "Kittens Archiver";
        if (args.Length >= 3)
        {
            string mode = args[0];
            string archivePath = args[1];
            string sourcePath = args[2];

            if (mode == "-c")
            {
                Compress(sourcePath, archivePath);
            }
            else if (mode == "-x")
            {
                Decompress(archivePath, sourcePath);
            }
            else
            {
                ShowUsage();
            }
        }
        else
        {
            InteractiveMenu();
        }
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Compress:   KittensArchiver.exe -c <output.kittens> <input_directory_or_file>");
        Console.WriteLine("  Extract:    KittensArchiver.exe -x <input.kittens> <output_directory>");
        Console.WriteLine("\nOr run without arguments for the interactive menu.");
    }

    static void InteractiveMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("       KITTENS SECURE ARCHIVER          ");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Compress a Directory to .kittens");
            Console.WriteLine("2. Extract a .kittens File");
            Console.WriteLine("3. Exit");
            Console.WriteLine("========================================");
            Console.Write("Enter your choice (1-3): ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("\nEnter the path of the directory to compress: ");
                string sourceDir = Console.ReadLine().Trim('"', ' ');
                if (!Directory.Exists(sourceDir))
                {
                    Console.WriteLine("Directory does not exist! Press any key to return...");
                    Console.ReadKey();
                    continue;
                }
                Console.Write("Enter the destination path for the .kittens file: ");
                string destFile = Console.ReadLine().Trim('"', ' ');
                if (!destFile.EndsWith(".kittens", StringComparison.OrdinalIgnoreCase))
                {
                    destFile += ".kittens";
                }

                try
                {
                    Compress(sourceDir, destFile);
                    Console.WriteLine("\nSuccessfully created: " + destFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError: " + ex.Message);
                }
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
            }
            else if (choice == "2")
            {
                Console.Write("\nEnter the path of the .kittens file to extract: ");
                string archiveFile = Console.ReadLine().Trim('"', ' ');
                if (!File.Exists(archiveFile))
                {
                    Console.WriteLine("File does not exist! Press any key to return...");
                    Console.ReadKey();
                    continue;
                }
                Console.Write("Enter the output directory path: ");
                string destDir = Console.ReadLine().Trim('"', ' ');

                try
                {
                    Decompress(archiveFile, destDir);
                    Console.WriteLine("\nSuccessfully extracted to: " + destDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nError: " + ex.Message);
                }
                Console.WriteLine("Press any key to return...");
                Console.ReadKey();
            }
            else if (choice == "3")
            {
                break;
            }
        }
    }

    static void Compress(string sourceDir, string archivePath)
    {
        string tempZip = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
        try
        {
            if (File.Exists(tempZip)) File.Delete(tempZip);
            ZipFile.CreateFromDirectory(sourceDir, tempZip, CompressionLevel.Optimal, false);

            if (File.Exists(archivePath)) File.Delete(archivePath);
            using (FileStream fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    // Add READ_ME.txt entry
                    ZipArchiveEntry readmeEntry = zip.CreateEntry("READ_ME.txt", CompressionLevel.NoCompression);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.Write(kReadmeContent);
                    }

                    // Add payload.dat entry
                    ZipArchiveEntry payloadEntry = zip.CreateEntry("payload.dat", CompressionLevel.NoCompression);
                    using (Stream payloadStream = payloadEntry.Open())
                    {
                        using (FileStream tempFs = new FileStream(tempZip, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            long totalBytesWritten = 0;
                            while ((bytesRead = tempFs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    buffer[i] ^= kKittensSecretKey[(totalBytesWritten + i) % kKittensSecretKey.Length];
                                }
                                payloadStream.Write(buffer, 0, bytesRead);
                                totalBytesWritten += bytesRead;
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            if (File.Exists(tempZip))
            {
                try { File.Delete(tempZip); } catch {}
            }
        }
    }

    static void Decompress(string archivePath, string destDir)
    {
        string tempZip = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
        try
        {
            using (FileStream fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry payloadEntry = zip.GetEntry("payload.dat");
                    if (payloadEntry == null)
                    {
                        throw new Exception("Not a valid .kittens archive. Make sure you use the official Kittens Archiver.");
                    }

                    using (Stream payloadStream = payloadEntry.Open())
                    {
                        using (FileStream tempFs = new FileStream(tempZip, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            long totalBytesRead = 0;
                            while ((bytesRead = payloadStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    buffer[i] ^= kKittensSecretKey[(totalBytesRead + i) % kKittensSecretKey.Length];
                                }
                                tempFs.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;
                            }
                        }
                    }
                }
            }

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            ZipFile.ExtractToDirectory(tempZip, destDir);
        }
        finally
        {
            if (File.Exists(tempZip))
            {
                try { File.Delete(tempZip); } catch {}
            }
        }
    }
}
