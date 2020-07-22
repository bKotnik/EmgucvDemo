using Dapper;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EmgucvDemo
{
    public partial class Form1 : Form
    {
        Dictionary<string, Image<Bgr, byte>> imgList;
        public Form1()
        {
            InitializeComponent();
            imgList = new Dictionary<string, Image<Bgr, byte>>();

            //create table Hole
            CreateTableHole();
        }

        private void OpenImage_Click(object sender, EventArgs e)
        {
            try
            {
                imgList.Clear();
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var img = new Image<Bgr, byte>(dialog.FileName);
                    img = img.Resize(1024, 768, Emgu.CV.CvEnum.Inter.Linear);
                    AddImage(img, "Input");
                    pictureBox1.Image = img.AsBitmap();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddImage(Image<Bgr, byte> img, string keyname)
        {
            if (!treeView1.Nodes.ContainsKey(keyname))
            {
                TreeNode node = new TreeNode(keyname);
                node.Name = keyname;
                treeView1.Nodes.Add(node);
                treeView1.SelectedNode = node;
            }

            if (!imgList.ContainsKey(keyname))
            {
                imgList.Add(keyname, img);
            }
            else
            {
                imgList[keyname] = img;
            }
        }

        private void findHolesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox1.Image == null) return;

                var img = new Bitmap(pictureBox1.Image).ToImage<Bgr, byte>();

                var gray = img.Convert<Gray, byte>()
                    .ThresholdBinaryInv(new Gray(240), new Gray(255));

                // contours
                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                Mat h = new Mat();

                CvInvoke.FindContours(gray, contours, h, Emgu.CV.CvEnum.RetrType.External
                    , Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);


                VectorOfPoint approx = new VectorOfPoint();

                Dictionary<int, double> shapes = new Dictionary<int, double>();

                for (int i = 0; i < contours.Size; i++)
                {
                    approx.Clear();
                    double perimeter = CvInvoke.ArcLength(contours[i], true);
                    CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);
                    double area = CvInvoke.ContourArea(contours[i]);

                    if (approx.Size > 6)
                    {
                        shapes.Add(i, area);
                    }
                }


                if (shapes.Count > 0)
                {
                    var sortedShapes = (from item in shapes
                                        orderby item.Value ascending
                                        select item).ToList();

                    for (int i = 0; i < sortedShapes.Count; i++)
                    {
                        CvInvoke.DrawContours(img, contours, sortedShapes[i].Key, new MCvScalar(0, 0, 255), 2);
                        var moments = CvInvoke.Moments(contours[sortedShapes[i].Key]);
                        int x = (int)(moments.M10 / moments.M00);
                        int y = (int)(moments.M01 / moments.M00);

                        /*Initiale mask for flood filling - tut useless
                        int height = img.Height;
                        int width = img.Width;
                        Mat mask = new Mat(width, height+4, DepthType.Cv8U, 1);
                        CvInvoke.FloodFill(img, mask, new Point(x, y), new MCvScalar(255, 255, 255), out rect,
                          new MCvScalar(155,155,155), new MCvScalar(255,255,255));
                        */

                        CvInvoke.PutText(img, (i + 1).ToString(), new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheyTriplex, 1.0,
                            new MCvScalar(0, 0, 255), 2);
                        CvInvoke.PutText(img, sortedShapes[i].Value.ToString(), new Point(x, y - 30), Emgu.CV.CvEnum.FontFace.HersheyTriplex, 1.0,
                            new MCvScalar(0, 0, 255), 2);

                        Console.WriteLine("X: " + x + ", Y: " + y);

                        // vzames x in y -> sepravi kjer je contour postavljen, gres cez zanke in pristevas dokler je piksel črn... recimo bo delal
                        int pixel = 0;

                        // za dimenzije
                        int x_desno_counter = 0;
                        int x_levo_counter = 0;
                        int x_sum = 0;
                        int y_spodnji_counter = 0;
                        int y_zgornji_counter = 0;
                        int y_sum = 0;
                        int tmp_x = x;
                        int tmp_y = y;
                        int area = 0;

                        /*ZA DOBIT X-E*/
                        while (true) // neskoncna pa jo pol breakam
                        {
                            if (img.Data[tmp_y, tmp_x, 0] < 255)
                                x_desno_counter++;
                            else
                                break;

                            //se pomaknemo desno -> povečamo x
                            tmp_x++;
                        }

                        tmp_x = x; //reseteram

                        while (true) // neskoncna pa jo pol breakam
                        {
                            if (img.Data[tmp_y, tmp_x, 0] < 255)
                                x_levo_counter++;
                            else
                                break;

                            //se pomaknemo levo -> zmanjsamo x
                            tmp_x--;
                        }

                        tmp_x = x;

                        /*ZA DOBIT Y-E*/
                        while (true) // neskoncna pa jo pol breakam
                        {
                            if (img.Data[tmp_y, tmp_x, 0] < 255)
                                y_spodnji_counter++;
                            else
                                break;

                            //se pomaknemo dol -> zmanjsamo y
                            tmp_y--;
                        }

                        tmp_y = y; //reseteram

                        while (true) // neskoncna pa jo pol breakam
                        {
                            if (img.Data[tmp_y, tmp_x, 0] < 255)
                                y_zgornji_counter++;
                            else
                                break;

                            //se pomaknemo gor -> povečamo y
                            tmp_y++;
                        }

                        //dobimo vsoto levih in desnih x pikslov
                        x_sum = x_desno_counter + x_levo_counter;
                        //dobimo vsoto zgornjih in spodnjih y pikslov
                        y_sum = y_zgornji_counter + y_spodnji_counter;

                        //dobimo začetke
                        int x_start = x - x_levo_counter;
                        int y_start = y + y_spodnji_counter;

                        area = x_sum + y_sum;
                        Console.WriteLine("AREA SUM BEFORE: " + area);

                        //dvojna zanka -> gremo čez kvadrat in štejemo samo črne piksle
                        for(int j = y_start; j < y_start+y_sum; j++)
                        {
                            for(int k = x_start; k < x_start+x_sum; k++)
                            {
                                if (img.Data[j, k, 0] < 255) // je črn piksel
                                    area++; // povečamo ploščino
                            }
                        }

                        Console.WriteLine("X DESNI COUNTER: " + x_desno_counter);
                        Console.WriteLine("X LEVI COUNTER: " + x_levo_counter);
                        Console.WriteLine("Y ZGORNJI COUNTER: " + y_zgornji_counter);
                        Console.WriteLine("Y SPODNJI  COUNTER: " + y_spodnji_counter);
                        Console.WriteLine("X SUM: " + x_sum);
                        Console.WriteLine("Y SUM: " + y_sum);
                        Console.WriteLine("X START: " + x_start);
                        Console.WriteLine("Y START: " + y_start);
                        Console.WriteLine("AREA: " + area);
                        Console.WriteLine("---------------------------------------------------------------");

                        //se zapisem v podatkovno bazo
                        Luknja luknja = new Luknja(x_sum, y_sum, area);
                        //connect to database
                        using (IDbConnection connection = new SqlConnection(Utility.ConnVal("ec38")))
                        {
                            //insert into table DelovniNalogi
                            connection.Query("INSERT INTO Holes(hole_height, hole_width, hole_area) VALUES(@height, @width, @area);",
                                    new { height = luknja.Height, width = luknja.Width, area = luknja.Area });
                        }
                    }

                    /*BOLJ PRIBLIŽNE VREDNOSTI SO...*/

                    /* BIGGEST CONTOUR - USELESS
                    double largest = sortedShapes[sortedShapes.Count - 1].Value;
                    Console.WriteLine("largest contour: " + largest);
                    */

                }

                pictureBox1.Image = img.ToBitmap();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Try different threshold\n" + ex.Message);
            }
        }

        private void binarizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                formHarisParameters form = new formHarisParameters(0, 255, 100);
                form.OnApply += ApplyThreshold;
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ApplyThreshold(int x)
        {
            ScalarArray elem = new ScalarArray(0);

            try
            {
                if (imgList["Input"] == null)
                {
                    return;
                }

                var img = imgList["Input"].Convert<Gray, byte>().Clone();
                img = img.Not();
                var output = img.ThresholdBinary(new Gray(x), new Gray(255));
                CvInvoke.Dilate(output, output, elem, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, new MCvScalar(255, 255, 255));
                pictureBox1.Image = output.AsBitmap();
                AddImage(output.Convert<Bgr, byte>(), "Thresholding");
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        private void CreateTableHole()
        {
            //connect to database
            using (IDbConnection connection = new SqlConnection(Utility.ConnVal("ec38")))
            {
                //first drop table if it already exists
                connection.Query("DROP TABLE IF EXISTS Holes;");

                //create table kosi
                connection.Query("CREATE TABLE Holes(id_hole int IDENTITY(1,1) not null primary key," +
                                 "hole_height int not null, hole_width int not null, hole_area float not null);");
            }
        }
    }
}
