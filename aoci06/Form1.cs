using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aoci06
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;
        Image<Gray, byte> bg = null;

        bool find = false;

        BackgroundSubtractorMOG2 subtractor = new BackgroundSubtractorMOG2(1000, 32, true);


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // инициализация веб-камеры
            subtractor.Clear();
            webCamIs = true;
            timer1.Enabled = false;
            capture = new VideoCapture();
            capture.ImageGrabbed += ProcessFrame;
            capture.Start();
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            var frame = new Mat();
            capture.Retrieve(frame);
            imageBox1.Image = frame;

            if (typs == 0)
            {
                imageBox2.Image = frame;
            }

            
                frame = new Mat();
                capture.Retrieve(frame);
                Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
                
                
                
                if (bg != null && typs == 1)
                {
                    imageBox2.Image = diffusal(image, bg);
                }
                else
                if (typs == 2)
                {
                    var foregroundMask = image.Convert<Gray, byte>().CopyBlank();
                    subtractor.Apply(image.Convert<Gray, byte>(), foregroundMask);
                    var filteredMask = FilterMask(foregroundMask, image);
                    imageBox2.Image = filteredMask;
                }
            
        }

        public Image<Bgr, byte> diffusal(Image<Bgr, byte> image, Image<Gray, byte> bg)
        {
            Image<Gray, byte> diff = bg.AbsDiff(image.Convert<Gray, byte>());

            diff.Erode(3);
            diff.Dilate(4);

            Image<Gray, byte> binarizedImage = diff.ThresholdBinary(new Gray(30), new Gray(255));

            var copy = image.Copy();
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(binarizedImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i], false) > 50)
                {
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                    copy.Draw(rect, new Bgr(Color.DarkRed), 1);
                }
            }

            return copy;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var frame = capture.QueryFrame();
            bg = frame.ToImage<Gray, byte>();
        }


        public string openV()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string fileName = openFileDialog.FileName;
                return fileName;
            }
            return null;
        }


        bool webCamIs = false;
        private void button3_Click(object sender, EventArgs e)
        {
            if (webCamIs == true)
            {
                webCamIs = false;
                capture.Stop();
            }
            vidFrame = 0;
            subtractor.Clear();
            capture = new VideoCapture(openV());
            timer1.Enabled = true;
            bg = null;
        }


        public Image<Bgr, byte> FilterMask(Image<Gray, byte> mask, Image<Bgr, byte> image)
        {
            var anchor = new Point(-1, -1);
            var borderValue = new MCvScalar(1);
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(3, 3), anchor);
            var closing = mask.MorphologyEx(MorphOp.Close, kernel, anchor, 1, BorderType.Default, borderValue);
            var opening = closing.MorphologyEx(MorphOp.Open, kernel, anchor, 1, BorderType.Default, borderValue);
            var dilation = opening.Dilate(7);
            var threshold = dilation.ThresholdBinary(new Gray(240), new Gray(255));

            var copy = image.Copy();
            var contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(threshold, contours, null, RetrType.External, ChainApproxMethod.ChainApproxTc89L1);
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i]) > 500)
                {
                    Rectangle boundingRect = CvInvoke.BoundingRectangle(contours[i]);
                    copy.Draw(boundingRect, new Bgr(Color.Red), 2);
                }
            }

            return copy;
        }

        int vidFrame = 0;
        byte typs = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            vidFrame++;
            if (vidFrame >= capture.GetCaptureProperty(CapProp.FrameCount))
            {
                timer1.Enabled = false;
            }
            else
            {
                var frame = capture.QueryFrame();
                Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();

                if (typs == 0)
                {
                    imageBox1.Image = image;
                    imageBox2.Image = image;
                }
                else
                    if (bg != null && typs == 1)
                {
                    imageBox2.Image = diffusal(image, bg);
                    imageBox1.Image = frame;
                }
                else
                if (typs == 2)
                {
                    imageBox1.Image = frame;
                    var foregroundMask = image.Convert<Gray, byte>().CopyBlank();
                    subtractor.Apply(image.Convert<Gray, byte>(), foregroundMask);
                    var filtrMask = FilterMask(foregroundMask, image);
                    imageBox2.Image = filtrMask;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (bg != null)
            
                typs = 1;
            
            else
            
                typs = 0;
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!find)
            {
                var frame = capture.QueryFrame();
                bg = frame.ToImage<Gray, byte>();
                button5.Text = "find on";
                find = true;
                typs = 2;
            }
            else
            {
                find = false;
                button5.Text = "find off";
                typs = 0;
            }

        }
    }
}
