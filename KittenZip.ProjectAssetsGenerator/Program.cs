using ImageMagick;
using Mile.Project.Helpers;
using System.Collections.Concurrent;

namespace KittenZip.ProjectAssetsGenerator
{
    internal class Program
    {
        static string RepositoryRoot = GitRepository.GetRootPath();

        static void Main(string[] args)
        {
            {
                string SourcePath = RepositoryRoot + @"\Assets\OriginalAssets\KittenZip";

                string OutputPath = RepositoryRoot + @"\Assets\PackageAssets";

                ConcurrentDictionary<int, MagickImage> StandardSources =
                    new ConcurrentDictionary<int, MagickImage>();
                ConcurrentDictionary<int, MagickImage> StandardIconSources =
                    new ConcurrentDictionary<int, MagickImage>();
                ConcurrentDictionary<int, MagickImage> ContrastBlackSources =
                    new ConcurrentDictionary<int, MagickImage>();
                ConcurrentDictionary<int, MagickImage> ContrastWhiteSources =
                    new ConcurrentDictionary<int, MagickImage>();

                ConcurrentDictionary<int, MagickImage> ArchiveFileSources =
                    new ConcurrentDictionary<int, MagickImage>();

                ConcurrentDictionary<int, MagickImage> SfxStubSources =
                    new ConcurrentDictionary<int, MagickImage>();

                foreach (var AssetSize in ProjectAssetsUtilities.AssetSizes)
                {
                    StandardSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "Standard",
                        AssetSize));
                    StandardIconSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath.Replace("OriginalAssets", "OriginalAssetsOptimized"),
                        "Standard",
                        AssetSize));
                    ContrastBlackSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "ContrastBlack",
                        AssetSize));
                    ContrastWhiteSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "ContrastWhite",
                        AssetSize));

                    ArchiveFileSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "ArchiveFile",
                        AssetSize));

                    SfxStubSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath.Replace("OriginalAssets", "OriginalAssetsOptimized"),
                        "SelfExtractingExecutable",
                        AssetSize));
                }

                ProjectAssetsUtilities.GeneratePackageApplicationImageAssets(
                    StandardSources,
                    ContrastBlackSources,
                    ContrastWhiteSources,
                    OutputPath);

                ProjectAssetsUtilities.GeneratePackageFileAssociationImageAssets(
                    ArchiveFileSources,
                    OutputPath,
                    @"ArchiveFile");

                ProjectAssetsUtilities.GenerateIconFile(
                    StandardIconSources,
                    OutputPath + @"\..\KittenZip.ico");

                ProjectAssetsUtilities.GenerateIconFile(
                    SfxStubSources,
                    OutputPath + @"\..\KittenZipSfx.ico");

                StandardSources[64].Write(
                    OutputPath + @"\..\KittenZip.png");
            }

            {
                string SourcePath = RepositoryRoot + @"\Assets\OriginalAssets\KittenZipPreview";

                string OutputPath = RepositoryRoot + @"\Assets\PreviewPackageAssets";

                ConcurrentDictionary<int, MagickImage> StandardSources =
                    new ConcurrentDictionary<int, MagickImage>();
                ConcurrentDictionary<int, MagickImage> StandardIconSources =
                    new ConcurrentDictionary<int, MagickImage>();
                ConcurrentDictionary<int, MagickImage> ContrastBlackSources =
                    new ConcurrentDictionary<int, MagickImage>();
                ConcurrentDictionary<int, MagickImage> ContrastWhiteSources =
                    new ConcurrentDictionary<int, MagickImage>();

                ConcurrentDictionary<int, MagickImage> ArchiveFileSources =
                    new ConcurrentDictionary<int, MagickImage>();

                ConcurrentDictionary<int, MagickImage> SfxStubSources =
                    new ConcurrentDictionary<int, MagickImage>();

                foreach (var AssetSize in ProjectAssetsUtilities.AssetSizes)
                {
                    StandardSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "Standard",
                        AssetSize));
                    StandardIconSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath.Replace("OriginalAssets", "OriginalAssetsOptimized"),
                        "Standard",
                        AssetSize));
                    ContrastBlackSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "ContrastBlack",
                        AssetSize));
                    ContrastWhiteSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "ContrastWhite",
                        AssetSize));

                    ArchiveFileSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath,
                        "ArchiveFile",
                        AssetSize));

                    SfxStubSources[AssetSize] = new MagickImage(string.Format(
                        @"{0}\{1}\{1}_{2}.png",
                        SourcePath.Replace("OriginalAssets", "OriginalAssetsOptimized"),
                        "SelfExtractingExecutable",
                        AssetSize));
                }

                ProjectAssetsUtilities.GeneratePackageApplicationImageAssets(
                    StandardSources,
                    ContrastBlackSources,
                    ContrastWhiteSources,
                    OutputPath);

                ProjectAssetsUtilities.GeneratePackageFileAssociationImageAssets(
                    ArchiveFileSources,
                    OutputPath,
                    @"ArchiveFile");

                ProjectAssetsUtilities.GenerateIconFile(
                    StandardIconSources,
                    OutputPath + @"\..\KittenZipPreview.ico");

                ProjectAssetsUtilities.GenerateIconFile(
                    SfxStubSources,
                    OutputPath + @"\..\KittenZipPreviewSfx.ico");

                StandardSources[64].Write(
                    OutputPath + @"\..\KittenZipPreview.png");
            }

            Console.WriteLine(
                "KittenZip.ProjectAssetsGenerator task has been completed.");
            Console.ReadKey();
        }
    }
}
