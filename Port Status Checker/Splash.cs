using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Port_Status_Checker
{
    public partial class Splash : Form
    {
        public Splash()
        {
            InitializeComponent();
        }
        private bool isProgressCompleted = false;
        private async void Splash_Load(object sender, EventArgs e)
        {
            // Guna2ProgressIndicator'ı başlatıyoruz
            guna2WinProgressIndicator1.Start();

            // Guna2ProgressBar'ı manuel olarak animasyonlu hale getirmek için
            guna2ProgressBar1.Style = ProgressBarStyle.Continuous;
            guna2ProgressBar1.Value = 0; // Başlangıç değeri
            guna2ProgressBar1.Maximum = 100; // Maksimum değer

            // ProgressBar'ı ilerletmek için bir döngü kullanıyoruz
            for (int i = 0; i <= 100; i++)
            {
                guna2ProgressBar1.Value = i; // ProgressBar'ı ilerlet
                await Task.Delay(10); // Hızını ayarlamak için gecikme ekliyoruz
            }

            // ProgressBar dolduktan sonra işlem tamamlandı
            isProgressCompleted = true;
            guna2WinProgressIndicator1.Stop(); // ProgressIndicator'ı durdur

            // Splash ekranını kapatıyoruz
            this.DialogResult = DialogResult.OK; // DialogResult'ü OK olarak ayarlıyoruz
            this.Close(); // Formu kapatıyoruz
        }

        private void SplashForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Eğer progress bar dolmamışsa, formun kapanmasını engelle
            if (!isProgressCompleted)
            {
                e.Cancel = true; // Kapanmayı engeller


            }
        }

        // Kullanıcının pencereyi kapatmak için sağ tıklayıp pencereyi kapatmasını engellemek için
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Eğer ProgressBar dolmamışsa, pencerenin kapanmasını engelliyoruz
            if (!isProgressCompleted)
            {
                e.Cancel = true;

            }
        }

    }
}
