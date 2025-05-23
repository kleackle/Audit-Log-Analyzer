using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Xml;
using System.Diagnostics.Eventing.Reader;
using System.Threading;

namespace LogAnalyzer
{
    public partial class MainForm : Form
    {
        private string? logFolderPath;
        private FileSystemWatcher? watcher;
        private DataTable? logTable;
        private IContainer? components = null;
        private CancellationTokenSource? cancellationTokenSource;
        private Task? currentProcessingTask;

        public MainForm()
        {
            InitializeComponent();
            InitializeDataTable();
            InitializeFileWatcher();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Text = "Log Analyzer";

            // Klasör seçme butonu
            Button btnSelectFolder = new Button();
            btnSelectFolder.Text = "Klasör Seç";
            btnSelectFolder.Location = new Point(10, 10);
            btnSelectFolder.Click += BtnSelectFolder_Click;
            this.Controls.Add(btnSelectFolder);

            // Filtreleme Grubu
            GroupBox filterGroup = new GroupBox();
            filterGroup.Text = "Filtreleme";
            filterGroup.Location = new Point(10, 40);
            filterGroup.Size = new Size(1180, 100);

            // Kullanıcı adı filtresi
            Label lblUser = new Label();
            lblUser.Text = "Kullanıcı:";
            lblUser.Location = new Point(10, 20);
            lblUser.AutoSize = true;

            TextBox txtSearchUser = new TextBox();
            txtSearchUser.Name = "txtSearchUser";
            txtSearchUser.Location = new Point(10, 40);
            txtSearchUser.Width = 150;
            txtSearchUser.PlaceholderText = "Kullanıcı adı...";
            txtSearchUser.TextChanged += TxtSearch_TextChanged;

            // İşlem filtresi
            Label lblOperation = new Label();
            lblOperation.Text = "İşlem:";
            lblOperation.Location = new Point(170, 20);
            lblOperation.AutoSize = true;

            TextBox txtSearchOperation = new TextBox();
            txtSearchOperation.Name = "txtSearchOperation";
            txtSearchOperation.Location = new Point(170, 40);
            txtSearchOperation.Width = 150;
            txtSearchOperation.PlaceholderText = "İşlem...";
            txtSearchOperation.TextChanged += TxtSearch_TextChanged;

            // Dosya yolu filtresi
            Label lblPath = new Label();
            lblPath.Text = "Dosya Yolu:";
            lblPath.Location = new Point(330, 20);
            lblPath.AutoSize = true;

            TextBox txtSearchPath = new TextBox();
            txtSearchPath.Name = "txtSearchPath";
            txtSearchPath.Location = new Point(330, 40);
            txtSearchPath.Width = 250;
            txtSearchPath.PlaceholderText = "Dosya yolu...";
            txtSearchPath.TextChanged += TxtSearch_TextChanged;

            // İşlem adı filtresi
            Label lblProcessName = new Label();
            lblProcessName.Text = "İşlem Adı:";
            lblProcessName.Location = new Point(590, 20);
            lblProcessName.AutoSize = true;

            TextBox txtSearchProcessName = new TextBox();
            txtSearchProcessName.Name = "txtSearchProcessName";
            txtSearchProcessName.Location = new Point(590, 40);
            txtSearchProcessName.Width = 150;
            txtSearchProcessName.PlaceholderText = "İşlem adı...";
            txtSearchProcessName.TextChanged += TxtSearch_TextChanged;

            // Bilgisayar filtresi
            Label lblComputer = new Label();
            lblComputer.Text = "Bilgisayar:";
            lblComputer.Location = new Point(750, 20);
            lblComputer.AutoSize = true;

            TextBox txtSearchComputer = new TextBox();
            txtSearchComputer.Name = "txtSearchComputer";
            txtSearchComputer.Location = new Point(750, 40);
            txtSearchComputer.Width = 150;
            txtSearchComputer.PlaceholderText = "Bilgisayar adı...";
            txtSearchComputer.TextChanged += TxtSearch_TextChanged;

            // Olay Kimliği filtresi
            Label lblEventId = new Label();
            lblEventId.Text = "Olay Kimliği:";
            lblEventId.Location = new Point(910, 20);
            lblEventId.AutoSize = true;

            TextBox txtSearchEventId = new TextBox();
            txtSearchEventId.Name = "txtSearchEventId";
            txtSearchEventId.Location = new Point(910, 40);
            txtSearchEventId.Width = 100;
            txtSearchEventId.PlaceholderText = "Olay ID...";
            txtSearchEventId.TextChanged += TxtSearch_TextChanged;

            // Kontrolleri gruba ekle
            filterGroup.Controls.AddRange(new Control[] { 
                lblUser, txtSearchUser,
                lblOperation, txtSearchOperation,
                lblPath, txtSearchPath,
                lblProcessName, txtSearchProcessName,
                lblComputer, txtSearchComputer,
                lblEventId, txtSearchEventId
            });

            this.Controls.Add(filterGroup);

            // DataGridView
            DataGridView dgvLogs = new DataGridView();
            dgvLogs.Name = "dgvLogs";
            dgvLogs.Location = new Point(10, 150);
            dgvLogs.Size = new Size(1180, 540);
            dgvLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLogs.ReadOnly = true;
            this.Controls.Add(dgvLogs);
        }

        private void InitializeDataTable()
        {
            logTable = new DataTable();
            logTable.Columns.Add("Tarih/Saat", typeof(DateTime));
            logTable.Columns.Add("Kullanıcı", typeof(string));
            logTable.Columns.Add("İşlem", typeof(string));
            logTable.Columns.Add("Dosya Yolu", typeof(string));
            logTable.Columns.Add("Dosya Adı", typeof(string));
            logTable.Columns.Add("İşlem Adı", typeof(string));
            logTable.Columns.Add("Bilgisayar", typeof(string));
            logTable.Columns.Add("Olay Kimliği", typeof(string));

            if (Controls.Find("dgvLogs", true).FirstOrDefault() is DataGridView dgv)
            {
                dgv.DataSource = logTable;
            }
        }

        private void InitializeFileWatcher()
        {
            watcher = new FileSystemWatcher();
            watcher.Filter = "*.evtx";
            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = false;
        }

        private void BtnSelectFolder_Click(object? sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    logFolderPath = fbd.SelectedPath;
                    if (watcher != null)
                    {
                        watcher.Path = logFolderPath;
                        watcher.EnableRaisingEvents = true;
                    }
                    LoadExistingLogs();
                }
            }
        }

        private void LoadExistingLogs()
        {
            if (logTable == null || string.IsNullOrEmpty(logFolderPath)) return;

            // Önceki işlemi iptal et
            cancellationTokenSource?.Cancel();
            currentProcessingTask?.Wait();

            logTable.Clear();
            string[] logFiles = Directory.GetFiles(logFolderPath, "*.evtx");
            
            // Yeni işlem için token oluştur
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            // Asenkron işlemi başlat
            currentProcessingTask = Task.Run(async () =>
            {
                foreach (string file in logFiles)
                {
                    if (token.IsCancellationRequested)
                        break;

                    await ProcessLogFileAsync(file, token);
                }
            }, token);
        }

        private async Task ProcessLogFileAsync(string eventFilePath, CancellationToken token)
        {
            if (logTable == null) return;

            try
            {
                using (var reader = new EventLogReader(eventFilePath, PathType.FilePath))
                {
                    EventRecord? eventRecord;
                    while ((eventRecord = reader.ReadEvent()) != null)
                    {
                        if (token.IsCancellationRequested)
                            break;

                        using (eventRecord)
                        {
                            try
                            {
                                var xml = new XmlDocument();
                                xml.LoadXml(eventRecord.ToXml());

                                // XML namespace tanımla
                                var nsManager = new XmlNamespaceManager(xml.NameTable);
                                nsManager.AddNamespace("e", "http://schemas.microsoft.com/win/2004/08/events/event");

                                // Kullanıcı bilgisini al
                                string? username = null;
                                var userNameNode = xml.SelectSingleNode("//e:Data[@Name='SubjectUserName']", nsManager);
                                var domainNameNode = xml.SelectSingleNode("//e:Data[@Name='SubjectDomainName']", nsManager);
                                if (userNameNode != null && domainNameNode != null)
                                {
                                    username = $"{domainNameNode.InnerText}\\{userNameNode.InnerText}";
                                }

                                // Dosya yolu bilgisini al
                                string? filePath = null;
                                var shareNameNode = xml.SelectSingleNode("//e:Data[@Name='ShareName']", nsManager);
                                var relativeTargetNode = xml.SelectSingleNode("//e:Data[@Name='RelativeTargetName']", nsManager);
                                if (shareNameNode != null && relativeTargetNode != null)
                                {
                                    string shareName = shareNameNode.InnerText.Replace("\\*", "");
                                    filePath = $"{shareName}\\{relativeTargetNode.InnerText}";
                                }

                                // Dosya adını al
                                string? fileName = relativeTargetNode?.InnerText;

                                // İşlem bilgisini al
                                string? operation = null;
                                var accessMaskNode = xml.SelectSingleNode("//e:Data[@Name='AccessMask']", nsManager);
                                var accessListNode = xml.SelectSingleNode("//e:Data[@Name='AccessList']", nsManager);
                                if (accessMaskNode != null && accessListNode != null)
                                {
                                    operation = GetOperationType(accessMaskNode.InnerText, accessListNode.InnerText);
                                }

                                // Bilgisayar bilgisini al
                                string? computer = xml.SelectSingleNode("//e:Computer", nsManager)?.InnerText;

                                // UI thread'inde veriyi ekle
                                await this.InvokeAsync(() =>
                                {
                                    if (token.IsCancellationRequested)
                                        return;

                                    DataRow row = logTable.NewRow();
                                    row["Tarih/Saat"] = eventRecord.TimeCreated ?? DateTime.Now;
                                    row["Kullanıcı"] = username ?? "Bilinmiyor";
                                    row["İşlem"] = operation ?? "Bilinmiyor";
                                    row["Dosya Yolu"] = filePath ?? "";
                                    row["Dosya Adı"] = fileName ?? "";
                                    row["İşlem Adı"] = eventRecord.ProviderName ?? "";
                                    row["Bilgisayar"] = computer ?? "";
                                    row["Olay Kimliği"] = eventRecord.Id.ToString();

                                    logTable.Rows.Add(row);
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Event kaydı işlenirken hata: {ex.Message}");
                                Console.WriteLine($"XML içeriği: {eventRecord.ToXml()}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Log dosyası işlenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private string GetOperationType(string accessMask, string accessList)
        {
            // AccessList içindeki kodları kontrol et
            if (accessList.Contains("%%1541"))
            {
                return "Okuma";
            }
            else if (accessList.Contains("%%4423"))
            {
                return "Yazma";
            }
            else if (accessList.Contains("%%4416"))
            {
                return "ReadData/ListDirectory";
            }

            // AccessMask değerlerine göre işlem türünü belirle
            return accessMask switch
            {
                "0x1" => "ReadData/ListDirectory",
                "0x2" => "WriteData/AddFile",
                "0x4" => "AppendData/AddSubdirectory",
                "0x8" => "ReadEA",
                "0x10" => "WriteEA",
                "0x20" => "Execute/Traverse",
                "0x40" => "DeleteChild",
                "0x80" => "ReadAttributes",
                "0x100" => "WriteAttributes",
                "0x10000" => "DELETE",
                "0x20000" => "READ_CONTROL",
                "0x40000" => "WRITE_DAC",
                "0x80000" => "WRITE_OWNER",
                "0x100000" => "SYNCHRONIZE",
                "0x100080" => "Okuma ve Yazma",
                _ => $"Diğer ({accessMask})"
            };
        }

        private void Watcher_Created(object? sender, FileSystemEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                _ = ProcessLogFileAsync(e.FullPath, CancellationToken.None);
            });
        }

        private void Watcher_Changed(object? sender, FileSystemEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                _ = ProcessLogFileAsync(e.FullPath, CancellationToken.None);
            });
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            if (logTable == null) return;

            var filters = new List<string>();

            // Kullanıcı filtresi
            string userFilter = ((TextBox)Controls.Find("txtSearchUser", true)[0]).Text.Trim();
            if (!string.IsNullOrEmpty(userFilter))
            {
                filters.Add($"[Kullanıcı] LIKE '%{userFilter}%'");
            }

            // İşlem filtresi
            string operationFilter = ((TextBox)Controls.Find("txtSearchOperation", true)[0]).Text.Trim();
            if (!string.IsNullOrEmpty(operationFilter))
            {
                filters.Add($"[İşlem] LIKE '%{operationFilter}%'");
            }

            // Dosya yolu filtresi
            string pathFilter = ((TextBox)Controls.Find("txtSearchPath", true)[0]).Text.Trim();
            if (!string.IsNullOrEmpty(pathFilter))
            {
                filters.Add($"[Dosya Yolu] LIKE '%{pathFilter}%'");
            }

            // İşlem adı filtresi
            string processNameFilter = ((TextBox)Controls.Find("txtSearchProcessName", true)[0]).Text.Trim();
            if (!string.IsNullOrEmpty(processNameFilter))
            {
                filters.Add($"[İşlem Adı] LIKE '%{processNameFilter}%'");
            }

            // Bilgisayar filtresi
            string computerFilter = ((TextBox)Controls.Find("txtSearchComputer", true)[0]).Text.Trim();
            if (!string.IsNullOrEmpty(computerFilter))
            {
                filters.Add($"[Bilgisayar] LIKE '%{computerFilter}%'");
            }

            // Olay Kimliği filtresi
            string eventIdFilter = ((TextBox)Controls.Find("txtSearchEventId", true)[0]).Text.Trim();
            if (!string.IsNullOrEmpty(eventIdFilter))
            {
                filters.Add($"[Olay Kimliği] LIKE '%{eventIdFilter}%'");
            }

            try
            {
                // Filtreleri birleştir
                string finalFilter = string.Join(" AND ", filters);

                // Filtreyi uygula
                DataView dv = logTable.DefaultView;
                dv.RowFilter = finalFilter;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filtreleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            base.OnFormClosing(e);
        }
    }

    public static class ControlExtensions
    {
        public static async Task InvokeAsync(this Control control, Action action)
        {
            await Task.Run(() => control.Invoke(action));
        }
    }
}