using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace ArtShop2
{
    public partial class Form1 : Form
    {
        string connectionString = "Server=DESKTOP-EMSML\\SQLEXPRESS; Database=ArtShop;Integrated Security=true;";
        DataSet ds = new DataSet();
        SqlDataAdapter parentAdapter = new SqlDataAdapter();
        SqlDataAdapter childAdapter = new SqlDataAdapter();
        BindingSource bsParent = new BindingSource();
        BindingSource bsChild = new BindingSource();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Art Shop for broke artists";
            lblParent.Text = ConfigurationManager.AppSettings["parent"];
            lblChild.Text = ConfigurationManager.AppSettings["child"];
            deleteBtn.BackColor = Color.LightSlateGray;
            insertBtn.BackColor = Color.LightGray;
            reloadBtn.BackColor = Color.LightGray;
            updateBtn.BackColor = Color.LightGray;
            panel.BackColor = Color.LightGray;
            gridFill();
            // MessageBox.Show(ConfigurationManager.AppSettings["greeting"]);
        }


        void gridFill()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    parentAdapter.SelectCommand = new SqlCommand(
                        ConfigurationManager.AppSettings["selectParent"], connection);
                    childAdapter.SelectCommand = new SqlCommand(
                        ConfigurationManager.AppSettings["selectChild"], connection);
                    parentAdapter.Fill(ds, ConfigurationManager.AppSettings["parent"]);
                    childAdapter.Fill(ds, ConfigurationManager.AppSettings["child"]);
                    bsParent.DataSource = ds.Tables[ConfigurationManager.AppSettings["parent"]];
                    DataColumn pk = ds.Tables[ConfigurationManager.AppSettings["parent"]].Columns[
                        ConfigurationManager.AppSettings["pk"]];
                    DataColumn fk = ds.Tables[ConfigurationManager.AppSettings["child"]].Columns[
                        ConfigurationManager.AppSettings["fk"]];
                    DataRelation relation = new DataRelation("FK_Parent_Child", pk, fk);
                    ds.Relations.Add(relation);
                    bsChild.DataSource = bsParent;
                    bsChild.DataMember = "FK_Parent_Child";
                    dataGridView1.DataSource = bsParent;
                    dataGridView2.DataSource = bsChild;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void reload()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    ds.Clear();
                    parentAdapter.SelectCommand = new SqlCommand(
                        ConfigurationManager.AppSettings["selectParent"], connection);
                    childAdapter.SelectCommand = new SqlCommand(
                        ConfigurationManager.AppSettings["selectChild"], connection);
                    parentAdapter.Fill(ds, ConfigurationManager.AppSettings["parent"]);
                    childAdapter.Fill(ds, ConfigurationManager.AppSettings["child"]);
                    bsParent.DataSource = ds.Tables[ConfigurationManager.AppSettings["parent"]];
                    bsChild.DataSource = bsParent;
                    dataGridView1.DataSource = bsParent;
                    dataGridView2.DataSource = bsChild;
                    panel.Controls.Clear();
                    insertBtn.Visible = deleteBtn.Visible = updateBtn.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void generateTextBoxesForINSERT()
        {
            deleteBtn.Visible = updateBtn.Visible = false;
            panel.Controls.Clear();
            insertBtn.Visible = true;
            List<string> childColumns = new List<string>(ConfigurationManager.AppSettings["childColumns"].Split('/'));
            panel.Controls.Clear();
            int y = 50;
            Label title = new Label();
            title.Location = new Point(100, 25);
            title.Parent = panel;
            title.Text = "INSERT";
            foreach (string column in childColumns)
            {
                TextBox txtBox = new TextBox();
                txtBox.Location = new Point(110, y);
                txtBox.Name = column;
                txtBox.Parent = panel;
                Label label = new Label();
                label.Location = new Point(10, y);
                label.Text = column;
                label.Parent = panel;
                y = y + 25;

            }
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DialogResult dialog;
                    dialog = MessageBox.Show("Are you sure?", "Please confirm this operation", MessageBoxButtons.YesNoCancel);
                    if (dialog == DialogResult.Yes)
                    {
                        string childId = dataGridView2.CurrentRow.Cells[
                            ConfigurationManager.AppSettings["idChild"]].Value.ToString();
                        childAdapter.DeleteCommand = new SqlCommand(
                            ConfigurationManager.AppSettings["delete"], connection);
                        childAdapter.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int).Value = childId;
                        childAdapter.DeleteCommand.Connection = connection;
                        childAdapter.DeleteCommand.ExecuteNonQuery();
                        MessageBox.Show("Deleted successfully!");
                        reload();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void reloadBtn_Click(object sender, EventArgs e)
        {
            reload();
        }

        private void insertBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    childAdapter.InsertCommand = new SqlCommand(ConfigurationManager.AppSettings["insert"], connection);
                    List<string> stringChildColumns = new List<string>(ConfigurationManager.AppSettings["stringChildColumns"].Split('/'));
                    List<string> floatChildColumns = new List<string>(ConfigurationManager.AppSettings["floatChildColumns"].Split('/'));
                    List<string> intChildColumns = new List<string>(ConfigurationManager.AppSettings["intChildColumns"].Split('/'));


                    string parentId = dataGridView1.CurrentRow.Cells[0].Value.ToString();

                    childAdapter.InsertCommand.Parameters.Add(
                        ConfigurationManager.AppSettings["idParentinChildTable"], SqlDbType.Int).Value = parentId;

                    foreach (string column in stringChildColumns)
                    {
                        childAdapter.InsertCommand.Parameters.Add("@" + column, SqlDbType.VarChar).Value = panel.Controls[column].Text;
                    }

                    if (floatChildColumns[0] != "")
                    {
                        foreach (string column in floatChildColumns)
                        {
                            childAdapter.InsertCommand.Parameters.Add("@" + column, SqlDbType.Float).Value = float.Parse(panel.Controls[column].Text);
                        }
                    }

                    if (intChildColumns[0] != "")
                    {
                        foreach (string column in intChildColumns)
                        {
                            childAdapter.InsertCommand.Parameters.Add("@" + column, SqlDbType.Int).Value = Int32.Parse(panel.Controls[column].Text);
                        }
                    }

                    childAdapter.InsertCommand.Connection = connection;
                    childAdapter.InsertCommand.ExecuteNonQuery();
                    MessageBox.Show("ADDED!");
                    reload();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            generateTextBoxesForINSERT();
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            updateBtn.Visible=true;
            deleteBtn.Visible = insertBtn.Visible = false;
            panel.Controls.Clear();
            
            List<string> childColumns = new List<string>(ConfigurationManager.AppSettings["childColumnsUpdate"].Split('/'));
            int y = 50;
            Label title = new Label();
            title.Location = new Point(100, 25);
            title.Parent = panel;
            title.Text = "UPDATE";
            foreach (string column in childColumns)
            {
                TextBox txtBox = new TextBox();
                txtBox.Location = new Point(110, y);
                txtBox.Name = column;
                txtBox.Parent = panel;
                Label label = new Label();
                label.Location = new Point(10, y);
                label.Text = column;
                label.Parent = panel;
                y = y + 25;
                txtBox.Clear();
                txtBox.Text = dataGridView2.CurrentRow.Cells[column].Value.ToString();
            }

        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    childAdapter.UpdateCommand = new SqlCommand(
                        ConfigurationManager.AppSettings["update"], connection);
                    List<string> stringChildColumns = new List<string>(ConfigurationManager.AppSettings["stringChildColumns"].Split('/'));
                    List<string> floatChildColumns = new List<string>(ConfigurationManager.AppSettings["floatChildColumns"].Split('/'));
                    List<string> intChildColumns = new List<string>(ConfigurationManager.AppSettings["intChildColumns"].Split('/'));

                    string childId = dataGridView2.CurrentRow.Cells[0].Value.ToString();

                    childAdapter.UpdateCommand.Parameters.Add(
                        ConfigurationManager.AppSettings["idChild"], SqlDbType.Int).Value = childId;

                    foreach (string column in stringChildColumns)
                    {
                        childAdapter.UpdateCommand.Parameters.Add("@" + column, SqlDbType.VarChar).Value = panel.Controls[column].Text;
                    }

                    if (floatChildColumns[0] != "")
                    {
                        foreach (string column in floatChildColumns)
                        {
                            childAdapter.UpdateCommand.Parameters.Add("@" + column, SqlDbType.Float).Value = float.Parse(panel.Controls[column].Text);
                        }
                    }

                    if (intChildColumns[0] != "")
                    {
                        foreach (string column in intChildColumns)
                        {
                            childAdapter.UpdateCommand.Parameters.Add("@" + column, SqlDbType.Int).Value = Int32.Parse(panel.Controls[column].Text);
                        }
                    }
                    string parentId = panel.Controls[ConfigurationManager.AppSettings["idParentinChildTable"]].Text;
                    childAdapter.UpdateCommand.Parameters.Add("@"+ ConfigurationManager.AppSettings["idParentinChildTable"],
                        SqlDbType.Int).Value = Int32.Parse(parentId);
                    childAdapter.UpdateCommand.Connection = connection;
                    childAdapter.UpdateCommand.ExecuteNonQuery();
                    reload();
                    MessageBox.Show("UPDATED!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            deleteBtn.Visible = true;
            updateBtn.Visible = insertBtn.Visible = false;
            panel.Controls.Clear();
            panel.Controls.Clear();
        }
    }
}
