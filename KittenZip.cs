using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

public class KittenZipForm : Form
{
    // Color Palette matching the Banner & Logo
    private static readonly Color BgColor = Color.FromArgb(253, 251, 247); // Cream background
    private static readonly Color HeaderColor = Color.FromArgb(245, 240, 232); // Soft sand title bar
    private static readonly Color OrangeAccent = Color.FromArgb(231, 138, 78); // Peach/Orange (kitten logo color)
    private static readonly Color BlueAccent = Color.FromArgb(79, 123, 155); // Hoodie blue
    private static readonly Color TextColor = Color.FromArgb(44, 62, 80); // Slate blue text
    private static readonly Color BorderColor = Color.FromArgb(220, 212, 200);

    // Cryptography key and readme
    private static readonly byte[] kKittensSecretKey = new byte[] {
        0x4B, 0x49, 0x54, 0x54, 0x45, 0x4E, 0x53, 0x5F,
        0x53, 0x45, 0x43, 0x52, 0x45, 0x54, 0x5F, 0x4B,
        0x45, 0x59, 0x5F, 0x32, 0x30, 0x32, 0x36, 0x5F,
        0x41, 0x44, 0x4D, 0x49, 0x4E, 0x5F, 0x39, 0x39
    };

    private static readonly string kReadmeContent =
        "This is a .kittens file compressed using KittenZip.\n" +
        "To extract this archive, please install KittenZip.\n\n" +
        "Credits: Made by TSun & 1Shot (Script Kittens)\n";

    // Controls
    private Panel headerPanel;
    private Label titleLabel;
    private Button closeButton;
    private Button minButton;
    
    private PictureBox logoBox;
    private Label mainTitleLabel;
    private Label subtitleLabel;
    
    private Panel dragDropPanel;
    private Label dragDropLabel;
    
    private Button compressBtn;
    private Button decompressBtn;
    
    private Label statusLabel;
    private Panel progressBg;
    private Panel progressBar;

    private bool drag = false;
    private Point startPoint = new Point(0, 0);

    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length >= 3)
        {
            RunCommandLine(args);
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new KittenZipForm());
    }

    private static void RunCommandLine(string[] args)
    {
        string mode = args[0];
        string archivePath = args[1];
        string sourcePath = args[2];
        try
        {
            if (mode == "-c")
            {
                Compress(sourcePath, archivePath, null);
            }
            else if (mode == "-x")
            {
                Decompress(archivePath, sourcePath, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    public KittenZipForm()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.Size = new Size(800, 480);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = BgColor;
        this.AllowDrop = true;

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // 1. Header Panel
        headerPanel = new Panel();
        headerPanel.Size = new Size(800, 40);
        headerPanel.Location = new Point(0, 0);
        headerPanel.BackColor = HeaderColor;
        headerPanel.MouseDown += Header_MouseDown;
        headerPanel.MouseMove += Header_MouseMove;
        headerPanel.MouseUp += Header_MouseUp;

        titleLabel = new Label();
        titleLabel.Text = "  KittenZip — Fast & Cute File Archiver";
        titleLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        titleLabel.ForeColor = TextColor;
        titleLabel.AutoSize = false;
        titleLabel.Size = new Size(600, 40);
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        titleLabel.MouseDown += Header_MouseDown;
        titleLabel.MouseMove += Header_MouseMove;
        titleLabel.MouseUp += Header_MouseUp;

        closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.Size = new Size(40, 40);
        closeButton.Location = new Point(760, 0);
        closeButton.FlatStyle = FlatStyle.Flat;
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 17, 35);
        closeButton.ForeColor = TextColor;
        closeButton.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        closeButton.Click += (s, e) => this.Close();

        minButton = new Button();
        minButton.Text = "—";
        minButton.Size = new Size(40, 40);
        minButton.Location = new Point(720, 0);
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

        // 2. Left side: Branding / Mascot Display
        logoBox = new PictureBox();
        logoBox.Location = new Point(30, 70);
        logoBox.Size = new Size(320, 280);
        logoBox.SizeMode = PictureBoxSizeMode.Zoom;
        logoBox.BackColor = Color.Transparent;

        string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "KittenZip.png");
        if (File.Exists(logoPath))
        {
            try
            {
                logoBox.Image = Image.FromFile(logoPath);
            }
            catch
            {
                DrawFallbackLogo();
            }
        }
        else
        {
            DrawFallbackLogo();
        }

        this.Controls.Add(logoBox);

        mainTitleLabel = new Label();
        mainTitleLabel.Text = "KITTENZIP";
        mainTitleLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
        mainTitleLabel.ForeColor = OrangeAccent;
        mainTitleLabel.Location = new Point(30, 360);
        mainTitleLabel.Size = new Size(320, 45);
        mainTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
        this.Controls.Add(mainTitleLabel);

        subtitleLabel = new Label();
        subtitleLabel.Text = "Fast & Cute File Archiver";
        subtitleLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        subtitleLabel.ForeColor = BlueAccent;
        subtitleLabel.Location = new Point(30, 405);
        subtitleLabel.Size = new Size(320, 25);
        subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
        this.Controls.Add(subtitleLabel);

        // 3. Right side: Interactive Controls & Drag-Drop
        dragDropPanel = new Panel();
        dragDropPanel.Location = new Point(390, 70);
        dragDropPanel.Size = new Size(380, 180);
        dragDropPanel.BackColor = Color.FromArgb(248, 245, 240);
        dragDropPanel.AllowDrop = true;
        dragDropPanel.DragEnter += DragDropPanel_DragEnter;
        dragDropPanel.DragLeave += DragDropPanel_DragLeave;
        dragDropPanel.DragDrop += DragDropPanel_DragDrop;
        dragDropPanel.Paint += DragDropPanel_Paint;

        dragDropLabel = new Label();
        dragDropLabel.Text = "Drag & Drop files or folders here\n\n— OR —";
        dragDropLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        dragDropLabel.ForeColor = Color.FromArgb(120, 110, 95);
        dragDropLabel.TextAlign = ContentAlignment.MiddleCenter;
        dragDropLabel.Dock = DockStyle.Fill;
        dragDropLabel.AllowDrop = true;
        dragDropLabel.DragEnter += DragDropPanel_DragEnter;
        dragDropLabel.DragDrop += DragDropPanel_DragDrop;
        
        dragDropPanel.Controls.Add(dragDropLabel);
        this.Controls.Add(dragDropPanel);

        // Compress Button
        compressBtn = new Button();
        compressBtn.Text = "Compress Folder";
        compressBtn.Location = new Point(390, 270);
        compressBtn.Size = new Size(180, 45);
        compressBtn.FlatStyle = FlatStyle.Flat;
        compressBtn.FlatAppearance.BorderSize = 0;
        compressBtn.BackColor = OrangeAccent;
        compressBtn.ForeColor = Color.White;
        compressBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        compressBtn.Cursor = Cursors.Hand;
        compressBtn.Click += CompressBtn_Click;
        this.Controls.Add(compressBtn);

        // Decompress Button
        decompressBtn = new Button();
        decompressBtn.Text = "Extract .kittens";
        decompressBtn.Location = new Point(590, 270);
        decompressBtn.Size = new Size(180, 45);
        decompressBtn.FlatStyle = FlatStyle.Flat;
        decompressBtn.FlatAppearance.BorderSize = 0;
        decompressBtn.BackColor = BlueAccent;
        decompressBtn.ForeColor = Color.White;
        decompressBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        decompressBtn.Cursor = Cursors.Hand;
        decompressBtn.Click += DecompressBtn_Click;
        this.Controls.Add(decompressBtn);

        // 4. Status Bar and Progress
        statusLabel = new Label();
        statusLabel.Text = "Ready to compress or extract.";
        statusLabel.Font = new Font("Segoe UI", 9, FontStyle.Regular);
        statusLabel.ForeColor = TextColor;
        statusLabel.Location = new Point(390, 350);
        statusLabel.Size = new Size(380, 30);
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        this.Controls.Add(statusLabel);

        progressBg = new Panel();
        progressBg.Location = new Point(390, 390);
        progressBg.Size = new Size(380, 10);
        progressBg.BackColor = Color.FromArgb(230, 225, 215);
        this.Controls.Add(progressBg);

        progressBar = new Panel();
        progressBar.Location = new Point(0, 0);
        progressBar.Size = new Size(0, 10);
        progressBar.BackColor = OrangeAccent;
        progressBg.Controls.Add(progressBar);

        this.Paint += Form_Paint;
    }

    private void DrawFallbackLogo()
    {
        Bitmap bmp = new Bitmap(320, 280);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(BgColor);
            g.SmoothingMode = SmoothingMode.HighQuality;
            using (Brush orangeBrush = new SolidBrush(OrangeAccent))
            {
                Point[] leftEar = { new Point(80, 120), new Point(120, 70), new Point(130, 120) };
                Point[] rightEar = { new Point(240, 120), new Point(200, 70), new Point(190, 120) };
                g.FillPolygon(orangeBrush, leftEar);
                g.FillPolygon(orangeBrush, rightEar);
                g.FillEllipse(orangeBrush, 90, 100, 140, 110);
            }
            g.FillEllipse(Brushes.White, 120, 130, 25, 25);
            g.FillEllipse(Brushes.White, 175, 130, 25, 25);
            g.FillEllipse(Brushes.Black, 127, 135, 15, 15);
            g.FillEllipse(Brushes.Black, 177, 135, 15, 15);
            using (Pen pen = new Pen(Color.FromArgb(100, 50, 20), 3))
            {
                g.DrawArc(pen, 138, 165, 20, 15, 0, 180);
                g.DrawArc(pen, 158, 165, 20, 15, 0, 180);
            }
        }
        logoBox.Image = bmp;
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

    private void DragDropPanel_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effect = DragDropEffects.Copy;
            dragDropPanel.BackColor = Color.FromArgb(235, 245, 250);
        }
    }

    private void DragDropPanel_DragLeave(object sender, EventArgs e)
    {
        dragDropPanel.BackColor = Color.FromArgb(248, 245, 240);
    }

    private void DragDropPanel_DragDrop(object sender, DragEventArgs e)
    {
        dragDropPanel.BackColor = Color.FromArgb(248, 245, 240);
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files.Length > 0)
        {
            string targetPath = files[0];
            if (Directory.Exists(targetPath))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "KittenZip Archive (*.kittens)|*.kittens";
                sfd.FileName = Path.GetFileName(targetPath) + ".kittens";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    DoCompressionAsync(targetPath, sfd.FileName);
                }
            }
            else if (File.Exists(targetPath))
            {
                if (targetPath.EndsWith(".kittens", StringComparison.OrdinalIgnoreCase))
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.Description = "Select extraction folder";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        DoDecompressionAsync(targetPath, fbd.SelectedPath);
                    }
                }
                else
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "KittenZip Archive (*.kittens)|*.kittens";
                    sfd.FileName = Path.GetFileNameWithoutExtension(targetPath) + ".kittens";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        DoCompressionAsync(targetPath, sfd.FileName);
                    }
                }
            }
        }
    }

    private void DragDropPanel_Paint(object sender, PaintEventArgs e)
    {
        Pen pen = new Pen(BorderColor, 2);
        pen.DashStyle = DashStyle.Dash;
        e.Graphics.DrawRectangle(pen, 2, 2, dragDropPanel.Width - 4, dragDropPanel.Height - 4);
    }

    private void Form_Paint(object sender, PaintEventArgs e)
    {
        Pen pen = new Pen(BorderColor, 1);
        e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
    }

    private void CompressBtn_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog fbd = new FolderBrowserDialog();
        fbd.Description = "Select a folder to compress";
        if (fbd.ShowDialog() == DialogResult.OK)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "KittenZip Archive (*.kittens)|*.kittens";
            sfd.FileName = Path.GetFileName(fbd.SelectedPath) + ".kittens";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                DoCompressionAsync(fbd.SelectedPath, sfd.FileName);
            }
        }
    }

    private void DecompressBtn_Click(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "KittenZip Archive (*.kittens)|*.kittens";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select destination folder for extraction";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                DoDecompressionAsync(ofd.FileName, fbd.SelectedPath);
            }
        }
    }

    private void UpdateProgress(int percentage)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action<int>(UpdateProgress), percentage);
            return;
        }
        progressBar.Width = (int)((progressBg.Width * percentage) / 100.0);
    }

    private void SetStatus(string text)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action<string>(SetStatus), text);
            return;
        }
        statusLabel.Text = text;
    }

    private void EnableButtons(bool enable)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action<bool>(EnableButtons), enable);
            return;
        }
        compressBtn.Enabled = enable;
        decompressBtn.Enabled = enable;
        dragDropPanel.Enabled = enable;
    }

    private void DoCompressionAsync(string sourcePath, string archivePath)
    {
        EnableButtons(false);
        SetStatus("Compressing files to secure .kittens archive...");
        UpdateProgress(10);

        System.Threading.ThreadPool.QueueUserWorkItem((s) => {
            try
            {
                Compress(sourcePath, archivePath, (percent) => {
                    UpdateProgress(percent);
                });
                UpdateProgress(100);
                SetStatus("Compression complete!");
                MessageBox.Show("Archive created successfully at:\n" + archivePath, "KittenZip", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UpdateProgress(0);
                SetStatus("Compression failed!");
                MessageBox.Show("Error: " + ex.Message, "KittenZip - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EnableButtons(true);
            }
        });
    }

    private void DoDecompressionAsync(string archivePath, string destPath)
    {
        EnableButtons(false);
        SetStatus("Decompressing files from secure .kittens archive...");
        UpdateProgress(10);

        System.Threading.ThreadPool.QueueUserWorkItem((s) => {
            try
            {
                Decompress(archivePath, destPath, (percent) => {
                    UpdateProgress(percent);
                });
                UpdateProgress(100);
                SetStatus("Extraction complete!");
                MessageBox.Show("Archive extracted successfully to:\n" + destPath, "KittenZip", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                UpdateProgress(0);
                SetStatus("Extraction failed!");
                MessageBox.Show("Error: " + ex.Message, "KittenZip - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EnableButtons(true);
            }
        });
    }

    private static void Compress(string sourcePath, string archivePath, Action<int> progressCallback)
    {
        string tempZip = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
        try
        {
            if (File.Exists(tempZip)) File.Delete(tempZip);
            
            if (Directory.Exists(sourcePath))
            {
                ZipFile.CreateFromDirectory(sourcePath, tempZip, CompressionLevel.Optimal, false);
            }
            else if (File.Exists(sourcePath))
            {
                using (ZipArchive zip = ZipFile.Open(tempZip, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(sourcePath, Path.GetFileName(sourcePath));
                }
            }

            if (progressCallback != null) progressCallback(40);

            if (File.Exists(archivePath)) File.Delete(archivePath);
            using (FileStream fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    ZipArchiveEntry readmeEntry = zip.CreateEntry("READ_ME.txt", CompressionLevel.NoCompression);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.Write(kReadmeContent);
                    }

                    if (progressCallback != null) progressCallback(60);

                    ZipArchiveEntry payloadEntry = zip.CreateEntry("payload.dat", CompressionLevel.NoCompression);
                    using (Stream payloadStream = payloadEntry.Open())
                    {
                        using (FileStream tempFs = new FileStream(tempZip, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            long totalBytesWritten = 0;
                            long fileLength = tempFs.Length;
                            
                            while ((bytesRead = tempFs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    buffer[i] ^= kKittensSecretKey[(totalBytesWritten + i) % kKittensSecretKey.Length];
                                }
                                payloadStream.Write(buffer, 0, bytesRead);
                                totalBytesWritten += bytesRead;

                                if (fileLength > 0 && progressCallback != null)
                                {
                                    int pct = 60 + (int)((totalBytesWritten * 35) / fileLength);
                                    progressCallback(pct);
                                }
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

    private static void Decompress(string archivePath, string destDir, Action<int> progressCallback)
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
                        throw new Exception("Not a valid .kittens archive. Make sure you use the official KittenZip.");
                    }

                    if (progressCallback != null) progressCallback(30);

                    using (Stream payloadStream = payloadEntry.Open())
                    {
                        using (FileStream tempFs = new FileStream(tempZip, FileMode.Create, FileAccess.Write))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            long totalBytesRead = 0;
                            long fileLength = payloadEntry.Length;

                            while ((bytesRead = payloadStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                for (int i = 0; i < bytesRead; i++)
                                {
                                    buffer[i] ^= kKittensSecretKey[(totalBytesRead + i) % kKittensSecretKey.Length];
                                }
                                tempFs.Write(buffer, 0, bytesRead);
                                totalBytesRead += bytesRead;

                                if (fileLength > 0 && progressCallback != null)
                                {
                                    int pct = 30 + (int)((totalBytesRead * 40) / fileLength);
                                    progressCallback(pct);
                                }
                            }
                        }
                    }
                }
            }

            if (progressCallback != null) progressCallback(80);

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
