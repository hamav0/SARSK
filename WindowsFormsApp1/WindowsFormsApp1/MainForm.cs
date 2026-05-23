using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        public string connectionString { get; private set; }
        public MainForm(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            Otch();
        }
        void Otch()
        {
            string query = @"SELECT COUNT(*) 
                     FROM INFORMATION_SCHEMA.TABLES 
                     WHERE TABLE_NAME = 'Продажи';";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        button4.Enabled = (count > 0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка проверки таблицы отчётов: " + ex.Message);
                        button4.Enabled = false;
                    }
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Form nF1 = new Form1(connectionString);
            nF1.FormClosed += (s, args) => this.Visible = true;
            nF1.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Form nF2 = new Form2(connectionString);
            nF2.FormClosed += (s, args) => this.Visible = true;
            nF2.Show();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Form nF4 = new Form4(connectionString);
            nF4.FormClosed += (s, args) => this.Visible = true;
            nF4.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Form nF5 = new Form5(connectionString);
            nF5.FormClosed += (s, args) => this.Visible = true;
            nF5.Show();
        }

    }
}
