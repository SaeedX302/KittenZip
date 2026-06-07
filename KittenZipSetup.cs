using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

public class KittenZipSetupForm : Form
{
    // Styling constants matching KittenZip Mascot layout
    private static readonly Color BgColor = Color.FromArgb(253, 251, 247); // Cream background
    private static readonly Color HeaderColor = Color.FromArgb(245, 240, 232); // Sand title bar
    private static readonly Color OrangeAccent = Color.FromArgb(231, 138, 78); // Peach/Orange
    private static readonly Color BlueAccent = Color.FromArgb(79, 123, 155); // Hoodie blue
    private static readonly Color TextColor = Color.FromArgb(44, 62, 80); // Slate blue text
    private static readonly Color BorderColor = Color.FromArgb(220, 212, 200);

    // Form controls
    private Panel headerPanel;
    private Label titleLabel;
    private Button closeButton;
    private Button minButton;

    private PictureBox mascotBox;
    private Panel containerPanel;

    // Wizard Panels
    private Panel welcomePanel;
    private Panel folderPanel;
    private Panel progressPanel;
    private Panel finishPanel;

    // Wizard Navigation buttons
    private Button nextBtn;
    private Button backBtn;
    private Button cancelBtn;

    // Folder select components
    private TextBox folderTxt;
    private Button browseBtn;

    // Progress components
    private Label installStatusLbl;
    private Panel progressBg;
    private Panel progressBar;

    // Finish components
    private CheckBox launchChk;

    // State
    private int currentStep = 1; // 1: Welcome, 2: Folder, 3: Install, 4: Finish
    private string installPath = @"C:\Program Files\KittenZip";

    private bool drag = false;
    private Point startPoint = new Point(0, 0);

    [STAThread]
    public static void Main(string[] args)
    {
        bool silent = false;
        foreach (string arg in args)
        {
            if (arg.Equals("/S", StringComparison.OrdinalIgnoreCase) || 
                arg.Equals("/silent", StringComparison.OrdinalIgnoreCase))
            {
                silent = true;
                break;
            }
        }

        if (silent)
        {
            RunSilentInstall();
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new KittenZipSetupForm());
    }

    private static void RunSilentInstall()
    {
        string installPath = @"C:\Program Files\KittenZip";
        try
        {
            Console.WriteLine("Creating directory: " + installPath);
            if (!Directory.Exists(installPath))
            {
                Directory.CreateDirectory(installPath);
            }

            Console.WriteLine("Extracting resources...");
            ExtractResource("KittenZipExe", Path.Combine(installPath, "KittenZip.exe"));
            ExtractResource("UninstallExe", Path.Combine(installPath, "Uninstall.exe"));
            ExtractResource("KittenZipIco", Path.Combine(installPath, "KittenZip.ico"));
            ExtractResource("KittenZipPng", Path.Combine(installPath, "KittenZip.png"));

            Console.WriteLine("Registering file associations...");
            Registry.SetValue(@"HKEY_CLASSES_ROOT\.kittens", "", "KittenZip.Assoc");
            Registry.SetValue(@"HKEY_CLASSES_ROOT\KittenZip.Assoc", "", "KittenZip Secure Archive");
            Registry.SetValue(@"HKEY_CLASSES_ROOT\KittenZip.Assoc\DefaultIcon", "", "\"" + Path.Combine(installPath, "KittenZip.ico") + "\"");
            Registry.SetValue(@"HKEY_CLASSES_ROOT\KittenZip.Assoc\shell\open\command", "", "\"" + Path.Combine(installPath, "KittenZip.exe") + "\" \"%1\"");

            Console.WriteLine("Registering uninstaller info...");
            string uninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\KittenZip";
            using (RegistryKey key = Registry.LocalMachine.CreateSubKey(uninstallKeyPath))
            {
                key.SetValue("DisplayName", "KittenZip");
                key.SetValue("DisplayVersion", "1.0.0");
                key.SetValue("Publisher", "TSun & 1Shot (Script Kittens)");
                key.SetValue("DisplayIcon", "\"" + Path.Combine(installPath, "KittenZip.ico") + "\"");
                key.SetValue("UninstallString", "\"" + Path.Combine(installPath, "Uninstall.exe") + "\"");
                key.SetValue("InstallLocation", "\"" + installPath + "\"");
                key.SetValue("EstimatedSize", 3072);
            }

            Console.WriteLine("Creating shortcuts...");
            string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "KittenZip.lnk");
            string startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "KittenZip");
            if (!Directory.Exists(startMenuFolder))
            {
                Directory.CreateDirectory(startMenuFolder);
            }
            string startMenuPath = Path.Combine(startMenuFolder, "KittenZip.lnk");
            string uninstallShortcutPath = Path.Combine(startMenuFolder, "Uninstall KittenZip.lnk");

            CreateShortcut(desktopPath, Path.Combine(installPath, "KittenZip.exe"), Path.Combine(installPath, "KittenZip.ico"));
            CreateShortcut(startMenuPath, Path.Combine(installPath, "KittenZip.exe"), Path.Combine(installPath, "KittenZip.ico"));
            CreateShortcut(uninstallShortcutPath, Path.Combine(installPath, "Uninstall.exe"), Path.Combine(installPath, "KittenZip.ico"));

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
            Console.WriteLine("Installation completed successfully!");
            try { File.WriteAllText("install_log.txt", "Installation completed successfully!"); } catch {}
        }
        catch (Exception ex)
        {
            try { File.WriteAllText("install_log.txt", "Silent installation failed: " + ex.ToString()); } catch {}
            Console.Error.WriteLine("Silent installation failed: " + ex.Message);
            Environment.Exit(1);
        }
    }

    public KittenZipSetupForm()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.Size = new Size(650, 420);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = BgColor;

        InitializeComponents();
        ShowStep(1);
    }

    private void InitializeComponents()
    {
        // 1. Custom Title Bar
        headerPanel = new Panel();
        headerPanel.Size = new Size(650, 40);
        headerPanel.Location = new Point(0, 0);
        headerPanel.BackColor = HeaderColor;
        headerPanel.MouseDown += Header_MouseDown;
        headerPanel.MouseMove += Header_MouseMove;
        headerPanel.MouseUp += Header_MouseUp;

        titleLabel = new Label();
        titleLabel.Text = "  KittenZip Setup Wizard";
        titleLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        titleLabel.ForeColor = TextColor;
        titleLabel.AutoSize = false;
        titleLabel.Size = new Size(500, 40);
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        titleLabel.MouseDown += Header_MouseDown;
        titleLabel.MouseMove += Header_MouseMove;
        titleLabel.MouseUp += Header_MouseUp;

        closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.Size = new Size(40, 40);
        closeButton.Location = new Point(610, 0);
        closeButton.FlatStyle = FlatStyle.Flat;
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
        closeButton.ForeColor = TextColor;
        closeButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        closeButton.Click += (s, e) => this.Close();

        minButton = new Button();
        minButton.Text = "—";
        minButton.Size = new Size(40, 40);
        minButton.Location = new Point(570, 0);
        minButton.FlatStyle = FlatStyle.Flat;
        minButton.FlatAppearance.BorderSize = 0;
        minButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 205, 195);
        minButton.ForeColor = TextColor;
        minButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        minButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(minButton);
        headerPanel.Controls.Add(closeButton);
        this.Controls.Add(headerPanel);

        // 2. Mascot panel (Left Side Banner)
        mascotBox = new PictureBox();
        mascotBox.Location = new Point(15, 60);
        mascotBox.Size = new Size(200, 280);
        mascotBox.SizeMode = PictureBoxSizeMode.Zoom;
        mascotBox.BackColor = Color.Transparent;

        // Try extracting and loading from embedded resources or disk
        string tempMascot = Path.Combine(Path.GetTempPath(), "KittenZipSetupLogo.png");
        try
        {
            ExtractResource("KittenZipPng", tempMascot);
            mascotBox.Image = Image.FromFile(tempMascot);
        }
        catch
        {
            DrawFallbackLogo();
        }

        this.Controls.Add(mascotBox);

        // 3. Central Container Panel for Wizard Pages
        containerPanel = new Panel();
        containerPanel.Location = new Point(230, 60);
        containerPanel.Size = new Size(400, 280);
        containerPanel.BackColor = Color.Transparent;
        this.Controls.Add(containerPanel);

        // --- Create Page 1: Welcome ---
        welcomePanel = new Panel();
        welcomePanel.Dock = DockStyle.Fill;
        welcomePanel.BackColor = Color.Transparent;

        Label welcomeTitle = new Label();
        welcomeTitle.Text = "Welcome to the KittenZip\nSetup Wizard";
        welcomeTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        welcomeTitle.ForeColor = OrangeAccent;
        welcomeTitle.Location = new Point(10, 10);
        welcomeTitle.Size = new Size(380, 70);

        Label welcomeDesc = new Label();
        welcomeDesc.Text = "This wizard will guide you through the installation of KittenZip on your computer.\n\nKittenZip is a fast, cute, and secure archive utility supporting the custom '.kittens' secure format.\n\nClick Next to continue.";
        welcomeDesc.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        welcomeDesc.ForeColor = TextColor;
        welcomeDesc.Location = new Point(10, 100);
        welcomeDesc.Size = new Size(380, 170);

        welcomePanel.Controls.Add(welcomeTitle);
        welcomePanel.Controls.Add(welcomeDesc);

        // --- Create Page 2: Folder selection ---
        folderPanel = new Panel();
        folderPanel.Dock = DockStyle.Fill;
        folderPanel.BackColor = Color.Transparent;

        Label folderTitle = new Label();
        folderTitle.Text = "Choose Install Location";
        folderTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        folderTitle.ForeColor = OrangeAccent;
        folderTitle.Location = new Point(10, 10);
        folderTitle.Size = new Size(380, 30);

        Label folderDesc = new Label();
        folderDesc.Text = "KittenZip will be installed in the folder below. To install in a different folder, click Browse and select another folder.";
        folderDesc.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        folderDesc.ForeColor = TextColor;
        folderDesc.Location = new Point(10, 50);
        folderDesc.Size = new Size(380, 60);

        folderTxt = new TextBox();
        folderTxt.Text = installPath;
        folderTxt.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        folderTxt.Location = new Point(10, 130);
        folderTxt.Size = new Size(270, 27);

        browseBtn = new Button();
        browseBtn.Text = "Browse...";
        browseBtn.Location = new Point(290, 129);
        browseBtn.Size = new Size(90, 29);
        browseBtn.FlatStyle = FlatStyle.Flat;
        browseBtn.FlatAppearance.BorderColor = BorderColor;
        browseBtn.BackColor = HeaderColor;
        browseBtn.ForeColor = TextColor;
        browseBtn.Cursor = Cursors.Hand;
        browseBtn.Click += BrowseBtn_Click;

        folderPanel.Controls.Add(folderTitle);
        folderPanel.Controls.Add(folderDesc);
        folderPanel.Controls.Add(folderTxt);
        folderPanel.Controls.Add(browseBtn);

        // --- Create Page 3: Progress ---
        progressPanel = new Panel();
        progressPanel.Dock = DockStyle.Fill;
        progressPanel.BackColor = Color.Transparent;

        Label progressTitle = new Label();
        progressTitle.Text = "Installing KittenZip";
        progressTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        progressTitle.ForeColor = OrangeAccent;
        progressTitle.Location = new Point(10, 10);
        progressTitle.Size = new Size(380, 30);

        installStatusLbl = new Label();
        installStatusLbl.Text = "Extracting files...";
        installStatusLbl.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        installStatusLbl.ForeColor = TextColor;
        installStatusLbl.Location = new Point(10, 80);
        installStatusLbl.Size = new Size(380, 30);

        progressBg = new Panel();
        progressBg.Location = new Point(10, 120);
        progressBg.Size = new Size(370, 15);
        progressBg.BackColor = Color.FromArgb(230, 225, 215);

        progressBar = new Panel();
        progressBar.Location = new Point(0, 0);
        progressBar.Size = new Size(0, 15);
        progressBar.BackColor = OrangeAccent;
        progressBg.Controls.Add(progressBar);

        progressPanel.Controls.Add(progressTitle);
        progressPanel.Controls.Add(installStatusLbl);
        progressPanel.Controls.Add(progressBg);

        // --- Create Page 4: Finish ---
        finishPanel = new Panel();
        finishPanel.Dock = DockStyle.Fill;
        finishPanel.BackColor = Color.Transparent;

        Label finishTitle = new Label();
        finishTitle.Text = "Installation Completed!";
        finishTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        finishTitle.ForeColor = OrangeAccent;
        finishTitle.Location = new Point(10, 10);
        finishTitle.Size = new Size(380, 40);

        Label finishDesc = new Label();
        finishDesc.Text = "KittenZip has been successfully installed on your computer.\n\nFile associations for '.kittens' secure files have been registered, and Desktop and Start Menu shortcuts are configured.";
        finishDesc.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        finishDesc.ForeColor = TextColor;
        finishDesc.Location = new Point(10, 70);
        finishDesc.Size = new Size(380, 100);

        launchChk = new CheckBox();
        launchChk.Text = "Launch KittenZip";
        launchChk.Checked = true;
        launchChk.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        launchChk.ForeColor = BlueAccent;
        launchChk.Location = new Point(15, 200);
        launchChk.Size = new Size(200, 30);

        finishPanel.Controls.Add(finishTitle);
        finishPanel.Controls.Add(finishDesc);
        finishPanel.Controls.Add(launchChk);

        // 4. Navigation Buttons (Bottom Panel)
        backBtn = new Button();
        backBtn.Text = "< Back";
        backBtn.Location = new Point(340, 360);
        backBtn.Size = new Size(90, 35);
        backBtn.FlatStyle = FlatStyle.Flat;
        backBtn.FlatAppearance.BorderColor = BorderColor;
        backBtn.BackColor = HeaderColor;
        backBtn.ForeColor = TextColor;
        backBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        backBtn.Cursor = Cursors.Hand;
        backBtn.Click += BackBtn_Click;
        this.Controls.Add(backBtn);

        nextBtn = new Button();
        nextBtn.Text = "Next >";
        nextBtn.Location = new Point(440, 360);
        nextBtn.Size = new Size(90, 35);
        nextBtn.FlatStyle = FlatStyle.Flat;
        nextBtn.FlatAppearance.BorderSize = 0;
        nextBtn.BackColor = OrangeAccent;
        nextBtn.ForeColor = Color.White;
        nextBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        nextBtn.Cursor = Cursors.Hand;
        nextBtn.Click += NextBtn_Click;
        this.Controls.Add(nextBtn);

        cancelBtn = new Button();
        cancelBtn.Text = "Cancel";
        cancelBtn.Location = new Point(540, 360);
        cancelBtn.Size = new Size(90, 35);
        cancelBtn.FlatStyle = FlatStyle.Flat;
        cancelBtn.FlatAppearance.BorderColor = BorderColor;
        cancelBtn.BackColor = HeaderColor;
        cancelBtn.ForeColor = TextColor;
        cancelBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        cancelBtn.Cursor = Cursors.Hand;
        cancelBtn.Click += (s, e) => this.Close();
        this.Controls.Add(cancelBtn);

        this.Paint += Form_Paint;
    }

    private void DrawFallbackLogo()
    {
        Bitmap bmp = new Bitmap(200, 280);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(BgColor);
            g.SmoothingMode = SmoothingMode.HighQuality;
            using (Brush orangeBrush = new SolidBrush(OrangeAccent))
            {
                Point[] leftEar = { new Point(30, 110), new Point(65, 70), new Point(75, 110) };
                Point[] rightEar = { new Point(170, 110), new Point(135, 70), new Point(125, 110) };
                g.FillPolygon(orangeBrush, leftEar);
                g.FillPolygon(orangeBrush, rightEar);
                g.FillEllipse(orangeBrush, 40, 90, 120, 100);
            }
            g.FillEllipse(Brushes.White, 65, 120, 20, 20);
            g.FillEllipse(Brushes.White, 115, 120, 20, 20);
            g.FillEllipse(Brushes.Black, 70, 125, 10, 10);
            g.FillEllipse(Brushes.Black, 118, 125, 10, 10);
        }
        mascotBox.Image = bmp;
    }

    private void Header_MouseDown(object sender, MouseEventArgs e)
    {
        drag = true;
        startPoint = new Point(e.X, e.Y);
    }

    private void Header_MouseMove(object sender, MouseEventArgs e)
    {
        if (drag)
        {
            Point p = PointToScreen(e.Location);
            this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
        }
    }

    private void Header_MouseUp(object sender, MouseEventArgs e)
    {
        drag = false;
    }

    private void Form_Paint(object sender, PaintEventArgs e)
    {
        Pen pen = new Pen(BorderColor, 1);
        e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
    }

    private void BrowseBtn_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        fbd.Description = "Select installation folder";
        if (fbd.ShowDialog() == DialogResult.OK)
        {
            folderTxt.Text = fbd.SelectedPath;
        }
    }

    private void ShowStep(int step)
    {
        currentStep = step;
        containerPanel.Controls.Clear();

        if (step == 1)
        {
            containerPanel.Controls.Add(welcomePanel);
            backBtn.Enabled = false;
            nextBtn.Text = "Next >";
            cancelBtn.Enabled = true;
        }
        else if (step == 2)
        {
            containerPanel.Controls.Add(folderPanel);
            backBtn.Enabled = true;
            nextBtn.Text = "Install";
            cancelBtn.Enabled = true;
        }
        else if (step == 3)
        {
            containerPanel.Controls.Add(progressPanel);
            backBtn.Enabled = false;
            nextBtn.Enabled = false;
            cancelBtn.Enabled = false;
            StartInstallation();
        }
        else if (step == 4)
        {
            containerPanel.Controls.Add(finishPanel);
            backBtn.Visible = false;
            nextBtn.Enabled = true;
            nextBtn.Text = "Finish";
            cancelBtn.Visible = false;
        }
    }

    private void NextBtn_Click(object sender, EventArgs e)
    {
        if (currentStep == 1)
        {
            ShowStep(2);
        }
        else if (currentStep == 2)
        {
            installPath = folderTxt.Text;
            ShowStep(3);
        }
        else if (currentStep == 4)
        {
            if (launchChk.Checked)
            {
                try
                {
                    System.Diagnostics.Process.Start(Path.Combine(installPath, "KittenZip.exe"));
                }
                catch {}
            }
            this.Close();
        }
    }

    private void BackBtn_Click(object sender, EventArgs e)
    {
        if (currentStep == 2)
        {
            ShowStep(1);
        }
    }

    private void UpdateProgress(int percentage, string status)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action<int, string>(UpdateProgress), percentage, status);
            return;
        }
        progressBar.Width = (int)((progressBg.Width * percentage) / 100.0);
        installStatusLbl.Text = status;
    }

    private void StartInstallation()
    {
        System.Threading.ThreadPool.QueueUserWorkItem((s) => {
            try
            {
                UpdateProgress(10, "Creating installation directory...");
                System.Threading.Thread.Sleep(500);

                if (!Directory.Exists(installPath))
                {
                    Directory.CreateDirectory(installPath);
                }

                UpdateProgress(30, "Extracting KittenZip application files...");
                System.Threading.Thread.Sleep(500);

                ExtractResource("KittenZipExe", Path.Combine(installPath, "KittenZip.exe"));
                ExtractResource("UninstallExe", Path.Combine(installPath, "Uninstall.exe"));
                ExtractResource("KittenZipIco", Path.Combine(installPath, "KittenZip.ico"));
                ExtractResource("KittenZipPng", Path.Combine(installPath, "KittenZip.png"));

                UpdateProgress(60, "Configuring registry & file associations...");
                System.Threading.Thread.Sleep(500);

                // Register .kittens File Association
                // HKCR\.kittens = "KittenZip.Assoc"
                Registry.SetValue(@"HKEY_CLASSES_ROOT\.kittens", "", "KittenZip.Assoc");
                // HKCR\KittenZip.Assoc = "KittenZip Secure Archive"
                Registry.SetValue(@"HKEY_CLASSES_ROOT\KittenZip.Assoc", "", "KittenZip Secure Archive");
                // Default Icon
                Registry.SetValue(@"HKEY_CLASSES_ROOT\KittenZip.Assoc\DefaultIcon", "", "\"" + Path.Combine(installPath, "KittenZip.ico") + "\"");
                // Shell Command
                Registry.SetValue(@"HKEY_CLASSES_ROOT\KittenZip.Assoc\shell\open\command", "", "\"" + Path.Combine(installPath, "KittenZip.exe") + "\" \"%1\"");

                UpdateProgress(80, "Registering uninstaller information...");
                System.Threading.Thread.Sleep(500);

                // Add to Windows Programs & Features (Add/Remove Programs)
                string uninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\KittenZip";
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(uninstallKeyPath))
                {
                    key.SetValue("DisplayName", "KittenZip");
                    key.SetValue("DisplayVersion", "1.0.0");
                    key.SetValue("Publisher", "TSun & 1Shot (Script Kittens)");
                    key.SetValue("DisplayIcon", "\"" + Path.Combine(installPath, "KittenZip.ico") + "\"");
                    key.SetValue("UninstallString", "\"" + Path.Combine(installPath, "Uninstall.exe") + "\"");
                    key.SetValue("InstallLocation", "\"" + installPath + "\"");
                    key.SetValue("EstimatedSize", 3072); // ~3 MB
                }

                UpdateProgress(90, "Creating Desktop and Start Menu shortcuts...");
                System.Threading.Thread.Sleep(500);

                string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "KittenZip.lnk");
                string startMenuFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), "KittenZip");
                if (!Directory.Exists(startMenuFolder))
                {
                    Directory.CreateDirectory(startMenuFolder);
                }
                string startMenuPath = Path.Combine(startMenuFolder, "KittenZip.lnk");
                string uninstallShortcutPath = Path.Combine(startMenuFolder, "Uninstall KittenZip.lnk");

                CreateShortcut(desktopPath, Path.Combine(installPath, "KittenZip.exe"), Path.Combine(installPath, "KittenZip.ico"));
                CreateShortcut(startMenuPath, Path.Combine(installPath, "KittenZip.exe"), Path.Combine(installPath, "KittenZip.ico"));
                CreateShortcut(uninstallShortcutPath, Path.Combine(installPath, "Uninstall.exe"), Path.Combine(installPath, "KittenZip.ico"));

                UpdateProgress(100, "Installation complete!");
                System.Threading.Thread.Sleep(500);

                // Notify shell that associations changed
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                this.Invoke(new Action(() => ShowStep(4)));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() => {
                    MessageBox.Show("Installation failed: " + ex.Message, "KittenZip Setup - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }));
            }
        });
    }

    private static void ExtractResource(string resourceName, string outputPath)
    {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new Exception("Resource '" + resourceName + "' not found.");
            
            // Ensure target directory exists
            string dir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fs);
            }
        }
    }

    private static void CreateShortcut(string shortcutPath, string targetPath, string iconPath)
    {
        try
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.Description = "KittenZip Archiver";
            shortcut.IconLocation = iconPath;
            shortcut.Save();
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to create shortcut: " + ex.Message);
        }
    }

    [System.Runtime.InteropServices.DllImport("shell32.dll")]
    private static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
}
