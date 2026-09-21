using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace SpeedrunToggle
{
    public class MainForm : Form
    {
        private const string TemporaryFolderName = "SpeedrunIsolated";
        private static readonly string[] ExcludedDllNames = new string[]
        {
            "fkUpdate.dll",
            "wkResolution.dll"
        };

        private readonly string gameFolder;
        private readonly string temporaryFolder;
        private TableLayoutPanel tlpLayout;
        private Button btnToggle;

        public MainForm()
        {
            gameFolder = AppDomain.CurrentDomain.BaseDirectory;
            temporaryFolder = Path.Combine(gameFolder, TemporaryFolderName);

            InitializeComponent();
            UpdateButtonText();
        }

        private bool IsSpeedrunEnabled()
        {
            if (!Directory.Exists(temporaryFolder))
            {
                return false;
            }

            string[] files = Directory.GetFiles(temporaryFolder, "*.dll", SearchOption.TopDirectoryOnly);
            int i;

            for (i = 0; i < files.Length; i++)
            {
                if (ShouldMove(Path.GetFileName(files[i])))
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateButtonText()
        {
            btnToggle.Text = IsSpeedrunEnabled() ? "Disable Speedrun" : "Enable Speedrun";
        }

        private void EnableSpeedrun()
        {
            List<string> filesToMove = FindSpeedrunDlls(gameFolder);

            if (filesToMove.Count == 0)
            {
                MessageBox.Show(
                    "No matching files were found to move.",
                    "Speedrun",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!Directory.Exists(temporaryFolder))
            {
                Directory.CreateDirectory(temporaryFolder);
            }

            int i;
            for (i = 0; i < filesToMove.Count; i++)
            {
                string sourcePath = filesToMove[i];
                string destinationPath = Path.Combine(temporaryFolder, Path.GetFileName(sourcePath));

                if (File.Exists(destinationPath))
                {
                    throw new IOException("A file already exists in the temporary folder: " + Path.GetFileName(sourcePath));
                }

                File.Move(sourcePath, destinationPath);
            }
        }
        private void DisableSpeedrun()
        {
            if (!Directory.Exists(temporaryFolder))
            {
                return;
            }

            List<string> filesToRestore = FindSpeedrunDlls(temporaryFolder);
            int i;

            for (i = 0; i < filesToRestore.Count; i++)
            {
                string sourcePath = filesToRestore[i];
                string destinationPath = Path.Combine(gameFolder, Path.GetFileName(sourcePath));

                if (File.Exists(destinationPath))
                {
                    throw new IOException("A file already exists in the game folder: " + Path.GetFileName(sourcePath));
                }

                File.Move(sourcePath, destinationPath);
            }

            if (Directory.Exists(temporaryFolder) && Directory.GetFileSystemEntries(temporaryFolder).Length == 0)
            {
                Directory.Delete(temporaryFolder);
            }
        }

        private static List<string> FindSpeedrunDlls(string folder)
        {
            List<string> result = new List<string>();
            string[] dllFiles = Directory.GetFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);
            int i;

            for (i = 0; i < dllFiles.Length; i++)
            {
                string fileName = Path.GetFileName(dllFiles[i]);
                if (ShouldMove(fileName))
                {
                    result.Add(dllFiles[i]);
                }
            }

            return result;
        }

        private static bool ShouldMove(string fileName)
        {
            foreach (string excludedDllName in ExcludedDllNames) {
                if (string.Equals(fileName, excludedDllName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return fileName.StartsWith("fk", StringComparison.OrdinalIgnoreCase)
                || fileName.StartsWith("wk", StringComparison.OrdinalIgnoreCase);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tlpLayout = new System.Windows.Forms.TableLayoutPanel();
            this.btnToggle = new System.Windows.Forms.Button();
            this.tlpLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpLayout
            // 
            this.tlpLayout.ColumnCount = 1;
            this.tlpLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.Controls.Add(this.btnToggle, 0, 0);
            this.tlpLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpLayout.Location = new System.Drawing.Point(0, 0);
            this.tlpLayout.Name = "tlpLayout";
            this.tlpLayout.RowCount = 1;
            this.tlpLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpLayout.Size = new System.Drawing.Size(323, 116);
            this.tlpLayout.TabIndex = 0;
            // 
            // btnToggle
            // 
            this.btnToggle.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnToggle.AutoSize = true;
            this.btnToggle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToggle.Location = new System.Drawing.Point(35, 27);
            this.btnToggle.Name = "btnToggle";
            this.btnToggle.Padding = new System.Windows.Forms.Padding(10);
            this.btnToggle.Size = new System.Drawing.Size(252, 61);
            this.btnToggle.TabIndex = 0;
            this.btnToggle.Text = "Enable Speedrun";
            this.btnToggle.UseVisualStyleBackColor = true;
            this.btnToggle.Click += new System.EventHandler(this.btnToggle_Click);
            // 
            // MainForm
            // 
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(323, 116);
            this.Controls.Add(this.tlpLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Speedrun";
            this.tlpLayout.ResumeLayout(false);
            this.tlpLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            btnToggle.Enabled = false;

            try {
                if (IsSpeedrunEnabled()) {
                    DisableSpeedrun();
                }
                else {
                    EnableSpeedrun();
                }

                UpdateButtonText();
            }
            catch (Exception ex) {
                MessageBox.Show(
                    "The operation could not be completed.\r\n\r\n" + ex.Message,
                    "Speedrun Toggle",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally {
                btnToggle.Enabled = true;
            }
        }
    }
}
