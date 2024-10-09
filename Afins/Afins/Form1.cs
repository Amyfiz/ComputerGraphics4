using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Afins
{
    public partial class Form1 : Form
    {
        private List<PointF> points = new List<PointF>();
        private Matrix transformMatrix = new Matrix();
        private bool isRotating = false;
        private bool isTranslating = false;
        private bool isScaling = false;
        private PointF figureCenter;
        private PointF lastMousePosition;
        public Form1()
        {
            InitializeComponent();
            this.panel1.MouseClick += new MouseEventHandler(this.panel1_MouseClick);
            this.panel1.Paint += new PaintEventHandler(this.panel1_Paint);
        }
        
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (points.Count > 1)
            {
                e.Graphics.Transform = transformMatrix;
                e.Graphics.DrawPolygon(Pens.Black, points.ToArray());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Invalidate();
            transformMatrix.Reset();
            points.Clear();
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF[] clickPoint = { e.Location };
                Matrix inverseMatrix = transformMatrix.Clone();
                inverseMatrix.Invert();
                inverseMatrix.TransformPoints(clickPoint);

                points.Add(clickPoint[0]);
                panel1.Invalidate();
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isRotating)
            {
                float angle = CalculateAngle(lastMousePosition, e.Location, figureCenter);
                ApplyRotation(angle, figureCenter);
                lastMousePosition = e.Location;
                panel1.Invalidate();
            }
            else if (isTranslating)
            {
                float dx = e.Location.X - lastMousePosition.X;
                float dy = e.Location.Y - lastMousePosition.Y;
                ApplyTranslation(dx, dy);
                lastMousePosition = e.Location;
                panel1.Invalidate();
            }
            else if (isScaling)
            {
                float scaleX = 1 + (e.Location.X - lastMousePosition.X) / 100.0f;
                float scaleY = 1 + (e.Location.Y - lastMousePosition.Y) / 100.0f;
                if (Control.ModifierKeys == Keys.Space)
                    ApplyScaling(scaleX, scaleY, figureCenter);
                else
                {
                    PointF center = GetPolygonCenter();
                    ApplyScaling(scaleX, scaleY, center);
                }
                lastMousePosition = e.Location;
                panel1.Invalidate();
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isRotating = false;
                isTranslating = false;
                isScaling = false;
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && points.Count > 1)
            {
                if (Control.ModifierKeys == Keys.Control)
                {

                    isRotating = true;
                    figureCenter = e.Location;
                }
                else if (Control.ModifierKeys == Keys.Shift)
                {
                    isTranslating = true;
                }
                else if (Control.ModifierKeys == Keys.Alt)
                {
                    isScaling = true;
                }
                else if (Control.ModifierKeys == Keys.Space)
                {
                    isScaling = true;
                    figureCenter = e.Location;
                }
                else
                {
                    isRotating = true;
                    figureCenter = GetPolygonCenter();
                }
                lastMousePosition = e.Location;
            }
        }
        private void ApplyScaling(float scaleX, float scaleY, PointF center)
        {
            Matrix scaleMatrix = new Matrix(
           scaleX, 0,
                0, scaleY,
            (1 - scaleX) * center.X, (1 - scaleY) * center.Y);

            transformMatrix.Multiply(scaleMatrix, MatrixOrder.Append);
        }

        private void ApplyTranslation(float dx, float dy)
        {
            Matrix translationMatrix = new Matrix(
                1, 0,
                0, 1,
                dx, dy
               );
            transformMatrix.Multiply(translationMatrix, MatrixOrder.Append);
        }
        private void ApplyRotation(float angle, PointF center)
        {
            float radians = angle * (float)Math.PI / 180;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            Matrix rotationMatrix = new Matrix(
                cos, sin,
                -sin, cos,
                -center.X * cos + center.Y * sin + center.X,
                -center.X * sin - center.Y * cos + center.Y);

            transformMatrix.Multiply(rotationMatrix, MatrixOrder.Append);
        }

        private float CalculateAngle(PointF start, PointF end, PointF center)
        {
            float dx1 = start.X - center.X;
            float dy1 = start.Y - center.Y;
            float dx2 = end.X - center.X;
            float dy2 = end.Y - center.Y;

            float angle1 = (float)Math.Atan2(dy1, dx1);
            float angle2 = (float)Math.Atan2(dy2, dx2);

            float angle = (float)((angle2 - angle1) * (180.0 / Math.PI));
            return angle;
        }
        private PointF GetPolygonCenter()
        {
            float sumX = 0;
            float sumY = 0;
            foreach (var point in points)
            {
                sumX += point.X;
                sumY += point.Y;
            }
            return new PointF(sumX / points.Count, sumY / points.Count);
        }
    }
}
