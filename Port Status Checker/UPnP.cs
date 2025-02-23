using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using NATUPNPLib;

namespace Port_Status_Checker
{
    public partial class UPnP : Form
    {
        private static UPnPNAT upnpNat = new UPnPNAT();
        private static IStaticPortMappingCollection mappings;

        public UPnP()
        {
            InitializeComponent();
        }

        private async void UPnP_Load(object sender, EventArgs e)
        {


            guna2ComboBox1.Items.Add("TCP");
            guna2ComboBox1.Items.Add("UDP");
            guna2ComboBox1.SelectedIndex = 0; // Varsayılan olarak TCP


            await Task.Delay(2000);
            try
            {
                mappings = upnpNat.StaticPortMappingCollection;
                if (mappings == null)
                {
                    richTextBox1.AppendText("UPnP is not supported or enabled!\n");
                }
                else
                {
                    richTextBox1.AppendText("UPnP is enabled and ready to use.\n");
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error loading UPnP mappings: {ex.Message}\n");
            }

            await UpnpLogger.LogUpnpDataAsync(richTextBox1, richTextBox2);

            await Task.Delay(5000);

            richTextBox1.Clear();


        }

        // Port açma
        public async void OpenPort(Guna.UI2.WinForms.Guna2TextBox guna2TextBox1, Guna.UI2.WinForms.Guna2TextBox guna2TextBox2,
                             Guna.UI2.WinForms.Guna2TextBox guna2TextBox3, Guna.UI2.WinForms.Guna2ComboBox guna2ComboBox1,
                             Guna.UI2.WinForms.Guna2TextBox guna2TextBox5)
        {
            try
            {
                if (mappings == null)
                {
                    richTextBox1.AppendText("UPnP is not supported or enabled!\n");
                    return;
                }

                int externalPort;
                if (!int.TryParse(guna2TextBox1.Text, out externalPort))
                {
                    richTextBox1.AppendText("Invalid external port input!\n");
                    return;
                }

                string internalIP = guna2TextBox2.Text;
                int internalPort;
                if (!int.TryParse(guna2TextBox3.Text, out internalPort))
                {
                    richTextBox1.AppendText("Invalid internal port input!\n");
                    return;
                }

                string protocol = guna2ComboBox1.Text.ToUpper(); // Correct the usage here
                string description = guna2TextBox5.Text;

                try
                {
                    // Mevcut portu kontrol et
                    var existingMapping = mappings[externalPort, protocol];
                    if (existingMapping != null)
                    {
                        // Port zaten açık, sadece işlem yapmaya devam et
                        richTextBox1.AppendText($"Port {externalPort} is already open!\n");
                        return;
                    }
                }
                catch (Exception) { }

                try
                {
                    mappings.Add(externalPort, protocol, internalPort, internalIP, true, description);

                    // Port başarıyla açıldığında, lime yeşil renk ile yazı yazdırılır.
                    richTextBox1.ForeColor = System.Drawing.Color.Lime;
                    richTextBox1.AppendText($"Port {externalPort} opened successfully!\n");
                    
                    await Task.Delay(5000); // 10 saniye bekler
                    richTextBox1.Clear();

                }
                catch (Exception ex)
                {
                    richTextBox1.AppendText($"Error opening port: {ex.Message}\n");
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error: {ex.Message}\n");
            
            }

            await UpnpLogger.LogUpnpDataAsync(richTextBox1, richTextBox2);
        }

        // UPnP'deki tüm portları kapama (Tek seferde)
        public async void CloseAllPorts()
        {
            try
            {
                if (mappings == null)
                {
                    richTextBox1.AppendText("UPnP is not supported or enabled!\n");
                    return;
                }

                richTextBox2.Clear(); // Önceden yazılanları temizle
                int portCount = mappings.Count;
                int currentPort = 1;

                // Tüm portları tek seferde kapatma ve her porttan sonra bekleme
                foreach (IStaticPortMapping mapping in mappings)
                {
                    try
                    {
                        mappings.Remove(mapping.ExternalPort, mapping.Protocol);

                        // Tüm portlar kapandıktan sonra mesaj yaz
                        if (currentPort == portCount)
                        {
                            richTextBox2.ForeColor = System.Drawing.Color.Red;
                            richTextBox2.AppendText("All UPnP ports have been closed.\n");

                            // 10 saniye bekle ve mesajı temizle
                            await Task.Delay(2000); // 10 saniye bekler
                            richTextBox2.Clear(); // Mesajı temizle
                        }
                        currentPort++;
                    }
                    catch (Exception ex)
                    {
                        richTextBox2.AppendText($"Error closing port {mapping.ExternalPort}: {ex.Message}\n");
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Error: {ex.Message}\n");
            
            }
            await UpnpLogger.LogUpnpDataAsync(richTextBox1, richTextBox2);


        }

        // Portları listeleme
        public async void ListPorts()
        {
            try
            {
                if (mappings == null)
                {
                    richTextBox2.AppendText("UPnP is not supported or enabled!\n");
                    return;
                }

                foreach (IStaticPortMapping mapping in mappings)
                {
                    string portInfo = $"Port: {mapping.ExternalPort}, Protocol: {mapping.Protocol}, Internal IP: {mapping.InternalClient}, Internal Port: {mapping.InternalPort}, Description: {mapping.Description}\n";
                    richTextBox2.AppendText(portInfo);

                    // Portlar arasında bir ayırıcı ekliyoruz.
                    richTextBox2.AppendText("---------------------------------------------------\n"); // Çizgi ekleniyor
                }
            }
            catch (Exception ex)
            {
                richTextBox2.AppendText($"Error listing ports: {ex.Message}\n");
            }
            await UpnpLogger.LogUpnpDataAsync(richTextBox1, richTextBox2);
        }


        // Button Click Events
        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            // UPnP aktifse port açma işlemini başlat
            if (mappings == null)
            {
                richTextBox1.AppendText("UPnP is not supported or enabled! Cannot open ports.\n");
                await Task.Delay(3000);
                richTextBox1.Clear();
            }
            else
            {
                richTextBox1.Clear();
                OpenPort(guna2TextBox1, guna2TextBox2, guna2TextBox3, guna2ComboBox1, guna2TextBox5); // Ensure the correct comboBox name
            }
            await UpnpLogger.LogUpnpDataAsync(richTextBox1, richTextBox2);

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            // UPnP aktifse tüm portları kapat
            CloseAllPorts();
            richTextBox1.Clear();
        }

        private async void guna2Button3_Click(object sender, EventArgs e)
        {
            // UPnP aktifse portları listele
            ListPorts();
            richTextBox1.Clear();

            await Task.Delay(15000); // 10 saniye bekler
            richTextBox2.Clear();
        }

        public static class UpnpLogger
        {
            public static async Task LogUpnpDataAsync(RichTextBox richTextBox1, RichTextBox richTextBox2)
            {
                // Asenkron olarak log kaydeder
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortCheck");

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, "PortHistory.txt");

                string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // RichTextBox içeriklerini al ve fazla boşlukları temizle
                string richTextBox1Content = richTextBox1.Text.Trim().Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
                string richTextBox2Content = richTextBox2.Text.Trim().Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

                // Tek bir tarih ile iki richTextBox içeriğini logla
                string logEntry = $"[{currentDate}] {richTextBox1Content} {richTextBox2Content}";

                try
                {
                    // StreamWriter ile asenkron yazma işlemi
                    using (StreamWriter writer = new StreamWriter(filePath, true)) // 'true' ile dosyaya ekleme yapıyoruz
                    {
                        await writer.WriteLineAsync(logEntry); // Log girişini asenkron olarak yaz
                    }
                }
                catch (Exception)
                {
                    // Hata oluştuğunda herhangi bir şey yapma
                }
            }
        }





    }
}

