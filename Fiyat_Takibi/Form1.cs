using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using CsvHelper;
using HtmlAgilityPack;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using Telegram.Bot;

namespace muhtas_2_fiyat_takibi
{
    public partial class Form1 : Form
    {
        public TelegramBotClient bot = new TelegramBotClient("5173446300:AAFa4HW1RgjvhBKNLCCH4ZMbe4U-ADRLkmM");
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }
        SQLiteConnection baglan = new SQLiteConnection("Data source=veritabani.db");
        

        public void listele()
        {
            baglan.Open(); // Bağlantıyı aç
            SQLiteDataAdapter adapter = new SQLiteDataAdapter("Select Id,urun_adi,Cast(urun_fiyati as varchar) as urun_fiyati,istenilen_fiyat,satici_ismi,urun_linki From fiyat_takip", baglan); //Belirtilen verileri, databaseye bağlanma yoluyla al. 
            DataSet dset = new DataSet();
            adapter.Fill(dset, "info");
            dataGridView1.DataSource = dset.Tables[0];
            baglan.Close();

        }

        public void kiymetli()
        {
            
            SQLiteCommand com = new SQLiteCommand("Select Cast(altin as varchar) as altin,Cast(gumus as varchar) as gumus,Cast(dolar as varchar) as dolar,Cast(euro as varchar) as euro from fiyat_takip_maden", baglan);
            baglan.Open();
            SQLiteDataReader dr = com.ExecuteReader();
            if (dr.Read())
            {
                label18.Text = dr["altin"].ToString();
                label19.Text = dr["gumus"].ToString();
                label20.Text = dr["dolar"].ToString();
                label21.Text = dr["euro"].ToString();
            }
            baglan.Close();
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            kiymetli();
            listele();
            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[1].Width = 290;
            dataGridView1.Columns[2].Width = 91;
            dataGridView1.Columns[3].Width = 100;
            dataGridView1.Columns[4].Width = 100;
            dataGridView1.Columns[5].Width = 320;

            Thread thread = new Thread(t =>
            {
                bool kontrol = true;
                while (kontrol)
                {
                    HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                    HtmlAgilityPack.HtmlDocument doc = web.Load("http://altin.in/");
                    var altin = doc.DocumentNode.SelectSingleNode("//*[@id='icerik']/div[1]/div[2]/div[4]/ul/li[2]").InnerText;
                    var gumus = doc.DocumentNode.SelectSingleNode("//*[@id='icerik']/div[1]/div[2]/div[11]/ul/li[2]").InnerText;
                    var dolar = doc.DocumentNode.SelectSingleNode("//*[@id='dfiy']").InnerText;
                    var euro = doc.DocumentNode.SelectSingleNode("//*[@id='efiy']").InnerText;
                    label7.Text = altin;
                    label8.Text = gumus;
                    label9.Text = dolar;
                    label10.Text = euro;
                    System.Threading.Thread.Sleep(2000);
                    

                }
            })
            { IsBackground = true };
                thread.Start();
            
            
        }

        private void button1_Click(object sender, EventArgs e)
        {   
            void veritabani_baglantisi(string urun_adi,float yeni_urun_fiyati,int istenilen_fiyat, string satici_ismi, string urun_linki)
            {
                baglan.Open();
                string SQL = "insert into fiyat_takip(urun_adi,urun_fiyati,istenilen_fiyat,satici_ismi,urun_linki) Values('" + urun_adi + "' , '" + yeni_urun_fiyati + "', '" + istenilen_fiyat + "' , '" + satici_ismi + "', '" + urun_linki + "')";
                SQLiteCommand komut = new SQLiteCommand(SQL, baglan);
                komut.ExecuteNonQuery();
                baglan.Close();
            }

            string urun_linki = textBox1.Text;
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load(urun_linki);

            if (urun_linki.IndexOf("trendyol")!= -1)
            {
                var urun_adi = doc.DocumentNode.SelectSingleNode("//h1[@class='pr-new-br']").InnerText;

                var urun_fiyati = doc.DocumentNode.SelectSingleNode("//span[@class='prc-dsc']").InnerText;
                float yeni_urun_fiyati = Convert.ToSingle(urun_fiyati.Substring(0, urun_fiyati.Length - 3));

                int istenilen_fiyat = Convert.ToInt32(textBox2.Text);
                var satici_ismi = doc.DocumentNode.SelectSingleNode("//a[@class='merchant-text']").InnerText;

                veritabani_baglantisi(urun_adi, yeni_urun_fiyati, istenilen_fiyat, satici_ismi, urun_linki);
                
            }
            else if(urun_linki.IndexOf("n11") != -1)
            {
                var urun_adi = doc.DocumentNode.SelectSingleNode("//h1[@class='proName']").InnerText;

                var urun_fiyati = doc.DocumentNode.SelectSingleNode("//del[@class='oldPrice']").InnerText;
                foreach (var baslik in urun_fiyati)
                {
                    label13.Text=baslik.ToString();
                }
                label13.Text = urun_fiyati.ToString();
               // float yeni_urun_fiyati = Convert.ToSingle(urun_fiyati.Substring(0, urun_fiyati.Length - 3));

               // int istenilen_fiyat = Convert.ToInt32(textBox2.Text);
               // var satici_ismi = doc.DocumentNode.SelectSingleNode("//a[@class='merchant-text']").InnerText;

               // veritabani_baglantisi(urun_adi, yeni_urun_fiyati, istenilen_fiyat, satici_ismi, urun_linki);
            }
            else if(urun_linki.IndexOf("hepsiburada") != -1)
            {
                var urun_adi = doc.DocumentNode.SelectSingleNode("//h1[@class='pr-new-br']").InnerText;

                var urun_fiyati = doc.DocumentNode.SelectSingleNode("//span[@class='prc-dsc']").InnerText;
                float yeni_urun_fiyati = Convert.ToSingle(urun_fiyati.Substring(0, urun_fiyati.Length - 3));

                int istenilen_fiyat = Convert.ToInt32(textBox2.Text);
                var satici_ismi = doc.DocumentNode.SelectSingleNode("//a[@class='merchant-text']").InnerText;

                veritabani_baglantisi(urun_adi, yeni_urun_fiyati, istenilen_fiyat, satici_ismi, urun_linki);
            }

            else if(urun_linki.IndexOf("gittigidiyor") != -1)
            {
                var urun_adi = doc.DocumentNode.SelectSingleNode("//h1[@class='pr-new-br']").InnerText;

                var urun_fiyati = doc.DocumentNode.SelectSingleNode("//span[@class='prc-dsc']").InnerText;
                float yeni_urun_fiyati = Convert.ToSingle(urun_fiyati.Substring(0, urun_fiyati.Length - 3));

                int istenilen_fiyat = Convert.ToInt32(textBox2.Text);
                var satici_ismi = doc.DocumentNode.SelectSingleNode("//a[@class='merchant-text']").InnerText;

                veritabani_baglantisi(urun_adi, yeni_urun_fiyati, istenilen_fiyat, satici_ismi, urun_linki);
            }
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            listele();

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
        Thread thread = new Thread(t =>
            {
                bool kontrol = true;
                while (kontrol)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                    {
                        
                        HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
                        HtmlAgilityPack.HtmlDocument doc = web.Load(dataGridView1.Rows[i].Cells[5].Value.ToString());
                        var urun_fiyati = doc.DocumentNode.SelectSingleNode("//span[@class='prc-dsc']").InnerText;
                        float yeni_urun_fiyati = Convert.ToSingle(urun_fiyati.Substring(0, urun_fiyati.Length - 3));
                        string istenilen_fiyat = dataGridView1.Rows[i].Cells[3].Value.ToString();
                        string urun_ismi = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        label13.Text = dataGridView1.Rows[i].Cells[0].Value.ToString();

                        if (Convert.ToSingle(yeni_urun_fiyati) < Convert.ToSingle(istenilen_fiyat))
                        {
                            bot.SendTextMessageAsync(1236391339, urun_ismi + "\nİNDİRİMLİ FİYATI --> " + yeni_urun_fiyati );
                            MessageBox.Show("Ürünün Fiyatı Düştü !!!");
                            kontrol = false;
                            
                        }
                        
                    }
                }
            })
            { IsBackground = true };

            thread.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            int selectedIndex = dataGridView1.CurrentCell.RowIndex;
            int secilen_index_id = dataGridView1.CurrentCell.RowIndex;
            label13.Text = selectedIndex.ToString();
            
            if (selectedIndex > -1)
            {
                //SQLiteCommand cmd = new SQLiteCommand("delete from fiyat_takip where name = " + );
                dataGridView1.Rows.RemoveAt(selectedIndex);
                dataGridView1.Refresh();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string secili_item = comboBox1.SelectedItem.ToString();
          
            baglan.Open();
            string SQL = "Update fiyat_takip_maden SET '" + secili_item + "' = '" + Convert.ToSingle(textBox3.Text) + "'";
            SQLiteCommand komut = new SQLiteCommand(SQL, baglan);
            komut.ExecuteNonQuery();
            baglan.Close();

            kiymetli();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            /*Thread thread = new Thread(t =>
            {
                string istenilen_altin = label18.Text;
            string istenilen_gumus = label19.Text;
            string istenilen_dolar = label20.Text;
            string istenilen_euro = label21.Text;

            if (Convert.ToSingle(altin) < Convert.ToSingle(istenilen_altin))
            {
                bot.SendTextMessageAsync(1236391339, "Altın Fiyatı Düştü " + altin);
                MessageBox.Show("selam");
            }
            else if (Convert.ToSingle(gumus) < Convert.ToSingle(istenilen_gumus))
            {
                bot.SendTextMessageAsync(1236391339, "Gümüş Fiyatı Düştü " + gumus);
                kontrol = false;

            }
            else if (Convert.ToSingle(dolar) < Convert.ToSingle(istenilen_dolar))
            {
                bot.SendTextMessageAsync(1236391339, "Dolar Fiyatı Düştü " + dolar);
                kontrol = false;

            }
            else if (Convert.ToSingle(euro) < Convert.ToSingle(istenilen_euro))
            {
                bot.SendTextMessageAsync(1236391339, "Euro Fiyatı Düştü " + euro);
                kontrol = false;

            }
            })
            { IsBackground = true };
            thread.Start();*/
        }
    }
}
