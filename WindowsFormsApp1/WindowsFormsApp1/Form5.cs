using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form5 : Form
    {
        public string connectionString { get; private set; }

        public class Client
        {
            public string ClientId { get; set; }
            public string FullName { get; set; }
        }

        private List<Client> clients = new List<Client>();
        private BindingSource _bindingSource = new BindingSource();

        public Form5(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;

            textBox1.DataBindings.Add("Text", _bindingSource, "ClientId");

            comboBox3.AutoCompleteMode = AutoCompleteMode.Suggest;
            comboBox3.AutoCompleteSource = AutoCompleteSource.CustomSource;

            LoadClientData();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            продажиDataGridView.DefaultCellStyle.Font = new Font("Times New Roman", 11, FontStyle.Regular);
        }

        private void LoadClientData()
        {
            clients.Clear();
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "SELECT Клиент_ИД, Фамилия, Имя, Отчество FROM Клиентская_База";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new Client
                            {
                                ClientId = reader["Клиент_ИД"].ToString(),
                                FullName = $"{reader["Фамилия"]} {reader["Имя"]} {reader["Отчество"]}"
                            });
                        }
                    }

                    AutoCompleteStringCollection autoCompleteCollection = new AutoCompleteStringCollection();
                    foreach (var client in clients)
                    {
                        autoCompleteCollection.Add(client.FullName);
                    }

                    comboBox3.AutoCompleteCustomSource = autoCompleteCollection;
                    _bindingSource.DataSource = clients;
                    comboBox3.DataSource = _bindingSource;
                    comboBox3.DisplayMember = "FullName";
                    comboBox3.ValueMember = "ClientId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных клиентов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Выберите клиента из списка для генерации отчета.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand("Продажи_ввод", sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;

                        sqlCommand.Parameters.AddWithValue("@период_от", dateTimePicker1.Value.Date);
                        sqlCommand.Parameters.AddWithValue("@период_до", dateTimePicker2.Value.Date);
                        sqlCommand.Parameters.AddWithValue("@Клиент_ИД", textBox1.Text);

                        sqlCommand.ExecuteNonQuery();
                    }

                    string selectQuery = "SELECT Пол, Возраст, Услуга, Повторяемость, Средний_чек, Источник_обращения FROM Продажи";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, sqlConnection))
                    {
                        DataTable reportTable = new DataTable();
                        adapter.Fill(reportTable);

                        продажиDataGridView.DataSource = reportTable;
                    }
                }

                продажиDataGridView.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при вызове процедуры отчета: {ex.Message}", "Ошибка выполнения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker2.MinDate = dateTimePicker1.Value;
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker1.MaxDate = dateTimePicker2.Value;
        }

        private void comboBox3_TextChanged(object sender, EventArgs e)
        {
            string searchText = comboBox3.Text.ToLower();
            _bindingSource.Filter = $"FullName LIKE '%{searchText}%'";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
