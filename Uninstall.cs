using Microsoft.Win32;
using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

class Program
{
    static void Main(string[] args)
    {
        DialogResult res = MessageBox.Show(
            "Are you sure you want to completely uninstall KittenZip?",
            "Uninstall KittenZip",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );
        if (res != DialogResult.Yes) return;

        try
        {
            // 1. Remove file associations
            Registry.ClassesRoot.DeleteSubKeyTree(".kittens", false);
            Registry.ClassesRoot.DeleteSubKeyTree("KittenZip.Assoc", false);

            // 2. Remove Uninstall info
            Registry.LocalMachine.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\KittenZip", false);

            // 3. Remove shortcuts
            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string startMenuDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "KittenZip");
            
            string desktopShortcut = Path.Combine(desktopDir, "KittenZip.lnk");
            if (File.Exists(desktopShortcut)) File.Delete(desktopShortcut);
            
            if (Directory.Exists(startMenuDir))
            {
                Directory.Delete(startMenuDir, true);
            }

            // 4. Delete files (except ourselves)
            string appDir = @"C:\Program Files\KittenZip";
            if (Directory.Exists(appDir))
            {
                foreach (string file in Directory.GetFiles(appDir))
                {
                    if (Path.GetFileName(file).Equals("Uninstall.exe", StringComparison.OrdinalIgnoreCase))
                        continue;
                    try { File.Delete(file); } catch {}
                }
            }

            // 5. Spawn self-deletion script
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c timeout /t 1 && del /f /q \"" + Path.Combine(appDir, "Uninstall.exe") + "\" && rmdir \"" + appDir + "\"";
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);

            MessageBox.Show("KittenZip has been successfully uninstalled.", "KittenZip", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Uninstall failed: " + ex.Message, "KittenZip - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
