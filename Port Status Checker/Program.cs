using System;
using System.Windows.Forms;

namespace Port_Status_Checker
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Splash formunu başlat ve ShowDialog kullan
            Splash splash = new Splash();
            DialogResult result = splash.ShowDialog(); // ShowDialog() formu modal olarak açar

            // Eğer Splash formu kapandıysa, Ana formu başlat
            if (result == DialogResult.OK)
            {
                // Ana formu başlat
                Application.Run(new AnaForm());
            }
            else
            {
                // Splash formu kapanmadan uygulama kapanmışsa (alt + f4, vb.)
                Application.Exit(); // Uygulamayı sonlandır
            }
        }
    }
}
