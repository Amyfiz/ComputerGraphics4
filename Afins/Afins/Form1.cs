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
    /*
    public partial class Form1 : Form
    {
        private Bitmap bitmap;
        private List<List<PointF>> shapes;
        private List<PointF> currentShape;
        private ListBox shapeListBox;
        private TextBox angleTextBox;
        private TextBox rotationPointXTextBox;
        private TextBox rotationPointYTextBox;
        private RadioButton centerRotationRadioButton;
        private RadioButton customPointRotationRadioButton;
        private const int drawingAreaWidth = 800;
        private const int drawingAreaHeight = 600;

        public Form1()
        {
            InitializeComponent();

            this.bitmap = new Bitmap(drawingAreaWidth, drawingAreaHeight);
            this.shapes = new List<List<PointF>>();
            this.currentShape = new List<PointF>();

            this.shapeListBox = new ListBox();
            this.shapeListBox.Location = new Point(drawingAreaWidth + 10, 10);
            this.shapeListBox.Size = new Size(150, 580);
            this.Controls.Add(this.shapeListBox);

            Button newShapeButton = new Button();
            newShapeButton.Text = "New Shape";
            newShapeButton.Location = new Point(drawingAreaWidth + 10, 600);
            newShapeButton.Click += new EventHandler(this.OnNewShapeButtonClick);
            this.Controls.Add(newShapeButton);

            Button rotateShapeButton = new Button();
            rotateShapeButton.Text = "Rotate Shape";
            rotateShapeButton.Location = new Point(drawingAreaWidth + 10, 640);
            rotateShapeButton.Click += new EventHandler(this.OnRotateShapeButtonClick);
            this.Controls.Add(rotateShapeButton);

            this.angleTextBox = new TextBox();
            this.angleTextBox.Location = new Point(drawingAreaWidth + 10, 680);
            this.angleTextBox.Text = "Turn by";
            this.Controls.Add(this.angleTextBox);

            this.rotationPointXTextBox = new TextBox();
            this.rotationPointXTextBox.Location = new Point(drawingAreaWidth + 10, 720);
            this.rotationPointXTextBox.Text = "X";
            this.Controls.Add(this.rotationPointXTextBox);

            this.rotationPointYTextBox = new TextBox();
            this.rotationPointYTextBox.Location = new Point(drawingAreaWidth + 10, 760);
            this.rotationPointYTextBox.Text = "Y";
            this.Controls.Add(this.rotationPointYTextBox);

            this.centerRotationRadioButton = new RadioButton();
            this.centerRotationRadioButton.Text = "Сenter";
            this.centerRotationRadioButton.Location = new Point(drawingAreaWidth + 10, 800);
            this.centerRotationRadioButton.Checked = true; // По умолчанию выбран этот режим
            this.Controls.Add(this.centerRotationRadioButton);

            this.customPointRotationRadioButton = new RadioButton();
            this.customPointRotationRadioButton.Text = "Сustom point";
            this.customPointRotationRadioButton.Location = new Point(drawingAreaWidth + 10, 830);
            this.Controls.Add(this.customPointRotationRadioButton);

            this.MouseClick += new MouseEventHandler(this.OnMouseClick);
            this.Paint += new PaintEventHandler(this.OnPaint);
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.X < drawingAreaWidth && e.Y < drawingAreaHeight)
            {
                if (currentShape.Count > 2)
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawLine(new Pen(this.BackColor), currentShape[currentShape.Count - 1], currentShape[0]);
                    }
                }

                currentShape.Add(e.Location);

                if (currentShape.Count > 1)
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawLine(Pens.Black, currentShape[currentShape.Count - 2], currentShape[currentShape.Count - 1]);
                    }
                }

                if (currentShape.Count > 2)
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.DrawLine(Pens.Black, currentShape[currentShape.Count - 1], currentShape[0]);
                    }
                }

                this.Invalidate();
            }
        }

        private void OnNewShapeButtonClick(object sender, EventArgs e)
        {
            if (currentShape.Count > 0)
            {
                shapes.Add(new List<PointF>(currentShape));
                shapeListBox.Items.Add($"Shape {shapes.Count}");
                currentShape.Clear();
            }
        }

        private void OnRotateShapeButtonClick(object sender, EventArgs e)
        {
            if (shapeListBox.SelectedIndex >= 0 && double.TryParse(angleTextBox.Text, out double angle))
            {
                int selectedIndex = shapeListBox.SelectedIndex;
                List<PointF> rotatedShape;

                if (centerRotationRadioButton.Checked)
                {
                    rotatedShape = RotateShape(shapes[selectedIndex], angle, GetShapeCenter(shapes[selectedIndex]));
                }
                else if (customPointRotationRadioButton.Checked &&
                         int.TryParse(rotationPointXTextBox.Text, out int rotationPointX) &&
                         int.TryParse(rotationPointYTextBox.Text, out int rotationPointY))
                {
                    PointF rotationPoint = new PointF(rotationPointX, rotationPointY);
                    rotatedShape = RotateShape(shapes[selectedIndex], angle, rotationPoint);
                }
                else
                {
                    MessageBox.Show("Пожалуйста, введите корректные числовые значения для угла поворота и координат точки поворота.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                shapes[selectedIndex] = rotatedShape;

                // Перерисовать фигуры
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(this.BackColor);
                    foreach (var shape in shapes)
                    {
                        for (int i = 0; i < shape.Count; i++)
                        {
                            PointF p1 = shape[i];
                            PointF p2 = shape[(i + 1) % shape.Count];
                            g.DrawLine(Pens.Black, p1, p2);
                        }
                    }
                }

                this.Invalidate();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректное числовое значение для угла поворота.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private PointF RotatePoint(PointF point, PointF center, double angle)
        {
            double radians = angle * Math.PI / 180;
            double cosTheta = Math.Cos(radians);
            double sinTheta = Math.Sin(radians);

            // Перемещение точки к центру координат
            float x = point.X - center.X;
            float y = point.Y - center.Y;

            // Применение матрицы поворота
            float newX = (float)(x * cosTheta - y * sinTheta);
            float newY = (float)(x * sinTheta + y * cosTheta);

            // Перемещение точки обратно
            return new PointF(newX + center.X, newY + center.Y);
        }

        private List<PointF> RotateShape(List<PointF> shape, double angle, PointF rotationPoint)
        {
            if (shape.Count == 0) return shape;

            // Повернуть каждую точку фигуры относительно заданной точки
            List<PointF> rotatedShape = new List<PointF>();
            foreach (var point in shape)
            {
                rotatedShape.Add(RotatePoint(point, rotationPoint, angle));
            }

            return rotatedShape;
        }

        private PointF GetShapeCenter(List<PointF> shape)
        {
            if (shape.Count == 0) return new PointF(0, 0);

            float sumX = 0, sumY = 0;
            foreach (var point in shape)
            {
                sumX += point.X;
                sumY += point.Y;
            }
            return new PointF(sumX / shape.Count, sumY / shape.Count);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bitmap, 0, 0);
            e.Graphics.DrawRectangle(Pens.Black, 0, 0, drawingAreaWidth - 1, drawingAreaHeight - 1); // Рисуем границу
        }
    }
     */
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
