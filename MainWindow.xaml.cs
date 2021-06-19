using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Shape currentDrawn = null;
        Point firstClick;
        List<Shape> selectedShapes = new List<Shape>();
        List<Point> points = new List<Point>();
        ObservableCollection<ColorInfo> colorInfos;
        // https://stackoverflow.com/questions/23385876/moving-the-dynamically-drawn-rectangle-inside-the-canvas-using-mousemove-event
        bool drag = false;
        Point startPt;
        Point lastLoc;
        double CanvasLeft, CanvasTop;
        // -- koniec wklejenia
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ClearData()
        {
            BindingOperations.ClearBinding(txtWidth, TextBox.TextProperty);
            BindingOperations.ClearBinding(txtHeight, TextBox.TextProperty);
            BindingOperations.ClearBinding(slider, Slider.ValueProperty);
            BindingOperations.ClearBinding(txtColor, ComboBox.SelectedValueProperty);
            txtWidth.IsEnabled = false;
            txtHeight.IsEnabled = false;
            txtColor.IsEnabled = false;
            slider.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnRandom.IsEnabled = false;
            txtWidth.Text = string.Empty;
            txtHeight.Text = string.Empty;
            slider.Value = 0;
            txtColor.SelectedItem = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Na podstawie tutoriala WPF
            var props = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            List<ColorInfo> colors = props.Select(prop =>
            {
                var color = (Color)prop.GetValue(null, null);
                string name = prop.Name;
                for (int i = 1; i < name.Length; i++)
                {
                    if (name[i] >= 'A' && name[i] <= 'Z' && name[i - 1] != ' ')
                    {
                        name = name.Insert(i, " ");
                    }
                }
                return new ColorInfo()
                {
                    Name = name,
                    Rgb = color,
                    InverseRgb = color.R + color.G + color.B > 400 ? Colors.Black : Colors.White
                };
            }).ToList();
            colors.RemoveAll(c => c.Name == "Transparent");
            colorInfos = new ObservableCollection<ColorInfo>(colors);
            DataContext = colorInfos;
            // koniec
            Random random = new Random();
            for(int i=0;i<4;i++)
            {
                int top = random.Next(0, (int)canvas.ActualHeight - 100);
                int left = random.Next(0, (int)canvas.ActualWidth - 100);
                int height = random.Next(100, (int)canvas.ActualHeight - top);
                int width = random.Next(100, (int)canvas.ActualWidth - left);
                Shape shape;
                if (random.Next(0, 2) == 0) shape = new Rectangle();
                else shape = new Ellipse();
                shape.Height = height;
                shape.Width = width;
                shape.Fill = new SolidColorBrush(colorInfos[random.Next(0, colorInfos.Count)].Rgb);
                RotateTransform rotateTransform = new RotateTransform();
                rotateTransform.CenterX = width / 2;
                rotateTransform.CenterY = height / 2;
                shape.RenderTransform = rotateTransform;
                canvas.Children.Add(shape);
                shape.SetValue(Canvas.TopProperty, (double)top);
                shape.SetValue(Canvas.LeftProperty, (double)left);
                shape.MouseEnter += Shape_MouseEnter;
                shape.MouseLeave += Shape_MouseLeave;
                shape.MouseDown += Shape_MouseDown;
                shape.MouseMove += Shape_MouseMove;
                shape.MouseLeftButtonUp += Shape_MouseLeftButtonUp;
            }
        }

        private void Shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // https://stackoverflow.com/questions/23385876/moving-the-dynamically-drawn-rectangle-inside-the-canvas-using-mousemove-event
            drag = false;
            Cursor = Cursors.Arrow;
            Mouse.Capture(null);
        }

        private void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            // https://stackoverflow.com/questions/23385876/moving-the-dynamically-drawn-rectangle-inside-the-canvas-using-mousemove-event
            if (drag)
            {
                var newX = (startPt.X + (e.GetPosition(canvas).X - startPt.X));
                var newY = (startPt.Y + (e.GetPosition(canvas).Y - startPt.Y));
                Point offset = new Point((startPt.X - lastLoc.X), (startPt.Y - lastLoc.Y));
                CanvasTop = newY - offset.Y;
                CanvasLeft = newX - offset.X;
                for (int i = 0; i < selectedShapes.Count; i++)
                {
                    selectedShapes[i].SetValue(Canvas.TopProperty, CanvasTop + points[i].Y);
                    selectedShapes[i].SetValue(Canvas.LeftProperty, CanvasLeft + points[i].X);
                }
            }
        }

        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Shape shape = sender as Shape;
            txtWidth.IsEnabled = true;
            txtHeight.IsEnabled = true;
            txtColor.IsEnabled = true;
            slider.IsEnabled = true;
            Binding bindingWidth = new Binding("Width");
            bindingWidth.Source = shape;
            bindingWidth.Mode = BindingMode.TwoWay;
            bindingWidth.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Binding bindingHeight = new Binding("Height");
            bindingHeight.Source = shape;
            bindingHeight.Mode = BindingMode.TwoWay;
            bindingHeight.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Binding bindingColor = new Binding("Color");
            bindingColor.Source = shape.Fill as SolidColorBrush;
            bindingColor.Mode = BindingMode.TwoWay;
            bindingColor.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Binding bindingAngle = new Binding("Angle");
            bindingAngle.Source = shape.RenderTransform as RotateTransform;
            if(e.ChangedButton == MouseButton.Left && currentDrawn == null)
            {
                if(shape.Effect == null)
                {
                    foreach (var s in canvas.Children)
                    {
                        Shape shape1 = s as Shape;
                        shape1.Effect = null;
                        Panel.SetZIndex(shape1, 0);
                        if(selectedShapes.Contains(shape1)) selectedShapes.Remove(shape1);
                    }
                    points.Clear();
                }
                Panel.SetZIndex(shape, 5);
                DropShadowEffect dropShadowEffect = new DropShadowEffect();
                dropShadowEffect.BlurRadius = 50;
                dropShadowEffect.Direction = 270;
                dropShadowEffect.Color = Colors.White;
                shape.Effect = dropShadowEffect;
                btnDelete.IsEnabled = true;
                btnRandom.IsEnabled = true;
                Point point = new Point(0, 0);
                if (!selectedShapes.Contains(shape))
                {
                    txtWidth.SetBinding(TextBox.TextProperty, bindingWidth);
                    txtHeight.SetBinding(TextBox.TextProperty, bindingHeight);
                    txtColor.SetBinding(ComboBox.SelectedValueProperty, bindingColor);
                    slider.SetBinding(Slider.ValueProperty, bindingAngle);
                    selectedShapes.Add(shape);
                }
                if (selectedShapes.Count <= points.Count)
                {
                    selectedShapes.Remove(shape);
                    points.Remove(point);
                    selectedShapes.Add(shape);
                }
                points.Add(point);
                drag = true;
                Cursor = Cursors.ScrollAll;
                startPt = e.GetPosition(canvas);
                lastLoc = new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape));
                for (int i = 0; i < selectedShapes.Count - 1; i++)
                {
                    points[i] = new Point(Canvas.GetLeft(selectedShapes[i]) - Canvas.GetLeft(shape), Canvas.GetTop(selectedShapes[i]) - Canvas.GetTop(shape));
                }
                Mouse.Capture(shape);
                e.Handled = true;
            }
            else if(e.ChangedButton == MouseButton.Right)
            {
                currentDrawn = null;
                if (shape.Effect != null)
                {
                    shape.Effect = null;
                    Panel.SetZIndex(shape, 0);
                    for(int i=0;i<selectedShapes.Count;i++)
                    {
                        if(selectedShapes[i] == shape)
                        {
                            selectedShapes.RemoveAt(i);
                            points.RemoveAt(i);
                            if(selectedShapes.Count >= 1)
                            {
                                points[points.Count - 1] = new Point(0, 0);
                                for(int j=0;j<points.Count-1;j++)
                                {
                                    points[j] = new Point(Canvas.GetLeft(selectedShapes[j]) - Canvas.GetLeft(selectedShapes[points.Count-1]), Canvas.GetTop(selectedShapes[j]) - Canvas.GetTop(selectedShapes[points.Count-1]));
                                }
                            }
                            break;
                        }
                    }
                    if(selectedShapes.Count == 0)
                    {
                        ClearData();
                    }
                    else
                    {
                        bindingWidth.Source = selectedShapes[selectedShapes.Count-1];
                        txtWidth.SetBinding(TextBox.TextProperty, bindingWidth);
                        bindingHeight.Source = selectedShapes[selectedShapes.Count - 1];
                        txtHeight.SetBinding(TextBox.TextProperty, bindingHeight);
                        bindingColor.Source = (selectedShapes[selectedShapes.Count - 1].Fill as SolidColorBrush);
                        txtColor.SetBinding(ComboBox.SelectedValueProperty, bindingColor);
                        bindingAngle.Source = selectedShapes[selectedShapes.Count-1].RenderTransform as RotateTransform;
                        slider.SetBinding(Slider.ValueProperty, bindingAngle);
                    }
                }
                else
                {
                    Panel.SetZIndex(shape, 5);
                    DropShadowEffect dropShadowEffect = new DropShadowEffect();
                    dropShadowEffect.BlurRadius = 50;
                    dropShadowEffect.Direction = 270;
                    dropShadowEffect.Color = Colors.White;
                    shape.Effect = dropShadowEffect;
                    btnDelete.IsEnabled = true;
                    btnRandom.IsEnabled = true;
                    selectedShapes.Add(shape);
                    startPt = e.GetPosition(canvas);
                    lastLoc = new Point(Canvas.GetLeft(shape), Canvas.GetTop(shape));
                    Point point = new Point(0, 0);
                    txtWidth.SetBinding(TextBox.TextProperty, bindingWidth);
                    txtHeight.SetBinding(TextBox.TextProperty, bindingHeight);
                    txtColor.SetBinding(ComboBox.SelectedValueProperty, bindingColor);
                    slider.SetBinding(Slider.ValueProperty, bindingAngle);
                    if (selectedShapes.Count <= points.Count)
                    {
                        selectedShapes.Remove(shape);
                        points.Remove(point);
                        selectedShapes.Add(shape);
                    }
                    points.Add(point);
                    for (int i = 0; i < selectedShapes.Count - 1; i++)
                    {
                        points[i] = new Point(Canvas.GetLeft(selectedShapes[i]) - Canvas.GetLeft(shape), Canvas.GetTop(selectedShapes[i]) - Canvas.GetTop(shape));
                    }
                }
            }
        }


        private void Shape_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = currentDrawn != null ? Cursors.Cross : Cursors.Arrow;
        }

        

        private void Shape_MouseEnter(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ClearData();
            foreach (Shape shape in selectedShapes)
            {
                canvas.Children.Remove(shape);
            }
            selectedShapes.Clear();
            points.Clear();
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            foreach(Shape shape in selectedShapes)
            {
                Color color = colorInfos[random.Next(0, colorInfos.Count)].Rgb;
                shape.Fill = new SolidColorBrush(color);
                txtColor.SelectedValue = color;
            }
        }

        private void btnRectangle_Click(object sender, RoutedEventArgs e)
        {
            foreach (var s in canvas.Children)
            {
                Shape shape1 = s as Shape;
                shape1.Effect = null;
                Panel.SetZIndex(shape1, 0);
                if (selectedShapes.Contains(shape1)) selectedShapes.Remove(shape1);
            }
            points.Clear();
            ClearData();
            Random random = new Random();
            Cursor = Cursors.Cross;
            currentDrawn = new Rectangle();
            currentDrawn.Fill = new SolidColorBrush(colorInfos[random.Next(0, colorInfos.Count)].Rgb);
        }

        private void btnEllipse_Click(object sender, RoutedEventArgs e)
        {
            foreach (var s in canvas.Children)
            {
                Shape shape1 = s as Shape;
                shape1.Effect = null;
                Panel.SetZIndex(shape1, 0);
                if (selectedShapes.Contains(shape1)) selectedShapes.Remove(shape1);
            }
            points.Clear();
            ClearData();
            Random random = new Random();
            Cursor = Cursors.Cross;
            currentDrawn = new Ellipse();
            currentDrawn.Fill = new SolidColorBrush(colorInfos[random.Next(0, colorInfos.Count)].Rgb);
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (currentDrawn != null)
            {
                var mousePos = e.GetPosition(canvas);
                firstClick = mousePos;
                currentDrawn.SetValue(Canvas.TopProperty, mousePos.Y);
                currentDrawn.SetValue(Canvas.LeftProperty, mousePos.X);
                RotateTransform rotateTransform = new RotateTransform();
                rotateTransform.CenterX = 0;
                rotateTransform.CenterY = 0;
                currentDrawn.RenderTransform = rotateTransform;
                currentDrawn.MouseDown += Shape_MouseDown;
                currentDrawn.MouseEnter += Shape_MouseEnter;
                currentDrawn.MouseLeave += Shape_MouseLeave;
                currentDrawn.MouseMove += Shape_MouseMove;
                currentDrawn.MouseLeftButtonUp += Shape_MouseLeftButtonUp;
                canvas.Children.Add(currentDrawn);
                currentDrawn.CaptureMouse();
            }
            else if(e.Handled == false)
            {
                foreach (var s in canvas.Children)
                {
                    Shape shape1 = s as Shape;
                    shape1.Effect = null;
                    Panel.SetZIndex(shape1, 0);
                    if (selectedShapes.Contains(shape1)) selectedShapes.Remove(shape1);
                }
                points.Clear();
                ClearData();
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(currentDrawn != null)
            {
                var mousePos = e.GetPosition(canvas);
                currentDrawn.Width = (int)Math.Abs(mousePos.X - firstClick.X);
                currentDrawn.Height = (int)Math.Abs(mousePos.Y - firstClick.Y);
                RotateTransform rotateTransform = new RotateTransform();
                rotateTransform.CenterX = currentDrawn.Width / 2;
                rotateTransform.CenterY = currentDrawn.Height / 2;
                currentDrawn.RenderTransform = rotateTransform;
                currentDrawn.SetValue(Canvas.TopProperty, Math.Min(firstClick.Y, mousePos.Y));
                currentDrawn.SetValue(Canvas.LeftProperty, Math.Min(firstClick.X, mousePos.X));
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(currentDrawn != null)
            {
                currentDrawn.ReleaseMouseCapture();
                currentDrawn = null;
            }
        }

        private void btnLanguage_Click(object sender, RoutedEventArgs e)
        {
            byte[] imageData;
            if(btnRectangle.Content.ToString() == "Rectangle")
            {
                btnRectangle.Content = Resource2.Rectangle;
                btnEllipse.Content = Resource2.Ellipse;
                btnDelete.Content = Resource2.Delete;
                lblColor.Content = Resource2.Color;
                lblHeight.Content = Resource2.Height;
                lblWidth.Content = Resource2.Width;
                txtAngle.Text = Resource2.Angle;
                txtExport.Text = Resource2.Export_to__png;
                txtRandom.Text = Resource2.Random_colors;
                // https://stackoverflow.com/questions/9564174/convert-byte-array-to-image-in-wpf
                imageData = Resource2.Flag;
            }
            else
            {
                btnRectangle.Content = Resource1.Rectangle;
                btnEllipse.Content = Resource1.Ellipse;
                btnDelete.Content = Resource1.Delete;
                lblColor.Content = Resource1.Color;
                lblHeight.Content = Resource1.Height;
                lblWidth.Content = Resource1.Width;
                txtAngle.Text = Resource1.Angle;
                txtExport.Text = Resource1.Export_to__png;
                txtRandom.Text = Resource1.Random_colors;
                // https://stackoverflow.com/questions/9564174/convert-byte-array-to-image-in-wpf
                imageData = Resource1.Flag;
            }
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            imgFlag.Source = image;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG file(*.png)|*.png";
            if(saveFileDialog.ShowDialog() == true)
            {
                // Na podstawie: https://stackoverflow.com/questions/21411878/saving-a-canvas-to-png-c-sharp-wpf
                Rect bounds = VisualTreeHelper.GetDescendantBounds(canvas);
                double dpi = 96d;

                RenderTargetBitmap rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, System.Windows.Media.PixelFormats.Default);

                DrawingVisual dv = new DrawingVisual();
                using (DrawingContext dc = dv.RenderOpen())
                {
                    VisualBrush vb = new VisualBrush(canvas);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                rtb.Render(dv);
                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                try
                {
                    MemoryStream ms = new MemoryStream();

                    pngEncoder.Save(ms);
                    ms.Close();

                    File.WriteAllBytes(saveFileDialog.FileName, ms.ToArray());
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.StartsWith('-')) textBox.Text = textBox.Text.Remove(0);
        }

    }
}
