using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.Util;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;



namespace Hash
{
    public partial class Form1 : Form
    {

        Image<Bgr, Byte> imgResult1;
        Image<Bgr, Byte> imgResult2;
        Image<Gray, Byte> imgCannyResult1;
        Image<Gray, Byte> imgCannyResult2;
        String imgresult1Src;
        String imgresult2Src;


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                imgresult1Src = Openfile.FileName;

                Image<Bgr, Byte> image1;
                image1 = new Image<Bgr, Byte>(Openfile.FileName);


                ////預設70
                //double cannyValue = Convert.ToDouble(textBox1.Text);
                //Image<Gray, Byte> grayImage = image1.Convert<Gray, Byte>();
                //Image<Gray, Byte> cannyGray = grayImage.Canny(cannyValue, cannyValue);
                ////Image<Gray, Byte> notImage = cannyGray.Not();
                //imgCannyResult1 = cannyGray;


                imgResult1 = image1;

                pictureBox1.Image = image1.ToBitmap();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            if (Openfile.ShowDialog() == DialogResult.OK)
            {
                imgresult2Src = Openfile.FileName;

                Image<Bgr, Byte> image2;
                image2 = new Image<Bgr, Byte>(Openfile.FileName);


                ////預設70
                //double cannyValue = Convert.ToDouble(textBox1.Text);
                //Image<Gray, Byte> grayImage = image2.Convert<Gray, Byte>();
                //Image<Gray, Byte> cannyGray = grayImage.Canny(cannyValue, cannyValue);
                ////Image<Gray, Byte> notImage = cannyGray.Not();
                //imgCannyResult2 = cannyGray;


                imgResult2 = image2;

                pictureBox2.Image = image2.ToBitmap();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            getResult(imgResult1.ToBitmap(), imgResult2.ToBitmap());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //預設70
            double cannyValue = Convert.ToDouble(textBox1.Text);

            //img 1 
            Image<Gray, Byte> grayImage1 = imgResult1.Convert<Gray, Byte>();
            Image<Gray, Byte> cannyGray1 = grayImage1.Canny(cannyValue, cannyValue);
            
            imgCannyResult1 = cannyGray1;

            //img 2 
            Image<Gray, Byte> grayImage2 = imgResult2.Convert<Gray, Byte>();
            Image<Gray, Byte> cannyGray2 = grayImage2.Canny(cannyValue, cannyValue);

            imgCannyResult2 = cannyGray2;


            getResult(imgCannyResult1.ToBitmap(), imgCannyResult2.ToBitmap());
        }

        private void getResult(Image imgresult1, Image imgresult2)
        {
            //第五步：利用 哈希 計算 Hammingdistance 取得百分比

            label1.Text = compareHash(GetHash(imgresult1), GetHash(imgresult2)).ToString() + "%";

        }

        private double compareHash(int[] first , int[] second)
        {
            //first
            StringBuilder hashCode1 = new StringBuilder();
            for (int i = 0; i < first.Length; i += 4)
            {
                int firstResult = first[i] * (int)Math.Pow(2, 3) + first[i + 1] * (int)Math.Pow(2, 2) + first[i + 2] * (int)Math.Pow(2, 1) + first[i + 2];
                hashCode1.Append(string.Format("{0:x}", Convert.ToInt32(firstResult)));//二进制转为16进制  
            }
            String sourceHashCode1 = hashCode1.ToString();

            //second
            StringBuilder hashCode2 = new StringBuilder();
            for (int i = 0; i < second.Length; i += 4)
            {
                int secondResult = second[i] * (int)Math.Pow(2, 3) + second[i + 1] * (int)Math.Pow(2, 2) + second[i + 2] * (int)Math.Pow(2, 1) + second[i + 2];
                hashCode2.Append(string.Format("{0:x}", Convert.ToInt32(secondResult)));//二进制转为16进制  
            }
            String sourceHashCode2 = hashCode2.ToString();

            //Hammingdistance
            int difference = 0;
            int length = sourceHashCode1.Length;

            for (int i = 0; i < length; i++)
            {
                if (sourceHashCode1[i] != sourceHashCode2[i])
                {
                    difference++;
                }
            }

            //0~16轉百分比
            double result;
            result = 100 - (difference * (100 / 16));

            return result;
        }

        public int[] GetHash(Image imgresult1)
        {
            int width = 8;
            int height = 8;

            //第一步：縮小尺寸

            Image SourceImg = ReduceSize(imgresult1, width, height);

            //第二步：簡化色彩

            Byte[] pixels = ReduceColor(SourceImg);

            //第三步：計算平均

            Byte average = CalcAverage(pixels);

            //第四步：比較像素的灰度
            int[] result = ComputeBits(pixels, average);


            return result;
        }

        private Image ReduceSize(Image SourceImg, int width = 8, int height = 8)
        {
            Image image = SourceImg.GetThumbnailImage(width, height, () => { return false; }, IntPtr.Zero);
            return image;
        }

        private Byte[] ReduceColor(Image image)
        {
            Bitmap bitMap = new Bitmap(image);
            Byte[] grayValues = new Byte[image.Width * image.Height];

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    Color color = bitMap.GetPixel(x, y);
                    byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    grayValues[x * image.Width + y] = grayValue;
                }
            return grayValues;
        }

        private Byte CalcAverage(byte[] values)
        {
            int sum = 0;
            for (int i = 0; i < values.Length; i++)
                sum += (int)values[i];
            return Convert.ToByte(sum / values.Length);
        }

        private int[] ComputeBits(byte[] values, byte averageValue)
        {

            int[] result = new int[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < averageValue)
                    result[i] = 0;
                else
                    result[i] = 1;
            }

            return result;
        }

        
    }
}
