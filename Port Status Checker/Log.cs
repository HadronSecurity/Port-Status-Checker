using System;
using System.IO;
using System.Windows.Forms;

namespace Port_Status_Checker
{
    public partial class Log : Form
    {
        public Log()
        {
            InitializeComponent();

            
        }
        private void LoadLogs()
        {
            // Kullanıcı dizininde "Documents" klasörüne kaydedilen "PortHistory.txt" dosyasının yolu
            string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortCheck");
            string filePath = Path.Combine(directoryPath, "PortHistory.txt");

            // Dosyanın var olup olmadığını kontrol et
            if (File.Exists(filePath))
            {
                try
                {
                    // Dosyanın içeriğini oku
                    string logs = File.ReadAllText(filePath);

                    // RichTextBox'a ekle
                    richTextBox1.Text = logs;
                }
                catch (Exception ex)
                {
                    // Hata meydana gelirse, kullanıcıya bildirme
                    MessageBox.Show($"Log dosyası okunurken bir hata oluştu: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Log file not found! Currently, there are no recorded logs. Please perform a port check first to generate logs.");
            }
        }
        private void Log_Load(object sender, EventArgs e)
        {
            LoadLogs();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            // Determine the log file path
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortCheck", "PortHistory.txt");

            // Check if the file exists
            if (File.Exists(filePath))
            {
                try
                {
                    // Clear the file content
                    File.WriteAllText(filePath, string.Empty);

                    // Clear the logs in the RichTextBox
                    richTextBox1.Clear();

                    MessageBox.Show("Logs have been successfully cleared!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while clearing the logs: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Log file not found!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // Klasör yolu
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortCheck");

            // Klasör var mı kontrol et
            if (Directory.Exists(folderPath))
            {
                try
                {
                    // Klasördeki tüm dosyaları sil
                    Directory.Delete(folderPath, true);

                    MessageBox.Show("PortCheck folder and its contents have been successfully deleted!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the folder: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("PortCheck folder not found!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
