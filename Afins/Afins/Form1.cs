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
        private PointF figureCenter;
        private PointF lastMousePosition;
        private const float closeDistance = 10f; // Максимальное расстояние для завершения фигуры

        public Form1()
        {
            InitializeComponent();
            this.panel1.MouseClick += new MouseEventHandler(this.panel1_MouseClick);
            this.panel1.Paint += new PaintEventHandler(this.panel1_Paint);
            panel1.MouseWheel += new MouseEventHandler(this.panel1_MouseWheel);
            panel1.MouseDown += new MouseEventHandler(this.panel1_MouseDown);
            panel1.MouseMove += new MouseEventHandler(this.panel1_MouseMove);
            panel1.MouseUp += new MouseEventHandler(this.panel1_MouseUp);
        }

        private void panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            float scaleFactor = e.Delta > 0 ? 1.1f : 0.9f;
            PointF pointToScale = e.Location;

            ApplyScaling(scaleFactor, scaleFactor, pointToScale);
            panel1.Invalidate();
        }

        //переделано
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Transform = transformMatrix;

            // Отрисовываем отрезки по точкам
            if (points.Count > 0)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    e.Graphics.DrawLine(Pens.Black, points[i], points[i + 1]);
                }

                // Соединяем последнюю и первую точку, если это необходимо
                if (points.Count > 2 && IsClose(points[0], points[points.Count - 2]))
                {
                    e.Graphics.DrawLine(Pens.Black, points[0], points[points.Count - 2]);
                }
            }
        }

        //Очистка
        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Invalidate();
            transformMatrix.Reset();
            points.Clear();
        }

        //
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF newPoint = e.Location;

                // Проверка, является ли новая точка близкой к первой точке
                /*if (points.Count > 0 && IsClose(newPoint, points[0]))
                {
                    // Завершить рисование
                    return;
                }*/

                points.Add(newPoint);
                panel1.Invalidate(); // Перерисовка панели
            }
        }

        //
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
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isRotating = false;
                isTranslating = false;
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && points.Count > 1)
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
                        lastMousePosition = e.Location;
                    }
                    else
                    {
                        isRotating = true;
                        figureCenter = GetPolygonCenter();
                    }
                }
            }
        }

        private bool IsClose(PointF point1, PointF point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2)) <= closeDistance;
        }

        private void ApplyScaling(float scaleX, float scaleY, PointF center)
        {
            Matrix scaleMatrix = new Matrix(scaleX, 0, 0, scaleY, (1 - scaleX) * center.X, (1 - scaleY) * center.Y);

            transformMatrix.Multiply(scaleMatrix, MatrixOrder.Append);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PointF center = GetPolygonCenter();
            ApplyScaling(1.1f, 1.1f, center);
            panel1.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PointF center = GetPolygonCenter();
            ApplyScaling(0.9f, 0.9f, center);
            panel1.Invalidate();
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
            if (points.Count == 0) return PointF.Empty;

            float sumX = 0, sumY = 0;
            foreach (var point in points)
            {
                sumX += point.X;
                sumY += point.Y;
            }
            return new PointF(sumX / points.Count, sumY / points.Count);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
