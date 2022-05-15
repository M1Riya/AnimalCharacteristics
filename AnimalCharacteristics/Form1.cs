using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Serialization;
using static AnimalCharacteristics.Animal;

namespace AnimalCharacteristics
{
    public partial class Form1 : Form
    {
        List<Animal> animal = new List<Animal>();
        BindingSource bs = new BindingSource();
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.MinimumSize = new Size(900, 600);
            this.MaximumSize = new Size(900, 600);
            FillListAnimal();
            bs.DataSource = animal;
            CreateElements();

        }
        void CreateElements()
        {
            BindingNavigator nav = new BindingNavigator(true);
            nav.BindingSource = bs;
            this.Controls.Add(nav);

            ToolStripButton sv = new ToolStripButton();
            sv.Image = Image.FromFile(@"..\..\image\save.png");
            sv.Enabled = true;
            sv.Click += SavingEvent;
            nav.Items.Add(sv);
            ToolStripButton dwnld = new ToolStripButton();
            dwnld.Image = Image.FromFile(@"..\..\image\download.png");
            dwnld.Enabled = true;
            dwnld.Click += LoadEvent;
            nav.Items.Add(dwnld);

            ToolStripComboBox search = new ToolStripComboBox();
            search.Name = "src";
            search.Items.Add(" ");
            search.Items.Add("числ.>10 тыс.");
            search.SelectedIndexChanged += (o, e) => SearchEvent(search);
            nav.Items.Add(search);

            DataGridView grid = new DataGridView()
            {
                Name = "grid",
                Width = chart2.Width,
                Height = chart2.Height - 20,
                Top = nav.Bottom
            };
            grid.AutoGenerateColumns = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            grid.DataSource = bs;
            CreateColumns(grid);
            grid.RowValidating += grid_RowValidating;
            grid.DataError += DataErrValidate;
            this.Controls.Add(grid);

            this.chart2.Top = grid.Bottom;

            PictureBox pic = new PictureBox()
            {
                Top = nav.Bottom,
                Height = grid.Height,
                Left = grid.Right,
                Width = this.Width - grid.Width,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pic.SizeMode = PictureBoxSizeMode.StretchImage;
            pic.DataBindings.Add("ImageLocation", bs, "Pic", true);
            pic.DoubleClick += ClickPicEvent;
            this.Controls.Add(pic);

            PropertyGrid prGrid = new PropertyGrid()
            {
                Top = pic.Bottom,
                Height = 270,
                Left = chart2.Right,
                Width = pic.Width,
                Anchor = AnchorStyles.Right
            };
            prGrid.DataBindings.Add("SelectedObject", bs, "");
            this.Controls.Add(prGrid);

            chart2.Series.Clear();
            chart2.ChartAreas.Clear();
            chart2.DataSource = from cn in bs.DataSource as List<Animal>
                                group cn by cn.Area into grupel
                                select new
                                {
                                    Area = grupel.Key.ToString(),
                                    Count = grupel.Count()
                                };
            chart2.ChartAreas.Add(new ChartArea());
            chart2.ChartAreas[0].AxisX.Title = "Ареал обитания";
            chart2.ChartAreas[0].AxisY.Title = "Количество животных";
            chart2.Series.Add(new Series());
            chart2.Series[0].ChartType = SeriesChartType.Pie;
            chart2.Series[0].XValueMember = "Area";
            chart2.Series[0].YValueMembers = "Count";
            bs.DataSourceChanged += (o, e) => chart2.DataSource = from cn in bs.DataSource as List<Animal>
                                                                  group cn by cn.Area into grupel
                                                                  select new
                                                                  {
                                                                      Area = grupel.Key.ToString(),
                                                                      Count = grupel.Count()
                                                                  };
            bs.CurrentChanged += (o, e) => chart2.DataBind();
        }
        void SearchEvent(ToolStripComboBox cb)
        {
            switch (cb.SelectedIndex)
            {
                case 0:
                    bs.DataSource = animal;
                    bs.ResetBindings(false);
                    break;
                case 1:
                    SearchPopul();
                    break;
                default:
                    break;
            }
        }

        void SearchPopul() //поиск численности больше 10000
        {
            List<Animal> newAn = new List<Animal>();
            newAn = animal.Where(el => el.Population > 10000).ToList();
            bs.DataSource = newAn;
            bs.ResetBindings(false);
        }

        void LoadEvent(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Файл в bin|*.bin|Файл в xml|*.xml";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                switch (fd.FilterIndex)
                {
                    case 1:
                        BinDeSerialize(fd.FileName);
                        break;
                    case 2:
                        XmlDeSerialize(fd.FileName);
                        break;
                    default:
                        break;
                }
            }
        }
        void BinDeSerialize(string fname)
        {
            Stream st = new FileStream(fname, FileMode.Open);
            BinaryFormatter binf = new BinaryFormatter();
            animal = (List<Animal>)binf.Deserialize(st);
            bs.DataSource = animal;
            st.Close();
            bs.ResetBindings(false);
        }
        void XmlDeSerialize(string fname)
        {
            Stream st = new FileStream(fname, FileMode.Open);
            XmlSerializer xmlser = new XmlSerializer(typeof(List<Animal>));
            animal = (List<Animal>)xmlser.Deserialize(st);
            bs.DataSource = animal;
            st.Close();
            bs.ResetBindings(false);
        }
        void SavingEvent(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.InitialDirectory = System.Environment.CurrentDirectory;
            sf.Filter = "Файл в bin|*.bin|Файл в xml|*.xml";
            if (sf.ShowDialog() == DialogResult.OK)
            {
                switch (sf.FilterIndex)
                {
                    case 1:
                        BinSerialize(sf.FileName);
                        break;
                    case 2:
                        XmlSerialize(sf.FileName);
                        break;
                    default:
                        break;
                }
            }
        }
        void BinSerialize(string fname)
        {
            BinaryFormatter binform = new BinaryFormatter();
            Stream st = new FileStream(fname, FileMode.Create);
            binform.Serialize(st, bs.DataSource);
            st.Close();
        }
        void XmlSerialize(string fname)
        {
            XmlSerializer xmlser = new XmlSerializer(typeof(List<Animal>));
            using (Stream st = new FileStream(fname, FileMode.Create))
            {
                xmlser.Serialize(st, bs.DataSource);
            }
        }
        void ClickPicEvent(object o, EventArgs e)  //обработка добавление картинки (по двойному щелчку)
        {
            OpenFileDialog f = new OpenFileDialog();
            f.ShowDialog();
            string pic = f.FileName;
            if (pic != "")
                (bs.Current as Animal).Pic = pic;
            f.Filter = "Image files(*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
        }
        void grid_RowValidating(object o, DataGridViewCellCancelEventArgs e)
        {
            var nameCheck = (Controls["grid"] as DataGridView)["Name", e.RowIndex].Value;
            var areaCheck = (Controls["grid"] as DataGridView)["Area", e.RowIndex].Value;
            var weightCheck = (Controls["grid"] as DataGridView)["AvgWeight", e.RowIndex].Value;
            var populationCheck = (Controls["grid"] as DataGridView)["Population", e.RowIndex].Value;
            if (nameCheck == null)
            {
                e.Cancel = true;
                (Controls["grid"] as DataGridView).CurrentCell = (Controls["grid"] as DataGridView)["Name", e.RowIndex];
                (Controls["grid"] as DataGridView).BeginEdit(true);
            }

            double wght;
            if (double.TryParse(Convert.ToString(weightCheck), out wght) && wght < 0)
            {
                e.Cancel = true;
                (Controls["grid"] as DataGridView).CurrentCell = (Controls["grid"] as DataGridView)["AvgWeight", e.RowIndex];
                (Controls["grid"] as DataGridView).BeginEdit(true);
            }

            double popul;
            if (double.TryParse(Convert.ToString(populationCheck), out popul) && popul < 0)
            {
                e.Cancel = true;
                (Controls["grid"] as DataGridView).CurrentCell = (Controls["grid"] as DataGridView)["Population", e.RowIndex];
                (Controls["grid"] as DataGridView).BeginEdit(true);
            }
        }
        void DataErrValidate(object o, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
            (Controls["grid"] as DataGridView).BeginEdit(true);
        }
        void CreateColumns(DataGridView g)
        {
            var col1 = new DataGridViewTextBoxColumn();
            col1.Name = "Name";
            col1.HeaderText = "Название";
            col1.DataPropertyName = "Name";
            g.Columns.Add(col1);

            var col2 = new DataGridViewTextBoxColumn();
            col2.Name = "Area";
            col2.HeaderText = "Обитание";
            col2.DataPropertyName = "Area";
            g.Columns.Add(col2);

            var col3 = new DataGridViewTextBoxColumn();
            col3.Name = "Population";
            col3.HeaderText = "Популяция";
            col3.DataPropertyName = "Population";
            g.Columns.Add(col3);

            var col4 = new DataGridViewTextBoxColumn();
            col4.Name = "AvgWeight";
            col4.HeaderText = "Средний вес (кг)";
            col4.DataPropertyName = "AvgWeight";
            g.Columns.Add(col4);

            g.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft; //выравнивание всех строк
        }
        void FillListAnimal()
        {
            animal.Add(new Animal("Речная выдра", Continent.Европа, 90000, 10, @"..\..\image\otter2.jpg"));
            animal.Add(new Animal("Лесной хорёк", Continent.Европа, 10000, 1.5, @"..\..\image\ferret2.jpg"));
            animal.Add(new Animal("Обыкновенная лисица", Continent.Европа, 63400, 8, @"..\..\image\fox1.jpg"));
            animal.Add(new Animal("Обыкновенный ёж", Continent.Европа, 105000, 0.7, @"..\..\image\hedgehog2.jpg"));
            animal.Add(new Animal("Енот-полоскун", Continent.Америка, 20000, 7, @"..\..\image\racoon2.jpg"));
            animal.Add(new Animal("Канадский бобр", Continent.Америка, 9000, 31, @"..\..\image\bobr.jpg"));
            animal.Add(new Animal("Красная панда", Continent.Азия, 1600, 5, @"..\..\image\panda2.jpg"));
        }
    }
}
