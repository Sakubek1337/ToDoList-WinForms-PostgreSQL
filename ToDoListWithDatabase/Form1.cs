using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace ToDoListWithDatabase
{
    public partial class Form1 : Form
    {
        String tasksReadCmd = "SELECT * FROM tasks";
        String createTableTasksCmd = "CREATE TABLE IF NOT EXISTS tasks (id serial, name varchar(255), status varchar(255), is_done boolean);";
        NpgsqlConnection conn;
        String databaseURI = "Server=localhost;Port=5432;Database=csharp;User Id=postgres;Password=1234";
        NpgsqlCommand cmd;
        public Form1()
        {
            InitializeComponent();
            if (conn == null) {
                conn = new NpgsqlConnection(databaseURI);
                conn.Open();
            }
            refreshTable();
        }

        private NpgsqlCommand newCommand(String query) {
            NpgsqlCommand newcmd = conn.CreateCommand();
            newcmd.CommandType = CommandType.Text;
            newcmd.CommandText = query;
            return newcmd;
        }

        private void refreshTable() {
            cmd = newCommand(createTableTasksCmd);
            cmd.ExecuteNonQuery();
            cmd.CommandText = tasksReadCmd;
            NpgsqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                DataTable dataTable = new DataTable();
                dataTable.Load(reader);
                dataGridView1.DataSource = dataTable;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String taskName = textBox1.Text == null ? "unknown" : textBox1.Text;
            String taskStatus = textBox2.Text == null ? "unknown" : textBox2.Text;
            String isDone = checkBox1.Checked ? "true" : "false";
            String addTaskCmd = String.Format("INSERT INTO tasks (name, status, is_done) values ('{0}', '{1}', {2})", 
                taskName, taskStatus, isDone);
            cmd = newCommand(addTaskCmd);
            cmd.ExecuteNonQuery();
            refreshTable();
        }

        private bool checkTaskExistence(Int32 taskId) {
            String checkCmd = String.Format("SELECT * FROM tasks WHERE id={0};", taskId);
            cmd = newCommand(checkCmd);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            bool value = reader.HasRows;
            reader.Close();
            return value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try {
                int taskId = Convert.ToInt32(textBox3.Text);
                if (!checkTaskExistence(taskId))
                {
                    throw new Exception();
                }
                else {
                    String deleteCmd = String.Format("DELETE FROM tasks WHERE id={0};", taskId);
                    cmd = newCommand(deleteCmd);
                    cmd.ExecuteNonQuery();
                    refreshTable();
                }
            } catch (Exception exception) {
                Console.WriteLine(exception.ToString());
                MessageBox.Show("Given task ID is invalid or non-existent!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                int taskId = Convert.ToInt32(textBox6.Text);
                if (!checkTaskExistence(taskId))
                {
                    throw new Exception();
                }
                else
                {
                    String readCmd = String.Format("SELECT name, status FROM tasks WHERE id={0}", taskId);
                    cmd = newCommand(readCmd); 
                    NpgsqlDataReader reader = cmd.ExecuteReader();
                    reader.Read();
                    String oldName = reader[0].ToString();
                    String oldStatus = reader[1].ToString();
                    reader.Close(); 
                    
                    String taskName = textBox4.Text.Trim() == "" ? oldName : textBox4.Text;
                    String taskStatus = textBox5.Text.Trim() == "" ? oldStatus : textBox5.Text;
                    String isDone = checkBox2.Checked ? "true" : "false";
                    String updateTaskCmd = String.Format("UPDATE tasks SET name='{0}', status='{1}', is_done={2} WHERE id={3};",
                        taskName, taskStatus, isDone, taskId);
                    cmd = newCommand(updateTaskCmd);
                    cmd.ExecuteNonQuery();
                    refreshTable();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                MessageBox.Show("Given task ID is invalid or non-existent!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
    }
}
