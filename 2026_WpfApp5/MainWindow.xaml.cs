using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace _2026_WpfApp5
{
    public partial class MainWindow : Window
    {
        private Color strokeColor = Colors.Black;
        private Color fillColor = Colors.Transparent;
        private int strokeThickness = 1;
        private string shapeType = "Line";
        private string actionType = "Draw";

        private Point start = new Point(0, 0);
        private Point end = new Point(0, 0);

        public MainWindow()
        {
            InitializeComponent();
            StrokeColorPicker.SelectedColor = strokeColor;
            FillColorPicker.SelectedColor = fillColor;
            DisplayStatus();
        }

        private void ShapeButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton targetShapeButton && targetShapeButton.Tag != null)
            {
                shapeType = targetShapeButton.Tag.ToString();
                actionType = "Draw";
                DisplayStatus();
            }
        }

        private void StrokeColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                strokeColor = e.NewValue.Value;
                DisplayStatus();
            }
        }

        private void FillColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                fillColor = e.NewValue.Value;
                DisplayStatus();
            }
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            strokeThickness = (int)e.NewValue;
            DisplayStatus();
        }

        private void DisplayStatus()
        {
            if (StatusLabel != null)
            {
                if (actionType == "Draw")
                {
                    StatusLabel.Content = $"圖形：{shapeType}  座標：({(int)start.X}, {(int)start.Y}) - ({(int)end.X}, {(int)end.Y})";
                }
                else
                {
                    StatusLabel.Content = $"工作模式：{actionType}";
                }
            }

            if (ColorLabel != null)
            {
                ColorLabel.Content = $"筆刷色彩:{strokeColor}     填充色彩:{fillColor}     筆刷粗細：{strokeThickness}";
            }

            if (ShapeLabel != null)
            {
                ShapeLabel.Content = $"形狀：{shapeType}";
            }
        }

        // ==================== 繪圖與游標互動邏輯 ====================

        private void MyCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            MyCanvas.Cursor = (actionType == "Eraser") ? Cursors.Hand : Cursors.Pen;
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            start = e.GetPosition(MyCanvas);
            end = start;

            if (actionType == "Draw")
            {
                MyCanvas.Cursor = Cursors.Cross;

                switch (shapeType)
                {
                    case "Line":
                        Line line = new Line
                        {
                            X1 = start.X,
                            Y1 = start.Y,
                            X2 = end.X,
                            Y2 = end.Y,
                            Stroke = Brushes.Gray,
                            StrokeThickness = 1
                        };
                        MyCanvas.Children.Add(line);
                        break;

                    case "Rectangle":
                        Rectangle rect = new Rectangle
                        {
                            Stroke = Brushes.Gray,
                            Fill = Brushes.LightGray
                        };
                        Canvas.SetLeft(rect, start.X);
                        Canvas.SetTop(rect, start.Y);
                        MyCanvas.Children.Add(rect);
                        break;

                    case "Ellipse":
                        Ellipse ellipse = new Ellipse
                        {
                            Stroke = Brushes.Gray,
                            Fill = Brushes.LightGray
                        };
                        Canvas.SetLeft(ellipse, start.X);
                        Canvas.SetTop(ellipse, start.Y);
                        MyCanvas.Children.Add(ellipse);
                        break;

                    case "Polyline":
                        Polyline polyline = new Polyline
                        {
                            Stroke = Brushes.Gray,
                            Fill = Brushes.LightGray
                        };
                        polyline.Points.Add(start);
                        MyCanvas.Children.Add(polyline);
                        break;
                }
            }
        }

        private void MyCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            end = e.GetPosition(MyCanvas);
            DisplayStatus();

            switch (actionType)
            {
                case "Draw":
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point origin = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                        double width = Math.Abs(end.X - start.X);
                        double height = Math.Abs(end.Y - start.Y);

                        switch (shapeType)
                        {
                            case "Line":
                                var line = MyCanvas.Children.OfType<Line>().LastOrDefault();
                                if (line != null)
                                {
                                    line.X2 = end.X;
                                    line.Y2 = end.Y;
                                }
                                break;

                            case "Rectangle":
                                var rect = MyCanvas.Children.OfType<Rectangle>().LastOrDefault();
                                if (rect != null)
                                {
                                    rect.Width = width;
                                    rect.Height = height;
                                    Canvas.SetLeft(rect, origin.X);
                                    Canvas.SetTop(rect, origin.Y);
                                }
                                break;

                            case "Ellipse":
                                var ellipse = MyCanvas.Children.OfType<Ellipse>().LastOrDefault();
                                if (ellipse != null)
                                {
                                    ellipse.Width = width;
                                    ellipse.Height = height;
                                    Canvas.SetLeft(ellipse, origin.X);
                                    Canvas.SetTop(ellipse, origin.Y);
                                }
                                break;

                            case "Polyline":
                                var polyline = MyCanvas.Children.OfType<Polyline>().LastOrDefault();
                                if (polyline != null)
                                {
                                    polyline.Points.Add(end);
                                }
                                break;
                        }
                    }
                    break;

                case "Eraser":
                    MyCanvas.Cursor = Cursors.Hand;
                    // 當處於橡皮擦模式，且游標移過 Shape 圖形時移除該圖形
                    if (e.OriginalSource is Shape targetShape)
                    {
                        MyCanvas.Children.Remove(targetShape);
                    }
                    break;
            }
        }

        private void MyCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (actionType != "Draw") return;

            MyCanvas.Cursor = Cursors.Pen;
            Brush strokeBrush = new SolidColorBrush(strokeColor);
            Brush fillBrush = new SolidColorBrush(fillColor);

            // 繪圖結束時，套用最終設定的筆刷顏色與粗細
            switch (shapeType)
            {
                case "Line":
                    var line = MyCanvas.Children.OfType<Line>().LastOrDefault();
                    if (line != null)
                    {
                        line.Stroke = strokeBrush;
                        line.StrokeThickness = strokeThickness;
                    }
                    break;

                case "Rectangle":
                    var rect = MyCanvas.Children.OfType<Rectangle>().LastOrDefault();
                    if (rect != null)
                    {
                        rect.Stroke = strokeBrush;
                        rect.Fill = fillBrush;
                        rect.StrokeThickness = strokeThickness;
                    }
                    break;

                case "Ellipse":
                    var ellipse = MyCanvas.Children.OfType<Ellipse>().LastOrDefault();
                    if (ellipse != null)
                    {
                        ellipse.Stroke = strokeBrush;
                        ellipse.Fill = fillBrush;
                        ellipse.StrokeThickness = strokeThickness;
                    }
                    break;

                case "Polyline":
                    var polyline = MyCanvas.Children.OfType<Polyline>().LastOrDefault();
                    if (polyline != null)
                    {
                        polyline.Stroke = strokeBrush;
                        polyline.Fill = fillBrush;
                        polyline.StrokeThickness = strokeThickness;
                    }
                    break;
            }
        }

        // ==================== 按鈕事件處理 ====================

        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            actionType = "Eraser";
            MyCanvas.Cursor = Cursors.Hand;
            DisplayStatus();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            actionType = "Clear";
            MyCanvas.Children.Clear();
            DisplayStatus();
            actionType = "Draw";
            MyCanvas.Cursor = Cursors.Pen;
        }
    }
}