using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public string connectionString { get; private set; }

        private Dictionary<string, Control> parameterControls = new Dictionary<string, Control>();

        private FlowLayoutPanel mainFlowPanel;

        public Form2(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            InitializeDynamicLayout();
            BuildDynamicUI();
        }

        private void InitializeDynamicLayout()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl != button1 && ctrl != button2 && ctrl != label11)
                {
                    ctrl.Visible = false;
                }
            }

            mainFlowPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = this.Height - 90,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(20),
                BackColor = SystemColors.Control
            };
            this.Controls.Add(mainFlowPanel);

            button2.Top = this.Height - 80;
            button1.Top = this.Height - 80;
            button2.BringToFront();
            button1.BringToFront();
        }

        private void BuildDynamicUI()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT PARAMETER_NAME, DATA_TYPE 
                        FROM INFORMATION_SCHEMA.PARAMETERS 
                        WHERE SPECIFIC_NAME = 'Ввод_клиент' AND PARAMETER_MODE = 'IN'";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string paramName = reader["PARAMETER_NAME"].ToString();
                                string dataType = reader["DATA_TYPE"].ToString();

                                CreateControlForParameter(paramName, dataType);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при автоматической генерации интерфейса: " + ex.Message);
            }
        }

        private void CreateControlForParameter(string paramName, string dataType)
        {
            string displayName = paramName.Replace("@", "").Replace("_", " ");

            if (!string.IsNullOrEmpty(displayName))
                displayName = char.ToUpper(displayName[0]) + displayName.Substring(1);

            Panel rowPanel = new Panel { Width = 460, Height = 35, Margin = new Padding(0, 3, 0, 3) };

            Label lbl = new Label
            {
                Text = displayName + ":",
                Location = new Point(0, 10),
                Width = 150,
                Font = new Font("Times New Roman", 12, FontStyle.Regular)
            };
            rowPanel.Controls.Add(lbl);

            string lowerParam = paramName.ToLower();

            if (lowerParam == "@пол")
            {
                Panel genderPanel = new Panel { Location = new Point(160, 5), Width = 200, Height = 30 };
                RadioButton rbMale = new RadioButton { Text = "Мужской", Checked = true, Location = new Point(0, 5), Width = 90, Tag = "м" };
                RadioButton rbFemale = new RadioButton { Text = "Женский", Location = new Point(90, 5), Width = 90, Tag = "ж" };

                genderPanel.Controls.Add(rbMale);
                genderPanel.Controls.Add(rbFemale);
                rowPanel.Controls.Add(genderPanel);

                parameterControls.Add(paramName, genderPanel);
            }
            else if (lowerParam == "@дата_рождения")
            {
                Panel datePanel = new Panel { Location = new Point(160, 5), Width = 300, Height = 30 };
                DateTimePicker dtp = new DateTimePicker { Location = new Point(0, 2), Width = 130, Format = DateTimePickerFormat.Short };
                CheckBox cbNoDate = new CheckBox { Text = "Не указывать", Location = new Point(145, 5), Width = 120 };

                cbNoDate.CheckedChanged += (s, e) => { dtp.Enabled = !cbNoDate.Checked; };

                datePanel.Controls.Add(dtp);
                datePanel.Controls.Add(cbNoDate);
                rowPanel.Controls.Add(datePanel);

                parameterControls.Add(paramName, datePanel);
            }
            else if (lowerParam == "@подразделение")
            {
                ComboBox cmb = new ComboBox { Location = new Point(160, 7), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Times New Roman", 12, FontStyle.Regular) };
                LoadDepartmentsData(cmb);
                rowPanel.Controls.Add(cmb);

                parameterControls.Add(paramName, cmb);
            }
            else
            {
                TextBox txt = new TextBox { Location = new Point(160, 5), Width = 250 };

                if (lowerParam.Contains("телефон"))
                {
                    txt.KeyPress += (s, e) =>
                    {
                        if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                            e.Handled = true;
                    };
                }

                rowPanel.Controls.Add(txt);
                parameterControls.Add(paramName, txt);
            }

            mainFlowPanel.Controls.Add(rowPanel);
        }

        private void LoadDepartmentsData(ComboBox cmb)
        {
            try
            {
                cmb.BindingContext = this.BindingContext;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string queryData = "SELECT Подразделение AS [Подразделение], Наименование AS [Наименование] FROM Отдел";

                    SqlDataAdapter da = new SqlDataAdapter(queryData, connection);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cmb.DisplayMember = "Наименование";
                    cmb.ValueMember = "Подразделение";
                    cmb.DataSource = dt;
                }
            }
            catch
            {
                cmb.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("Ввод_клиент", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        foreach (var pair in parameterControls)
                        {
                            string paramName = pair.Key;
                            Control ctrl = pair.Value;
                            string lowerParam = paramName.ToLower();

                            if (lowerParam == "@пол")
                            {
                                string gender = "м";
                                foreach (Control c in ctrl.Controls)
                                {
                                    if (c is RadioButton rb && rb.Checked)
                                    {
                                        gender = rb.Tag.ToString();
                                        break;
                                    }
                                }
                                command.Parameters.AddWithValue(paramName, gender);
                            }
                            else if (lowerParam == "@дата_рождения")
                            {
                                DateTimePicker dtp = null;
                                CheckBox cb = null;
                                foreach (Control c in ctrl.Controls)
                                {
                                    if (c is DateTimePicker d) dtp = d;
                                    if (c is CheckBox b) cb = b;
                                }

                                if (cb != null && cb.Checked)
                                    command.Parameters.AddWithValue(paramName, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(paramName, dtp.Value.Date);
                            }
                            else if (lowerParam == "@подразделение")
                            {
                                ComboBox cmb = (ComboBox)ctrl;
                                if (cmb.Enabled && cmb.SelectedValue != null)
                                    command.Parameters.AddWithValue(paramName, cmb.SelectedValue);
                                else
                                    command.Parameters.AddWithValue(paramName, DBNull.Value);
                            }
                            else
                            {
                                TextBox txt = (TextBox)ctrl;
                                if (string.IsNullOrWhiteSpace(txt.Text))
                                    command.Parameters.AddWithValue(paramName, DBNull.Value);
                                else
                                    command.Parameters.AddWithValue(paramName, txt.Text);
                            }
                        }

                        command.ExecuteNonQuery();
                        MessageBox.Show("Клиент успешно добавлен в базу!");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных клиента: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
