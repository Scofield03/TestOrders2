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

namespace TestOrders2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            Loading f = new Loading();
            f.Show();
            f.Refresh();
            InitializeComponent();
            if (SQLInit())
                GetData();
            f.Close();
        }
        private string path = Application.StartupPath;
        private readonly string _sqlConnectionString2 = "Data Source={0};Integrated Security=SSPI;Persist Security Info=False;Encrypt=False;Application Name={1}";
        public static string Connection = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\DBProduction.mdf;Initial Catalog=DBProduction;Trusted_Connection=True;Encrypt=False";
        private readonly int _commandTimeOut;
        public static readonly String ExceptionFormatString = "Exception message: {0}{1}Exception Source: {2}{1}Exception StackTrace: {3}{1}";
        public bool SQLInit()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(Connection))
                //using (var sqlCommand = new SqlCommand("", sqlConnection) { CommandTimeout = _commandTimeOut })
                {
                    sqlConnection.Open();
                    label3.ForeColor = Color.ForestGreen;
                    label3.Text = "Успешное соединение с БД";
                    return true;
                }
            }
            catch (Exception ex)
            {
                String exDetail = String.Format(ExceptionFormatString, ex.Message, Environment.NewLine, ex.Source, ex.StackTrace);
                //throw new Exception($"Error executing SQLInit: " + exDetail);
                label3.ForeColor = Color.IndianRed;
                label3.Text = "Отсутствует связь с БД";
                MessageBox.Show(/*exDetail*/ex.ToString());
                return false;
            }
        }
        public struct Prod
        {
            public int id;    
            public string name;
            public bool isKit;
            public int cnt;
        }

        public Prod[] prods = new Prod[0];
        //public static string Connection = "Data Source=PK-TEST\\SQLEXPRESS;Initial Catalog=DBProduction;Trusted_Connection=True;Encrypt=False";
        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBProductionDataSet.WhatInStock1". При необходимости она может быть перемещена или удалена.
            this.whatInStock1TableAdapter.Fill(this.dBProductionDataSet.WhatInStock1);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBProductionDataSet.OrderList1". При необходимости она может быть перемещена или удалена.
            this.orderList1TableAdapter.Fill(this.dBProductionDataSet.OrderList1);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBProductionDataSet.WhatInStock". При необходимости она может быть перемещена или удалена.
            this.whatInStockTableAdapter.Fill(this.dBProductionDataSet.WhatInStock);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBProductionDataSet.Statuses". При необходимости она может быть перемещена или удалена.
            this.statusesTableAdapter.Fill(this.dBProductionDataSet.Statuses);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBProductionDataSet.ProdList". При необходимости она может быть перемещена или удалена.
            this.prodListTableAdapter.Fill(this.dBProductionDataSet.ProdList);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBProductionDataSet.ProdsInAllAreas". При необходимости она может быть перемещена или удалена.
            this.prodsInAllAreasTableAdapter.Fill(this.dBProductionDataSet.ProdsInAllAreas);
            // TODO: данная строка кода позволяет загрузить данные в таблицу "dBProductionDataSet.OrderList". При необходимости она может быть перемещена или удалена.
            this.orderListTableAdapter.Fill(this.dBProductionDataSet.OrderList);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
                MessageBox.Show("Внимание! Вы не выбрали ни одной позиции для оформления заказа", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                if (MessageBox.Show("Вы уверены, что хотите оформить новый заказ с выбранным переченем позиций?", "Сообщение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                var ordid = GetNewOrdID();
                if(ordid>0)
                //MessageBox.Show("OK");
                for (int i = 0; i < prods.Length; i++)
                    AddNewOrder(prods[i],ordid);

                /*this.orderListTableAdapter.Fill(this.dBProductionDataSet.OrderList);
                dataGridView1.Update();*/
                GetData();

                button4_Click(sender, e);
            }
                /*else
                    MessageBox.Show("No");*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool fnd = false;
            string str = comboBox1.Text.Substring(0, comboBox1.Text.Length - 2);
            string del = comboBox1.Text.Substring(comboBox1.Text.Length - 1, 1);

            foreach (var item in listBox1.Items) 
            {
                if (item.ToString().Contains(str))
                {
                    listBox1.Items.Remove(item);
                    fnd = true;
                    break;
                }
            }
            listBox1.Items.Add(str + "*"+numericUpDown1.Value.ToString());
            if (!fnd)
            {
                del = comboBox1.Text.Substring(comboBox1.Text.Length - 1, 1);
                Array.Resize(ref prods, prods.Length + 1);
                prods[prods.Length - 1].id = Convert.ToInt32(comboBox1.SelectedValue);
                prods[prods.Length - 1].name = str;
                if (del == "1")
                    prods[prods.Length - 1].isKit = true;
                else
                    prods[prods.Length - 1].isKit = false;
                prods[prods.Length - 1].cnt = Convert.ToInt32(numericUpDown1.Value);
            }
          // MessageBox.Show(prods[prods.Length - 1].name);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex >= 0)
                button3.Enabled = true;
            else
                button3.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool fnd = false;
           // MessageBox.Show(listBox1.Text);
            if (listBox1.SelectedIndex >= 0)
            {
                foreach (var pr in prods)
                {
                    if (listBox1.Text.Contains(pr.name))
                    {                       
                        
                        prods = prods.Where(val  => val.id != pr.id).ToArray();
                            //Array.Resize(ref prods, prods.Length - 1);                        
                        break;
                    }
                }
                listBox1.Items.Remove(listBox1.Items[listBox1.SelectedIndex]);
                //MessageBox.Show(prods.Length.ToString());
            }
        }

        private int GetNewOrdID()
        {
            //MessageBox.Show(pr.name+"|"+pr.cnt.ToString()+"|"+pr.isKit.ToString());
           
            string SQL = "SELECT [dbo].[GetNewOrderID] ()";
            //int val = 0;
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand(SQL, conn);
            //cmd.CommandType = System.Data.CommandType.StoredProcedure;

            try
            {
                conn.Open();
                object val= cmd.ExecuteScalar();
               // MessageBox.Show(val.ToString());
                return Convert.ToInt32(val);
            }
            catch (Exception ex) {  MessageBox.Show(ex.ToString()); return - 1; }
            finally { conn.Close(); }
            //return true;
        }

        private bool AddNewOrder (Prod pr, int idord)
        {
            //MessageBox.Show(pr.name+"|"+pr.cnt.ToString()+"|"+pr.isKit.ToString());
            
            string SQL = "AddNewOrder";
            
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand(SQL, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
         
            cmd.Parameters.AddWithValue("@isKit", pr.isKit);
            cmd.Parameters.AddWithValue("@cnt", pr.cnt);
            cmd.Parameters.AddWithValue("@ID_Prod", pr.id);
            cmd.Parameters.AddWithValue("@ID_Order", idord);
            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex) { MessageBox.Show(ex.ToString()); return false; }
            finally { conn.Close(); }
            return true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            Array.Clear(prods,0, prods.Length);
            Array.Resize(ref prods, 0);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите передать выбранный заказ в работу?", "Сообщение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {

            }
        }
        private bool ChangeOrderStatus(int idord, int idstst)
        {           
            string SQL = "ChangeOrderStatus";
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand(SQL, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@ID_Status", idstst);
            cmd.Parameters.AddWithValue("@ID_Order", idord);
            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); return false; }
            finally { conn.Close(); }
            return true;           
        }

        private bool GetOrderLog(int idord)
        {
            string SQL = "GetOrderLog";
            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand(SQL, conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ID_Order", idord);
            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                var dtgr = new DataTable();
                dtgr.Load(reader);
                dataGridView6.DataSource = dtgr;
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); return false; }
            finally { conn.Close(); }
            return true;
        }
        /*
        private SendToReserv(int idord,int cnt,int idprod)
        {

        }*/

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                dataGridView6.DataSource = null;
                GetOrderLog(Convert.ToInt32(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells["ID_Order"].Value));
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex >= 0 && dataGridView1.SelectedCells.Count > 0)
                if (comboBox2.Text == dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells["статусDataGridViewTextBoxColumn"].Value.ToString())
                    MessageBox.Show("Внимание! Выбранный статус совпадает с текущим", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    ChangeOrderStatus(Convert.ToInt32(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells["ID_Order"].Value), Convert.ToInt32(comboBox2.SelectedValue));
                    this.orderListTableAdapter.Fill(this.dBProductionDataSet.OrderList);
                    dataGridView1.Update();                   
                }
        }

        private void dataGridView3_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            object head = this.dataGridView3.Rows[e.RowIndex].HeaderCell.Value;
            if (head == null ||
                !head.Equals((e.RowIndex + 1).ToString()))
                this.dataGridView3.Rows[e.RowIndex].HeaderCell.Value =
                    (e.RowIndex + 1).ToString();
            dataGridView3.RowHeadersWidth = 60;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView7.SelectedCells.Count > 0)
            {
                int whantcnt = Convert.ToInt32(dataGridView2.Rows[dataGridView2.SelectedCells[0].RowIndex].Cells["колвоDataGridViewTextBoxColumn2"].Value.ToString());
                int instcnt = Convert.ToInt32(dataGridView7.Rows[dataGridView7.SelectedCells[0].RowIndex].Cells["колвоDataGridViewTextBoxColumn1"].Value.ToString());
                if (whantcnt > instcnt)
                {
                    
                }
                else
                {
                    ChangeOrderStatus(Convert.ToInt32(dataGridView2.Rows[dataGridView2.SelectedCells[0].RowIndex].Cells["iDOrderDataGridViewTextBoxColumn"].Value), 6);
                    this.orderList1TableAdapter.Fill(this.dBProductionDataSet.OrderList1);
                    GetData();
                }
                //SendToReserv();
                MessageBox.Show("Требуемое кол-во: "+whantcnt.ToString() + " На остатке: " + instcnt.ToString());
                //   MessageBox.Show(dataGridView2.Rows[dataGridView2.SelectedCells[0].RowIndex].Cells["Наименование"].Value.ToString());
            }
            else
                MessageBox.Show("Отсутствует на остатке");
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedCells.Count > 0)
            {
                whatInStockBindingSource.Filter = " [Номенклатура]='" + dataGridView2.Rows[dataGridView2.SelectedCells[0].RowIndex].Cells["Наименование"].Value.ToString()+"'";
                // Convert.ToInt32(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells["ID_Order"].Value);
            }
            else
                whatInStockBindingSource.Filter = null;
        }

        private void GetData()
        {
            Loading f = new Loading();
            f.Show();
            f.Refresh();            
            string SQL = "";
            SQL = "SELECT * FROM dbo.OrderList";

            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand(SQL, conn);           

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                var dtgr = new DataTable();
                dtgr.Load(reader);
                
                dataGridView1.DataSource = dtgr;
                dataGridView1.Update();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // return false;
            }
            finally
            {
                conn.Close();
                f.Close();
            }
        }

        private void GetProdList()
        {

            Loading f = new Loading();
            f.Show();
            f.Refresh();
            string SQL = "SELECT ID, Name + (CASE WHEN isKit = 'true' THEN '|1' ELSE '|0' END) AS Name, Name AS Name2, " +
                "case when isKit='true' then 'ДА' else 'НЕТ' end as isKit " +
                "FROM ProdList";

            SqlConnection conn = new SqlConnection(Connection);
            SqlCommand cmd = new SqlCommand(SQL, conn);

            try
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                var dtgr = new DataTable();
                dtgr.Load(reader);
                comboBox1.DataSource = dtgr;

                comboBox1.Update();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // return false;
            }
            finally
            {
                conn.Close();
                f.Close();
            }
        }

       

    }
}
