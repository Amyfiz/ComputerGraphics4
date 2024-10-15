using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace figures
{
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
}
