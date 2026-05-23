using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Класс для хранения информации о динамически созданных сетках
        private class DynamicGridInfo
        {
            public string TableName { get; set; }
            public DataGridView Grid { get; set; }
            public RadioButton RadioBtn { get; set; }
        }

        private string connectionString;

        private List<DynamicGridInfo> dynamicGrids = new List<DynamicGridInfo>();
        private BindingSource _bindingSource = new BindingSource();
        private List<Client> clients = new List<Client>();

        public class Client
        {
            public string ClientId { get; set; }
            public string FullName { get; set; }
        }

        public Form1(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            comboBox3.AutoCompleteMode = AutoCompleteMode.Suggest;
            comboBox3.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadClientData();
            InitializeSchemaDrivenUI();
            CheckAndLoadDepartments();
            textBox1.Text = comboBox3.SelectedValue.ToString();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            Form nF2 = new Form2(connectionString);
            nF2.FormClosed += (s, args) => this.Enabled = true;
            nF2.Show();
        }
        private void CheckAndLoadDepartments()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string checkTableQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Отдел'";

                    using (SqlCommand checkCmd = new SqlCommand(checkTableQuery, connection))
                    {
                        int tableExists = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (tableExists > 0)
                        {
                            string loadQuery = "SELECT Подразделение, Наименование FROM Отдел";

                            using (SqlCommand loadCmd = new SqlCommand(loadQuery, connection))
                            {
                                SqlDataAdapter adapter = new SqlDataAdapter(loadCmd);
                                DataTable dtDepartments = new DataTable(); 
                                adapter.Fill(dtDepartments);

                                comboBox2.DataSource = dtDepartments;
                                comboBox2.DisplayMember = "Наименование";
                                comboBox2.ValueMember = "Подразделение"; 

                                comboBox2.Enabled = true;
                            }
                        }
                        else
                        {
                            comboBox2.DataSource = null;
                            comboBox2.Enabled = false; 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при проверке или загрузке подразделений: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void InitializeSchemaDrivenUI()
        {
            dynamicGrids.Clear();
            flowLayoutPanelRadioButtons.Controls.Clear();
            panelGridsContainer.Controls.Clear();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string tableQuery = @"
                        SELECT TABLE_NAME 
                        FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE COLUMN_NAME = 'Номер_заказа' 
                          AND TABLE_NAME NOT IN ('Заказ', 'tempValue', 'ErrorLog')
                        ORDER BY TABLE_NAME DESC;";

                    List<string> tables = new List<string>();
                    using (var cmd = new SqlCommand(tableQuery, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader["TABLE_NAME"].ToString());
                        }
                    }

                    // Для каждой таблицы строим UI
                    foreach (string tableName in tables)
                    {
                        // 1 Создаем RadioButton
                        RadioButton rb = new RadioButton
                        {
                            Text = tableName,
                            AutoSize = true,
                            Margin = new Padding(10, 5, 10, 5),
                            Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular)
                        };
                        flowLayoutPanelRadioButtons.Controls.Add(rb);

                        // 2 Создаем DataGridView
                        DataGridView dgv = new DataGridView
                        {
                            Dock = DockStyle.Fill,
                            Visible = false,
                            AllowUserToAddRows = true,
                            BackgroundColor = Color.White,
                            RowHeadersWidth = 25
                        };

                        dgv.DataError += (s, args) => { args.ThrowException = false; };
                        dgv.CellValueChanged += (s, e) => {
                            if (dgv.Columns[e.ColumnIndex].Name == "Стоимость")
                            {
                                CalculateTotalSum();
                            }
                        };
                        panelGridsContainer.Controls.Add(dgv);

                        var gridInfo = new DynamicGridInfo
                        {
                            TableName = tableName,
                            Grid = dgv,
                            RadioBtn = rb
                        };
                        dynamicGrids.Add(gridInfo);

                        rb.CheckedChanged += (s, args) =>
                        {
                            dgv.Visible = rb.Checked;
                            if (rb.Checked) dgv.BringToFront();
                        };

                        GenerateColumnsForGrid(dgv, tableName, connection);
                    }

                    if (dynamicGrids.Count > 0)
                    {
                        dynamicGrids[0].RadioBtn.Checked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения интерфейса: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GenerateColumnsForGrid(DataGridView dgv, string tableName, SqlConnection connection)
        {
            string columnsQuery = @"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = @TableName 
                ORDER BY ORDINAL_POSITION;";

            DataTable columnsTable = new DataTable();
            using (var cmd = new SqlCommand(columnsQuery, connection))
            {
                cmd.Parameters.AddWithValue("@TableName", tableName);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(columnsTable);
                }
            }

            foreach (DataRow row in columnsTable.Rows)
            {
                string columnName = row["COLUMN_NAME"].ToString();

                if (columnName == "Номер_заказа")
                {
                    DataGridViewTextBoxColumn hiddenCol = new DataGridViewTextBoxColumn
                    {
                        Name = columnName,
                        Visible = false
                    };
                    dgv.Columns.Add(hiddenCol);
                    continue;
                }

                string fkQuery = @"
                    SELECT 
                        OBJECT_NAME(fkc.referenced_object_id) AS RefTable,
                        rc.name AS RefColumn
                    FROM sys.foreign_key_columns fkc
                    INNER JOIN sys.columns pc ON fkc.parent_object_id = pc.object_id AND fkc.parent_column_id = pc.column_id
                    INNER JOIN sys.columns rc ON fkc.referenced_object_id = rc.object_id AND fkc.referenced_column_id = rc.column_id
                    WHERE OBJECT_NAME(fkc.parent_object_id) = @TableName AND pc.name = @ColumnName;";

                string referencedTable = null;
                string referencedColumn = null;

                using (var cmd = new SqlCommand(fkQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    cmd.Parameters.AddWithValue("@ColumnName", columnName);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            referencedTable = reader["RefTable"].ToString();
                            referencedColumn = reader["RefColumn"].ToString();
                        }
                    }
                }

                if (referencedTable != null && referencedColumn != null)
                {
                    DataGridViewComboBoxColumn comboCol = new DataGridViewComboBoxColumn
                    {
                        Name = columnName,
                        HeaderText = columnName.Replace("_ИД", ""),
                        DataPropertyName = columnName,
                        FlatStyle = FlatStyle.Flat
                    };

                    DataTable refData = new DataTable();
                    using (var cmd = new SqlCommand($"SELECT * FROM [{referencedTable}]", connection))
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(refData);
                    }

                    comboCol.DataSource = refData;
                    comboCol.ValueMember = referencedColumn; 
                    comboCol.DisplayMember = ResolveDisplayMember(refData, referencedColumn);

                    dgv.Columns.Add(comboCol);
                }
                else
                {
                    DataGridViewTextBoxColumn textCol = new DataGridViewTextBoxColumn
                    {
                        Name = columnName,
                        HeaderText = columnName,
                        DataPropertyName = columnName
                    };
                    dgv.Columns.Add(textCol);
                }
            }
        }
        private void CalculateTotalSum()
        {
            decimal totalSum = 0;

            foreach (var gridInfo in dynamicGrids)
            {
                DataGridView dgv = gridInfo.Grid;

                string priceColumnName = "";
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (col.Name.Contains("Стоимость") || col.HeaderText.Contains("Стоимость") ||
                        col.Name.Contains("Цена") || col.Name.Contains("Сумма"))
                    {
                        priceColumnName = col.Name;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(priceColumnName)) continue;

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.IsNewRow) continue;

                    object cellValue = row.Cells[priceColumnName].Value;

                    if (cellValue != null && cellValue != DBNull.Value)
                    {
                        decimal val;
                        if (decimal.TryParse(cellValue.ToString(), out val))
                        {
                            totalSum += val;
                        }
                    }
                }
            }

            textBox2.Text = totalSum.ToString("N2");
        }
        private string ResolveDisplayMember(DataTable dt, string valueMember)
        {
            string[] textNames = { "ФИО", "Название", "Наименование", "Бренд" };
            foreach (string name in textNames)
            {
                if (dt.Columns.Contains(name)) return name;
            }

            foreach (DataColumn dc in dt.Columns)
            {
                if (dc.DataType == typeof(string) && dc.ColumnName != valueMember)
                    return dc.ColumnName;
            }

            if (dt.Columns.Count > 1) return dt.Columns[1].ColumnName;

            return valueMember;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string подразделение = comboBox2.SelectedValue?.ToString();
            string типЗаказа = "ERR";
            if (comboBox1.Text == "Предварительный заказ") { типЗаказа = "1"; }
            if (comboBox1.Text == "Исполненый") { типЗаказа = "2"; }
            if (comboBox1.Text == "Отказ") { типЗаказа = "0"; }

            if (типЗаказа == "ERR")
            {
                MessageBox.Show("Выберите тип заказа");
                return;
            }

            string клиентId = textBox1.Text;
            DateTime датаИсполнения = dateTimePicker1.Value;

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                int x = 0, y = 0, z = 0;
                try
                {
                    sqlConnection.Open();

                    using (var cmdOrder = new SqlCommand("Ввод_заказ", sqlConnection))
                    {
                        cmdOrder.CommandType = CommandType.StoredProcedure;
                        if (HasProcedureParameter(sqlConnection, "Ввод_заказ", "@Подразделение"))
                        {
                            cmdOrder.Parameters.AddWithValue("@Подразделение", подразделение ?? (object)DBNull.Value);
                        }
                        cmdOrder.Parameters.AddWithValue("@Тип_заказа", типЗаказа);
                        cmdOrder.Parameters.AddWithValue("@Клиент_ИД", клиентId);
                        cmdOrder.Parameters.AddWithValue("@Дата", датаИсполнения);
                        cmdOrder.ExecuteNonQuery();
                        x++;
                    }

                    foreach (var gridInfo in dynamicGrids)
                    {
                        string procedureName = $"Ввод_{gridInfo.TableName.ToLower()}";

                        foreach (DataGridViewRow row in gridInfo.Grid.Rows)
                        {
                            if (row.IsNewRow) continue;

                            bool hasData = false;
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                if (cell.Value != null && cell.OwningColumn.Name != "Номер_заказа")
                                {
                                    hasData = true;
                                    break;
                                }
                            }
                            if (!hasData) continue;

                            using (var cmdSubItem = new SqlCommand(procedureName, sqlConnection))
                            {
                                cmdSubItem.CommandType = CommandType.StoredProcedure;

                                foreach (DataGridViewColumn col in gridInfo.Grid.Columns)
                                {
                                    if (col.Name == "Номер_заказа") continue;

                                    object val = row.Cells[col.Name].Value;
                                    cmdSubItem.Parameters.AddWithValue("@" + col.Name, val ?? DBNull.Value);
                                }

                                cmdSubItem.ExecuteNonQuery();
                                y++;
                            }
                        }
                    }

                    using (var cmdEnd = new SqlCommand("Конец", sqlConnection))
                    {
                        cmdEnd.CommandType = CommandType.StoredProcedure;
                        cmdEnd.ExecuteNonQuery();
                        z++;
                    }

                    MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка {x}{y}{z} при сохранении: {ex.Message}", "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool HasProcedureParameter(SqlConnection conn, string procName, string paramName)
        {
            string checkQuery = @"
                SELECT 1 FROM sys.parameters 
                WHERE object_id = OBJECT_ID(@ProcName) AND name = @ParamName;";
            using (var cmd = new SqlCommand(checkQuery, conn))
            {
                cmd.Parameters.AddWithValue("@ProcName", procName);
                cmd.Parameters.AddWithValue("@ParamName", paramName);
                return cmd.ExecuteScalar() != null;
            }
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных клиентов: " + ex.Message);
            }
        }

        private void comboBox3_TextChanged(object sender, EventArgs e)
        {
            string searchText = comboBox3.Text.ToLower();
            _bindingSource.Filter = $"FullName LIKE '%{searchText}%'";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedValue != null)
            {
                textBox1.Text = comboBox3.SelectedValue.ToString();
            }
            else
            {
                textBox1.Text = string.Empty;
            }
        }
    }
}
