using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mile.Project.Helpers;
using System.IO;
using System.Xml;

namespace KittenZip.Build.Tasks
{
    public class RefreshProjectBuildNumberDate : Task
    {
        [Required]
        public string? FilePath { get; set; }

        [Required]
        public string? BuildNumberDate { get; set; }

        public override bool Execute()
        {
            XmlDocument Document = new XmlDocument();
            Document.PreserveWhitespace = true;
            Document.Load(FilePath);

            XmlNode? ProjectNode = Document["Project"];
            if (ProjectNode == null)
            {
                Log.LogError(
                    "Cannot find Project node in the manifest.");
                return false;
            }

            XmlNode? PropertyGroupNode = ProjectNode["PropertyGroup"];
            if (PropertyGroupNode == null)
            {
                Log.LogError(
                    "Cannot find PropertyGroup node in the manifest.");
                return false;
            }

            XmlNode? KittenZipBuildNumberDateNode =
                PropertyGroupNode["KittenZipBuildNumberDate"];
            if (KittenZipBuildNumberDateNode == null)
            {
                Log.LogError(
                    "Cannot find KittenZipBuildNumberDate node in the manifest.");
                return false;
            }

            FileUtilities.SaveTextToFileAsUtf8Bom(
                FilePath,
                File.ReadAllText(FilePath).Replace(
                    string.Format(
                        "<{0}>{1}</{0}>",
                        KittenZipBuildNumberDateNode.Name,
                        KittenZipBuildNumberDateNode.InnerText),
                    string.Format(
                        "<{0}>{1}</{0}>",
                        KittenZipBuildNumberDateNode.Name,
                        BuildNumberDate)));

            return true;
        }
    }
}
