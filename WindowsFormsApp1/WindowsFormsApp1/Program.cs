using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Создаем и показываем форму авторизации
            Form0 loginForm = new Form0();
            if (loginForm.ShowDialog() == DialogResult.OK) // Успешная авторизация
            {
                string connectionString = loginForm.connectionString;
                Application.Run(new MainForm(connectionString));
            }
            else
            {
                // Если авторизация не удалась, завершаем приложение
                Application.Exit();
            }

        }
    }
}
