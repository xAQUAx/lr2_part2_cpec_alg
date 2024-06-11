using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace lr2_part2
{
    public partial class Form1 : Form
    {
        private Database DB;
        private DataTable table;
        private MySqlDataAdapter adapter;

        class Database
        {
            public String serverString;
            public String databaseString;
            public String userString;
            public String passwordString;
            public String tableString;
            public String connectionString;

            MySqlConnection connection;

            public void buildConnectionString()
            {
                connectionString = "Server=" + serverString + "; Database=" + databaseString + "; User ID=" + userString + "; Password=" + passwordString;
                connection = new MySqlConnection(connectionString);
            }
            
            public void openConnection()
            {
                if (connection.State == System.Data.ConnectionState.Closed) connection.Open();
            }

            public void closeConnection()
            {
                if (connection.State == System.Data.ConnectionState.Open) connection.Close();
            }

            public MySqlConnection GetConnection()
            {
                return connection;
            }
        }

        public Form1()
        {
            InitializeComponent();
            DB = new Database();
            table = new DataTable();
            adapter = new MySqlDataAdapter();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void clearAll()
        {
            table = new DataTable();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            clearAll();
            DB.serverString = textBox_server.Text;
            DB.databaseString = textBox_database.Text;
            DB.userString = textBox_user.Text;
            DB.passwordString = textBox_password.Text;
            DB.tableString = textBox_table.Text;
            DB.buildConnectionString();
            DB.openConnection();
            MySqlCommand command = new MySqlCommand("SELECT * FROM " + DB.databaseString + "." + DB.tableString, DB.GetConnection());
            adapter.SelectCommand = command;
            adapter.Fill(table);
            chart2.DataSource = table;
            DB.closeConnection();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            chart2.EndEdit();
            DataTable changedRows = table.GetChanges();
            if(changedRows != null)
            {
                MySqlCommandBuilder builder = new MySqlCommandBuilder(adapter);
                adapter.Update(changedRows);
                table.AcceptChanges();
                MessageBox.Show("Changes saves");
            }
        }

        private void findButton_Click(object sender, EventArgs e)
        {
            DB.openConnection();
            String findString = textBox_find.Text;
            MySqlCommand command = new MySqlCommand();
            String sqlCommand = "SELECT * FROM " + DB.databaseString + "." + DB.tableString + " WHERE ";
            String sqlParam;
            DataGridViewColumn col = chart2.Columns[0];
            sqlParam = "@" + col.Name;
            sqlCommand += col.Name + "=" + sqlParam;
            if(col.ValueType == typeof(int))
            {
                command.Parameters.Add(sqlParam, MySqlDbType.Int32).Value = Convert.ToInt32(findString);
            }
            else
            {
                command.Parameters.Add(sqlParam, MySqlDbType.VarChar).Value = findString;
            }
            command.CommandText = sqlCommand;
            command.Connection = DB.GetConnection();
            adapter.SelectCommand = command;
            clearAll();
            adapter.Fill(table);
            chart2.DataSource = table;
            DB.closeConnection();
        }

        private void resizeContent(object sender, EventArgs e)
        {
            chart2.Width = Width - (chart2.Location.X * 2) - (chart2.Margin.All * 2) - 10;
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            string product = textBox_product.Text;
            string count = textBox_count.Text;
            string buyer = textBox_buyer.Text;

            // Проверка валидности данных (например, что количество и ID — это числа)
            if (!int.TryParse(product, out int productId) || !int.TryParse(count, out int orderCount) || !int.TryParse(buyer, out int buyerId))
            {
                MessageBox.Show("Invalid input data. Please enter valid numbers for product, count, and buyer.");
                return;
            }

            try
            {
                DB.openConnection();
                MySqlCommand command = new MySqlCommand("CALL lr2_maxim.createOrder(@buyer, @product, @count);", DB.GetConnection());
                command.Parameters.AddWithValue("@buyer", buyerId);
                command.Parameters.AddWithValue("@product", productId);
                command.Parameters.AddWithValue("@count", orderCount);
                command.ExecuteNonQuery();

                MessageBox.Show("Order created successfully.");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            finally
            {
                DB.closeConnection();
            }

        }

        private void drawButton_Click(object sender, EventArgs e)
        {
            DB.openConnection();
            String axisXstring = textBox_x.Text;
            String axisYstring = textBox_y.Text;
            DB.tableString = textBox_table.Text;
            table.Clear();
            MySqlCommand command = new MySqlCommand();
            string sqlCommand = "Select " + axisXstring + "," + axisYstring + " from " + DB.tableString + ";";
            command.CommandText = sqlCommand;
            command.Connection = DB.GetConnection();
            adapter.SelectCommand = command;
            adapter.Fill(table);
            DB.closeConnection();
            chart1.Series.Clear();
            chart1.Series.Add("Chart");
            chart1.Series["Chart"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;

            ChartArea chartArea = new ChartArea();
            chart1.ChartAreas.Add(chartArea);

            Series series = new Series()
            {
                Name = "Series2",
                Color = System.Drawing.Color.Red,
                ChartType = SeriesChartType.Line
            };

            chart1.Series.Add(series);

            for (int i=0; i < table.Rows.Count; i++)
            {
                string xValue = (table.Rows[i][axisXstring]).ToString();
                double yValue = Double.Parse(table.Rows[i][axisYstring].ToString());
                chart1.Series["Chart"].Points.AddXY(xValue, yValue);
                chart1.Series["Series2"].Points.AddXY(xValue, yValue);
            }

            
        }
    }
}
