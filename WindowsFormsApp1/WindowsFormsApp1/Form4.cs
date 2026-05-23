using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form4 : Form
    {
        public string connectionString { get; private set; }

        private DataTable clientTable;
        private BindingSource clientBindingSource;

        public Form4(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            LoadDynamicData();
            CheckAndLoadDepartments();
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                using (System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Клиентская_База";

                    System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter(query, connection);
                    System.Data.DataTable table = new System.Data.DataTable();
                    adapter.Fill(table);

                    клиентBindingSource.DataSource = table;
                    данныеDataGridView.DataSource = клиентBindingSource;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }
        private void LoadDynamicData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Клиентская_База";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        clientTable = new DataTable();
                        adapter.Fill(clientTable);

                        clientBindingSource = new BindingSource();
                        clientBindingSource.DataSource = clientTable;

                        данныеDataGridView.DataSource = clientBindingSource;
                    }
                }

                listBox1.Items.Clear();
                foreach (DataColumn column in clientTable.Columns)
                {
                    listBox1.Items.Add(column.ColumnName);
                }

                foreach (DataGridViewColumn col in данныеDataGridView.Columns)
                {
                    col.HeaderText = col.Name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при динамической загрузке клиентской базы: " + ex.Message);
            }
        }

        private void CheckAndLoadDepartments()
        {
            try
            {
                bool tableExists = false;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string checkQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Отдел'";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
                    {
                        int count = (int)checkCmd.ExecuteScalar();
                        tableExists = (count > 0);
                    }
                    if (tableExists && clientTable != null && clientTable.Columns.Contains("Подразделение"))
                    {
                        string query = "SELECT Подразделение, Наименование FROM Отдел";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            SqlDataAdapter adapter = new SqlDataAdapter(command);
                            DataTable deptTable = new DataTable();
                            adapter.Fill(deptTable);

                            наименованиеComboBox.DataSource = deptTable;
                            наименованиеComboBox.DisplayMember = "Наименование";
                            наименованиеComboBox.ValueMember = "Подразделение";

                            наименованиеComboBox.Enabled = true;
                            if (наименованиеLabel != null) наименованиеLabel.Enabled = true;

                            наименованиеComboBox.SelectedIndexChanged += наименованиеComboBox_SelectedIndexChanged;
                        }
                    }
                    else
                    {
                        наименованиеComboBox.DataSource = null;
                        наименованиеComboBox.Enabled = false;
                        if (наименованиеLabel != null) наименованиеLabel.Enabled = false;
                    }
                }
            }
            catch
            {
                наименованиеComboBox.Enabled = false;
                if (наименованиеLabel != null) наименованиеLabel.Enabled = false;
            }
        }
        private void ApplyFilters()
        {
            if (клиентBindingSource == null || клиентBindingSource.DataSource == null) return;

            System.Data.DataTable table = клиентBindingSource.DataSource as System.Data.DataTable;
            if (table == null) return;

            List<string> activeFilters = new List<string>();

            if (наименованиеComboBox.SelectedIndex != -1 && наименованиеComboBox.SelectedValue != null)
            {
                string depCode = "";

                if (наименованиеComboBox.SelectedValue is System.Data.DataRowView drv)
                {
                    depCode = drv["Подразделение"].ToString().Replace("'", "''");
                }
                else
                {
                    depCode = наименованиеComboBox.SelectedValue.ToString().Replace("'", "''");
                }

                if (!string.IsNullOrEmpty(depCode))
                {
                    activeFilters.Add($"[Подразделение] = '{depCode}'");
                }
            }

            if (listBox1.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                string selectedColumn = listBox1.SelectedItem.ToString();
                string searchText = textBox1.Text.Trim().Replace("'", "''");

                if (table.Columns.Contains(selectedColumn))
                {
                    if (table.Columns[selectedColumn].DataType == typeof(string))
                    {
                        activeFilters.Add($"[{selectedColumn}] LIKE '%{searchText}%'");
                    }
                    else
                    {
                        activeFilters.Add($"CONVERT([{selectedColumn}], 'System.String') LIKE '%{searchText}%'");
                    }
                }
            }

            if (activeFilters.Count > 0)
            {
                клиентBindingSource.Filter = string.Join(" AND ", activeFilters);
            }
            else
            {
                клиентBindingSource.RemoveFilter();
            }
        }

        private void наименованиеComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (наименованиеComboBox.Focused)
            {
                ApplyFilters();
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1 && !string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Пожалуйста, выберите поле в списке для выполнения поиска.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ApplyFilters();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            listBox1.SelectedIndex = -1;

            наименованиеComboBox.SelectedIndex = -1;

            if (клиентBindingSource != null)
            {
                клиентBindingSource.RemoveFilter();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите поле в списке для выполнения сортировки/поиска.");
                return;
            }

            string selectedColumnName = listBox1.SelectedItem.ToString();
            int columnIndex = -1;

            for (int i = 0; i < данныеDataGridView.Columns.Count; i++)
            {
                if (данныеDataGridView.Columns[i].Name.Equals(selectedColumnName, StringComparison.OrdinalIgnoreCase))
                {
                    columnIndex = i;
                    break;
                }
            }

            if (columnIndex != -1)
            {
                DataGridViewColumn sortColumn = данныеDataGridView.Columns[columnIndex];
                ListSortDirection direction = ListSortDirection.Ascending;

                if (данныеDataGridView.SortedColumn == sortColumn && данныеDataGridView.SortOrder == System.Windows.Forms.SortOrder.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }

                данныеDataGridView.Sort(sortColumn, direction);

                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    string searchText = textBox1.Text.Trim();

                    for (int j = 0; j < данныеDataGridView.RowCount; j++)
                    {
                        string cellValue = данныеDataGridView[columnIndex, j].Value?.ToString();

                        if (cellValue != null && cellValue.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                        {
                            данныеDataGridView[columnIndex, j].Style.BackColor = Color.AliceBlue;
                            данныеDataGridView[columnIndex, j].Style.ForeColor = Color.Blue;
                        }
                        else
                        {
                            данныеDataGridView[columnIndex, j].Style.BackColor = данныеDataGridView.DefaultCellStyle.BackColor;
                            данныеDataGridView[columnIndex, j].Style.ForeColor = данныеDataGridView.DefaultCellStyle.ForeColor;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выбранный столбец не найден в текущей структуре таблицы.");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            Form nF2 = new Form2(connectionString);
            nF2.FormClosed += (s, args) => this.Enabled = true;
            nF2.Show();
        }
    }
}
