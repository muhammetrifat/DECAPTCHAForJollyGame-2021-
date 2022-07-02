using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IronOcr;
using AForge.Imaging.Filters;
using Tesseract;
using System.Drawing.Drawing2D;
using System.Data.SqlClient;

namespace DECAPTCHA
{
    public partial class Form1 : Form
    {
        SqlConnection con = new SqlConnection(@"Data Source = DESKTOP-H56L443\SQLEXPRESS ; Initial Catalog = DenemeDB ; User ID = sa; Password = 1234");
        public int sayi = 7;//İD YE EKLENİP OTOMATİK ARTACAK
        public Form1()
        {
            InitializeComponent();
        }//FORM
        void scroll()
        {
            webBrowser2.Document.Window.ScrollTo(293, 1122);
        }//SAYFAYI AŞAĞIYA ÇEK
        private void button2_Click(object sender, EventArgs e)
        {
            scroll();
            Bitmap image = new Bitmap(89, 25);
            webBrowser2.DrawToBitmap(image, new Rectangle(0, 0, 89, 25));
            Bitmap grayScaleBP = new Bitmap(2, 2, PixelFormat.Format16bppGrayScale);
            Bitmap bmp = new Bitmap(3 * image.Width, 3 * image.Height, PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, 3 * image.Width, 3 * image.Height);
            }
            int width = bmp.Width;
            int height = bmp.Height;
            int[] arr = new int[225];
            int i = 0;
            Color p;
            //Grayscale
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    p = bmp.GetPixel(x, y);
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;
                    int avg = (r + g + b) / 3;
                    avg = avg < 130 ? 255 : 0;     // Converting gray pixels to either pure black or pure white
                    bmp.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }
            textBox4.Text = DeCaptcha((Image)bmp);
        }//YAZIYA ÇEVİR
        private string DeCaptcha(Image img)
        {
            Bitmap bmp = new Bitmap(img);
            bmp = bmp.Clone(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Erosion erosion = new Erosion();
            Dilatation dilatation = new Dilatation();
            Invert inverter = new Invert();
            ColorFiltering cor = new ColorFiltering();
            cor.Blue = new AForge.IntRange(200, 255);
            cor.Red = new AForge.IntRange(200, 255);
            cor.Green = new AForge.IntRange(200, 255);
            Opening open = new Opening();
            BlobsFiltering bc = new BlobsFiltering() { MinHeight = 10 };
            Closing close = new Closing();
            GaussianSharpen gs = new GaussianSharpen(0.1);
            ContrastCorrection cc = new ContrastCorrection(1);
            //FiltersSequence seq = new FiltersSequence(gs, inverter, open, inverter, bc, inverter, open, cc, cor, bc, inverter);


            //FiltersSequence seq2 = new FiltersSequence(inverter, erosion, dilatation, inverter, open, open, open);
            FiltersSequence seq3 = new FiltersSequence(inverter, erosion, dilatation, dilatation, inverter, dilatation);
            //FiltersSequence seq4 = new FiltersSequence(inverter);
            //FiltersSequence seq5 = new FiltersSequence(cor);
            //FiltersSequence seq6 = new FiltersSequence(open);
            //FiltersSequence seq7 = new FiltersSequence(bc);
            //FiltersSequence seq8 = new FiltersSequence(close);
            //FiltersSequence seq9 = new FiltersSequence(gs);
            //FiltersSequence seq10 = new FiltersSequence(cc);


            //FiltersSequence seq11 = new FiltersSequence(inverter);
            //FiltersSequence seq12 = new FiltersSequence(erosion);


            //Bitmap bmp2 = seq2.Apply(bmp);
            Bitmap bmp3 = seq3.Apply(bmp);
            //Bitmap bmp4 = seq4.Apply(bmp);
            //Bitmap bmp5 = seq5.Apply(bmp);
            //Bitmap bmp6 = seq6.Apply(bmp);
            //Bitmap bmp7 = seq7.Apply(bmp);
            //Bitmap bmp8 = seq8.Apply(bmp);
            //Bitmap bmp9 = seq9.Apply(bmp);
            //Bitmap bmp10 = seq10.Apply(bmp);
            //Bitmap bmp11 = seq11.Apply(bmp);
            //Bitmap bmp12 = seq12.Apply(bmp);


            pictureBox1.Image = bmp3;
            //pictureBox2.Image = bmp2;
            //pictureBox3.Image = bmp3;
            //pictureBox4.Image = bmp4;
            //pictureBox5.Image = bmp5;
            //pictureBox6.Image = bmp6;
            //pictureBox7.Image = bmp7;
            //pictureBox8.Image = bmp8;
            //pictureBox9.Image = bmp9;
            //pictureBox10.Image = bmp10;
            //pictureBox11.Image = bmp11;
            //pictureBox12.Image = bmp;
            //pictureBox2.Image = bmp;
            return OCR(bmp3);
            //return OCR(bmp2);
        }//DECAPTCHA
        private string OCR(Bitmap bmp)
        {
            using (TesseractEngine engine = new TesseractEngine(@"tessdata", "eng", EngineMode.Default))
            {
                engine.SetVariable("tessedit_char_whitelist", "1234567890abcdefghijklmnopqrstuvwxyz");
                engine.SetVariable("tessedit_unrej_any_wd", true);
                //engine.SetVariable("textord_all_prop", 1);
                engine.SetVariable("tessedit_reject_bad_qual_wds", 0);
                engine.SetVariable("textord_noise_rejwords", 0);
                engine.SetVariable("textord_noise_rejrows", 0);
                engine.SetVariable("tessedit_use_reject_spaces", 1);
                engine.SetVariable("textord_noise_rejrows", 0);

                using (var page = engine.Process(bmp, PageSegMode.SingleLine))
                {
                    return page.GetText();
                }
            }
        }//OCR
        public static string UserPassword;
        public string id = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            id = "kriptorhan";
            string sqlquery1 = "SELECT Count FROM Table1 where AccountID = '" + id + "'";
            textBox2.Text = id;
            SqlCommand command1 = new SqlCommand(sqlquery1, con);
            con.Open();
            object topo = command1.ExecuteScalar();
            con.Close();
            string sqlquery2 = "SELECT userPassword FROM Table1 where AccountID = '" + id + "'";
            SqlCommand command2 = new SqlCommand(sqlquery2, con);
            con.Open();
            object password = command2.ExecuteScalar();
            con.Close();
            sayi = Convert.ToInt32(topo);
            UserPassword = password.ToString();
            label1.Text = sayi.ToString();
            webBrowser2.Navigate("https://www.csilkroad.com/index.php?page=register");
            fillAccountData();
        }//LOAD
        void fillAccountData()
        {
            DataSet ds = new DataSet();
            con.Open();
            SqlDataAdapter adtr = new SqlDataAdapter("Select * from Table1", con);
            adtr.Fill(ds, "Table1");
            dataGridView1.DataSource = ds.Tables["Table1"];
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            adtr.Dispose();

            this.dataGridView1.Columns["id"].Visible = false;

            dataGridView1.Columns[1].HeaderText = "Account İsmi";
            dataGridView1.Columns[2].HeaderText = "Sayı";
            dataGridView1.Columns[3].HeaderText = "Şifresi";

            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.WhiteSmoke;

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            con.Close();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            webBrowser2.Document.GetElementById("reload").InvokeMember("click");
        }//RELOAD İMAGE
        void refreshPages()
        {
            webBrowser2.Navigate("https://www.csilkroad.com/index.php?page=register");

        }//RELOAD PAGE FONK.
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                textBox1.Text = "99999";
            if (sayi - 1 >= Convert.ToInt32(textBox1.Text))
            {
                timer2.Stop();
                MessageBox.Show("İstenilen sayıya ulaştınız!", "Sistem Mesajı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                upperCount();
                HtmlDocument doc1 = this.webBrowser2.Document;
                doc1.GetElementById("id").SetAttribute("Value", textBox2.Text + sayi.ToString());
                doc1.GetElementById("pw").SetAttribute("Value", UserPassword);
                doc1.GetElementById("pw2").SetAttribute("Value", UserPassword);
                doc1.GetElementById("email").SetAttribute("Value", textBox2.Text + sayi.ToString() + "@guuuyuuur.com");
                doc1.GetElementById("email2").SetAttribute("Value", textBox2.Text + sayi.ToString() + "@guuuyuuur.com");
                doc1.GetElementById("name").SetAttribute("Value", textBox2.Text + sayi.ToString());
                doc1.GetElementById("answer").SetAttribute("Value", textBox2.Text + sayi.ToString());
                doc1.GetElementById("scode").SetAttribute("Value", textBox4.Text);
                doc1.GetElementById("check").SetAttribute("checked", "checked");

                var links = doc1.GetElementsByTagName("button");
                foreach (HtmlElement link in links)
                {
                    if (link.GetAttribute("className") == "button1")
                    {
                        link.InvokeMember("click");
                        sayi++;
                        label1.Text = sayi.ToString();
                    }
                }
                timer1.Start();
            }
        }//KAYDOL
        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (textBox2.Text == "ID")
            {
                textBox2.Text = "";
            }
        }//ID YAZISINI SİLİYOR
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            refreshPages();
            button2_Click(sender, e);
        }//RELOAD PAGE
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //upperCount();
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            SqlCommand command = new SqlCommand("update Table1 set Count=@Count where AccountID = '" + textBox2.Text + "'", con);
            sayi--;
            command.Parameters.AddWithValue("@Count", Convert.ToInt32(sayi));
            con.Open();
            command.ExecuteNonQuery();
            con.Close();
            label1.Text = sayi.ToString();
            dataGridView1.CurrentRow.Cells["Count"].Value = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Count"].Value) - 1;
            refreshPages();
        }
        void upperCount()
        {
            SqlCommand command = new SqlCommand("update Table1 set Count=@Count where AccountID = '" + textBox2.Text + "'", con);
            command.Parameters.AddWithValue("@Count", Convert.ToInt32(sayi + 1));
            con.Open();
            command.ExecuteNonQuery();
            con.Close();
            dataGridView1.CurrentRow.Cells["Count"].Value = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Count"].Value) + 1;
        }//DATABASE SON AÇILAN SAYIYI GİRİYOR
        void ikincilOkuma()
        {
            Bitmap image = new Bitmap(89, 25);
            webBrowser2.DrawToBitmap(image, new Rectangle(0, 0, 89, 25));
            var img = image;
            var ocr = new TesseractEngine("./tessdata", "eng");
            var sonuc = ocr.Process(img);
            //textBox1.Text = sonuc.GetText();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            HtmlDocument doc1 = this.webBrowser2.Document;
            var links1 = doc1.GetElementsByTagName("div");
            foreach (HtmlElement link in links1)
            {
                //if (link.InnerText == "Oyuna giriş yapabilirsiniz, kayıt işleminiz başarıyla tamamlanmıştır.")
                //{
                //    timer1.Stop();
                //    pictureBox2_Click(sender, e);
                //    break;
                //}
                //else
                //{
                //    timer1.Stop();
                //    pictureBox3_Click(sender, e);
                //    break;
                //}
                if (link.InnerText == "Güvenlik kodu hatalı.")
                {
                    timer1.Stop();
                    pictureBox3_Click(sender, e);
                    break;
                }
                else if (link.InnerText == "Oyuna giriş yapabilirsiniz, kayıt işleminiz başarıyla tamamlanmıştır.")
                {
                    timer1.Stop();
                    pictureBox2_Click(sender, e);
                    break;
                }
            }
        }
        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            button2_Click(sender, e);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            timer2.Start();
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }
        private void button5_Click(object sender, EventArgs e)
        {
            timer2.Stop();
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            id = dataGridView1.CurrentRow.Cells["AccountID"].Value.ToString();
            sayi = Convert.ToInt32(dataGridView1.CurrentRow.Cells["Count"].Value);
            UserPassword = dataGridView1.CurrentRow.Cells["userPassword"].Value.ToString();
            label1.Text = dataGridView1.CurrentRow.Cells["Count"].Value.ToString();
            textBox2.Text = dataGridView1.CurrentRow.Cells["AccountID"].Value.ToString();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
