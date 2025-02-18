using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;



namespace Port_Status_Checker
{
    public partial class PortCheckForm1 : Form

    {
        private bool firewallStatusLoaded = false; // Form load sırasında mesaj göstermemek için bir kontrol flag'i
        private string lastLoggedContent = "";
        public PortCheckForm1()
        {
            InitializeComponent();

        }

        private async Task<bool> CheckTCPPortAsync(string ip, int port)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    var connectTask = tcpClient.ConnectAsync(ip, port);
                    if (await Task.WhenAny(connectTask, Task.Delay(3000)) == connectTask)
                    {
                        return true; // Bağlandı
                    }
                    else
                    {
                        return false; // Zaman aşımına uğradı
                    }
                }
            }
            catch (SocketException)
            {
                return false;
            }
        }

        // TCP Listener eklenmiş versiyon
        private void StartTCPListener(int port)
        {
            Task.Run(() =>
            {
                try
                {
                    TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
                    tcpListener.Start();
                    

                    while (true)
                    {
                        TcpClient client = tcpListener.AcceptTcpClient();
                        NetworkStream stream = client.GetStream();
                        byte[] responseBytes = Encoding.ASCII.GetBytes("Hello from TCP server");
                        stream.Write(responseBytes, 0, responseBytes.Length);
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                  
                }
            });
        }


        private bool CheckUDPPort(string ip, int port)
        {
            try
            {
                Task.Run(() => StartUDPListener(ip, port));

                using (UdpClient udpClient = new UdpClient())
                {
                    udpClient.Client.ReceiveTimeout = 2000;
                    udpClient.Connect(ip, port);

                    byte[] sendBytes = Encoding.ASCII.GetBytes("PING");
                    udpClient.Send(sendBytes, sendBytes.Length);

                    System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);
                    try
                    {
                        byte[] receiveBytes = udpClient.Receive(ref remoteEndPoint);
                        string receivedMessage = Encoding.ASCII.GetString(receiveBytes);

                        return receivedMessage == "PONG";
                    }
                    catch (SocketException)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void StartUDPListener(string ip, int port)
        {
            using (UdpClient udpListener = new UdpClient(port))
            {
                System.Net.IPEndPoint endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, port);

                while (true)
                {
                    try
                    {
                        byte[] receivedBytes = udpListener.Receive(ref endPoint);
                        string receivedData = Encoding.ASCII.GetString(receivedBytes);

                        if (receivedData == "PING")
                        {
                            byte[] responseBytes = Encoding.ASCII.GetBytes("PONG");
                            udpListener.Send(responseBytes, responseBytes.Length, endPoint);
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }

        private async Task<bool> IsInternetAvailable()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("https://www.google.com");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void guna2Button1_Click(object sender, EventArgs e)
        {
            bool isInternetAvailable = await IsInternetAvailable();
            if (!isInternetAvailable)
            {
                MessageBox.Show("No internet connection available. Please check your connection.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!guna2RadioButton1.Checked && !guna2RadioButton2.Checked)
            {
                MessageBox.Show("Please select TCP or UDP to proceed.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string input = guna2TextBox1.Text;
            if (!int.TryParse(guna2TextBox2.Text, out int port) || port < 1 || port > 65535)
            {
                richTextBox1.Text = "Invalid port number! Please provide a port between 1 and 65535!";
                richTextBox1.ForeColor = System.Drawing.Color.Red;
                return;
            }

            richTextBox1.Text = "";
            guna2Button1.Enabled = false;

            string ip;
            if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
            {
                try
                {
                    Uri uri = new Uri(input);
                    ip = Dns.GetHostEntry(uri.Host).AddressList[0].ToString();
                }
                catch
                {
                    richTextBox1.Text = "Invalid URL!";
                    richTextBox1.ForeColor = System.Drawing.Color.Red;
                    guna2Button1.Enabled = true;
                    return;
                }
            }
            else if (Uri.CheckHostName(input) == UriHostNameType.Dns)
            {
                try
                {
                    ip = Dns.GetHostEntry(input).AddressList[0].ToString();
                }
                catch
                {
                    richTextBox1.Text = "Invalid domain name!";
                    richTextBox1.ForeColor = System.Drawing.Color.Red;
                    guna2Button1.Enabled = true;
                    return;
                }
            }
            else
            {
                ip = input;
            }
            StartTCPListener(port); // TCP dinleyiciyi başlat
            Task.Run(async () =>
            {
                bool isPortOpen = false;
                string protocol = guna2RadioButton1.Checked ? "TCP" : "UDP";

                this.Invoke((Action)(() =>
                {
                    richTextBox1.Text = $"Status: Checking Port...{Environment.NewLine}IP: {ip}{Environment.NewLine}Protocol: {protocol}{Environment.NewLine}Port: {port}";
                    richTextBox1.ForeColor = System.Drawing.Color.Blue;
                }));

                await Task.Delay(500);

                isPortOpen = guna2RadioButton1.Checked ? await CheckTCPPortAsync(ip, port) : CheckUDPPort(ip, port);

                this.Invoke((Action)(() =>
                {
                    string status = isPortOpen ? "Open" : "Closed";
                    System.Drawing.Color statusColor = isPortOpen ? System.Drawing.Color.FromArgb(0, 255, 0) : System.Drawing.Color.Red;

                    richTextBox1.Text = $"IP: {ip}{Environment.NewLine}Protocol: {protocol}{Environment.NewLine}Port: {port}{Environment.NewLine}Status: {status}";
                    richTextBox1.ForeColor = statusColor;

                    guna2Button1.Enabled = true;
                }));
            });
        }




        

        private async void guna2Button2_Click(object sender, EventArgs e)
        {
            // Dış IP'yi alalım
            string externalIP = await GetExternalIPAddress();

            // IP'yi TextBox1'e yazalım
            guna2TextBox1.Text = externalIP;
        }

        private async Task<string> GetExternalIPAddress()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // ipify API'sinden dış IP'yi alıyoruz
                    HttpResponseMessage response = await client.GetAsync("https://api.ipify.org?format=text");
                    if (response.IsSuccessStatusCode)
                    {
                        string ip = await response.Content.ReadAsStringAsync();
                        return ip;
                    }
                    else
                    {
                        // IP almak başarısız olduğunda
                        return "Failed to get IP";
                    }
                }
            }
            catch (HttpRequestException)
            {
                // İnternet bağlantısı yoksa hata mesajı veriyoruz
                MessageBox.Show("No internet connection available. Please check your connection.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }





        private void ToggleFirewall(bool enable)
        {
            string state = enable ? "on" : "off";
            string command = $"netsh advfirewall set allprofiles state {state}";

            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                Verb = "runas" // Yönetici olarak çalıştır
            };

            try
            {
                Process process = Process.Start(psi);
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void PortCheckForm1_Load(object sender, EventArgs e)
        {

            // Private ve Public profillerinin durumlarını kontrol et
            // Profil durumlarını kontrol et ve label6'ya yazdır
            // Profil durumlarını kontrol et ve label6'ya yazdır



            // Başlangıçta richTextBox'ı temizleyelim
            richTextBox2.Clear();
            richTextBox2.Visible = false; // Başlangıçta görünmesin




            // Arka planda firewall durumunu kontrol et
            bool firewallStatus = await Task.Run(() => GetFirewallStatus());



            // İlk switch'in durumunu ayarla (Açık/Kapalı)

            guna2ToggleSwitch2.Checked = firewallStatus; // Diğer switch tersini alır

            // Firewall durumunu kontrol ettikten sonra, doğru state'leri ayarlamak için her iki switch'i de sıfırlıyoruz
            firewallStatusLoaded = true; // Form yüklendi, şimdi mesaj gösterebiliriz
                                         // Arka planda firewall durumunu kontrol e



        }

        private bool GetFirewallStatus()
        {
            string command = "netsh advfirewall show allprofiles state";
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            try
            {
                Process process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // "State ON" veya "State OFF" kelimelerinin varlığını kontrol et
                if (output.Contains("State ON"))
                {
                    return true; // Firewall açık
                }
                else if (output.Contains("State OFF"))
                {
                    return false; // Firewall kapalı
                }
                else
                {
                    return false; // Varsayılan olarak kapalı kabul edebiliriz
                }
            }
            catch (Exception)
            {
                return false; // Hata durumunda firewall kapalı kabul edilir
            }
        }

        private void guna2ToggleSwitch2_CheckedChanged(object sender, EventArgs e)
        {
            if (!firewallStatusLoaded) return;

            bool enable = guna2ToggleSwitch2.Checked; // Is it ON or OFF?
            if (enable)
            {


                // Diğer switch'i kapat


            }
            if (enable)
            {
                // Firewall'ı kapat
                ToggleFirewall(false);

                // label6'ya durumu yazdır ve rengi değiştir
                label6.Text = "Windows Defender Firewall is now disabled.";
                label6.ForeColor = Color.Red;  // Kırmızı renk
            }
            else
            {
                // Firewall'ı aç
                ToggleFirewall(true);

                // label6'ya durumu yazdır ve rengi değiştir
                label6.Text = "Windows Defender Firewall is now enabled.";
                label6.ForeColor = Color.Green;  // Yeşil renk
            }
            SaveFirewallStatus();

        }



        private string GetFirewallStatus14()
        {
            try
            {
                string command = "netsh advfirewall show allprofiles state";
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C " + command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using (Process process = Process.Start(psi))
                {
                    if (process == null)
                        return "ERROR"; // Process başlamadı

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Satırları tek tek oku
                    string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        if (line.Contains("State"))
                        {
                            // "State" kelimesinden sonra "ON" geçiyor mu?
                            if (line.Contains("ON"))
                                return "ON";
                            else if (line.Contains("OFF"))
                                return "OFF";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "ERROR"; // Hata varsa
            }

            return "ERROR"; // Varsayılan hata durumu
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            string firewallStatus = GetFirewallStatus14(); // Durumu al

            if (firewallStatus == "ON")
            {
                label6.Text = "Windows Defender Firewall is enabled.";
                label6.ForeColor = Color.Green; // Yeşil
            }
            else if (firewallStatus == "OFF")
            {
                label6.Text = "Windows Defender Firewall is disabled.";
                label6.ForeColor = Color.Red; // Kırmızı
            }
            else
            {
                label6.Text = "Firewall status unknown.";
                label6.ForeColor = Color.Gray; // Gri (Hata durumu)
            }
            SaveFirewallStatus();

        }


        private void SavePortHistory(string decodedContent)
        {
            try
            {
                // Kullanıcı dizininde "Documents" klasörüne kaydetmek için dizin yolu
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortCheck");

                // Klasörün var olup olmadığını kontrol et
                if (!Directory.Exists(directoryPath))
                {
                    // Klasör yoksa oluştur
                    Directory.CreateDirectory(directoryPath);
                }

                // Kayıt dosyasının yolunu belirle
                string filePath = Path.Combine(directoryPath, "PortHistory.txt");

                // Şu anki tarihi al
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Veriyi formatlayarak ekle
                StringBuilder logBuilder = new StringBuilder();
                logBuilder.AppendLine($"[{currentDate}] {decodedContent.Replace("\n", " ")}");

                // Dosyaya ekleme işlemi
                File.AppendAllText(filePath, logBuilder.ToString());
            }
            catch (Exception)
            {
                // Hata oluşursa bile kullanıcıya herhangi bir bildirim vermez.
            }
        }

        private void SaveFirewallStatus()
        {
            try
            {
                // Kullanıcı dizininde "Documents" klasörüne kaydetmek için dizin yolu
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortCheck");

                // Klasörün var olup olmadığını kontrol et
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath); // Klasör yoksa oluştur
                }

                // Kayıt dosyasının yolunu belirle
                string filePath = Path.Combine(directoryPath, "PortHistory.txt");

                // Şu anki tarihi al
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // Firewall durumunu kaydet
                string logEntry = $"[{currentDate}] Firewall Status: {label6.Text}";

                // Dosyaya ekleme işlemi
                File.AppendAllText(filePath, logEntry + Environment.NewLine);
            }
            catch (Exception)
            {
                // Hata oluşursa bile kullanıcıya herhangi bir bildirim vermez.
            }
        }






        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            // Kullanıcı tarafından girilen veriyi al
            string inputText = richTextBox1.Text.Trim();

            if (!string.IsNullOrWhiteSpace(inputText))
            {
                try
                {
                    // Eğer metin Base64 formatındaysa, çözümle
                    byte[] decodedBytes = Convert.FromBase64String(inputText);
                    string decodedText = Encoding.UTF8.GetString(decodedBytes);

                    // "Checking" kelimesi içeren satırları engelle
                    if (!decodedText.Contains("Checking"))
                    {
                        // Çözülen veriyi kontrol et, son kaydedilenle aynı değilse kaydet
                        if (decodedText != lastLoggedContent)
                        {
                            SavePortHistory(decodedText);
                            lastLoggedContent = decodedText; // Yeni kaydedilen içeriği sakla
                        }
                    }
                }
                catch (FormatException)
                {
                    // Eğer Base64 değilse, olduğu gibi kaydet
                    if (inputText != lastLoggedContent && !inputText.Contains("Checking"))
                    {
                        SavePortHistory(inputText);
                        lastLoggedContent = inputText; // Yeni kaydedilen içeriği sakla
                    }
                }
            }
        }

        private async void guna2CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (guna2CheckBox1.Checked)
            {
                // Yerel IP, Public IP, ISP ve Hostname bilgilerini al

                string publicIP = await GetPublicIPAddress(); // Public IP
                string isp = await GetISP(publicIP); // ISP
                string country = await GetCountryInfo(publicIP); // Asenkron Ülke bilgisi


                // RichTextBox'ı görünür hale getirelim
                richTextBox2.Visible = true;
                // RichTextBox içine bilgileri yazdıralım
                richTextBox2.Clear(); // Önceden yazılanları temizleyelim
                richTextBox2.AppendText("Country: " + country + "\n"); // Ülke bilgisini yazdırıyoruz
                richTextBox2.AppendText("Public IP: " + publicIP + "\n");
                richTextBox2.AppendText("ISP: " + isp + "\n");


            }
            else
            {
                // Checkbox işareti kaldırıldığında richTextBox'ı gizle
                richTextBox2.Visible = false;
            }
        }
        private async Task<string> GetCountryInfo(string publicIP)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string jsonResponse = await client.GetStringAsync($"https://ipinfo.io/{publicIP}/json");

                // JSON yanıtını işleyerek ülke bilgisini al
                dynamic data = JsonConvert.DeserializeObject(jsonResponse);
                string country = data.country; // Tam ülke ismi (örneğin "Turkey" yerine "Türkiye")

                return country; // Ülke ismini döndür
            }
        }


        private async Task<string> GetISP(string publicIP)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string jsonResponse = await client.GetStringAsync($"https://ipinfo.io/{publicIP}/json");

                // JSON yanıtını işleyerek ISP bilgisini al
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
                string isp = data.org; // ISP bilgisi genellikle "org" alanında olur
                return isp;
            }
        }

        private async Task<string> GetPublicIPAddress()
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string publicIP = await client.GetStringAsync("https://api.ipinfo.io/ip");
                return publicIP.Trim(); // API'den gelen IP
            }
        }

    }
}

        
