using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Math.Annotations;

// ReSharper disable All

namespace Math
{
    using Face = System.Tuple<int, int, int, int>;

    /// <summary>
    /// Interaction logic for CubeRenderer.xaml
    /// </summary>
    public partial class CubeRenderer : UserControl, INotifyPropertyChanged
    {
        #region Initialize Class
        public CubeRenderer()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #endregion

        #region RenderQuality Enmus

        /// <summary>
        /// This thing basically is just an enum to Control the Shadow Quality
        /// </summary>
        enum ShadowQualityEnum
        {
            Quality = 0,
            Performance = 1
        }

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Dependency Property for the Cube Size Multiplier (Default Value: 1)
        /// </summary>
        private DependencyProperty CubeMultProperty = DependencyProperty.Register("CubeMult", typeof(double),
            typeof(CubeRenderer), new PropertyMetadata(1.0d));

        public double CubeMult
        {
            get { return (double)GetValue(CubeMultProperty); }
            set
            {
                this.SetValue(CubeMultProperty, value);
                if (value <= 0) return;
                sizeMult = value;
            }
        }

        /// <summary>
        /// Dependency Property for the Cube Size Multiplier (Default Value: 1)
        /// </summary>
        DependencyProperty CubeOpacityProperty = DependencyProperty.Register("CubeOpacity", typeof(double),
            typeof(CubeRenderer), new PropertyMetadata(0.8d));

        public double CubeOpacity
        {
            get { return (double) this.GetValue(CubeOpacityProperty); }
            set
            {
                this.SetValue(CubeOpacityProperty, value);
                if (value <= 0) return;
                for (var i = 0; i < CubeSideBrushes.Count; i++)
                {
                    var cubeSideBrush = CubeSideBrushes[i];
                    var item = (byte) System.Math.Clamp((255 * value), 0, 255);
                    CubeSideBrushes[i] =
                        new SolidColorBrush(
                            Color.FromArgb(item,
                                cubeSideBrush.Color.R,
                                cubeSideBrush.Color.G,
                                cubeSideBrush.Color.B));
                }
            }
        }

        /// <summary>
        /// Dependency Property for the Cube Line Size (Default value 1.5)
        /// </summary>
        DependencyProperty CubeLineThicknessProperty = DependencyProperty.Register(
            "CubeLineThickness", 
            typeof(double),
            typeof(CubeRenderer), 
            new PropertyMetadata(1.5));

        public double CubeLineThickness
        {
            get { return (double) this.GetValue(CubeLineThicknessProperty); }
            set
            {
                this.SetValue(CubeLineThicknessProperty, value);
                if (value <= 0) return;
                lineThickness = value;
            }
        }


        /// <summary>
        /// Dependency if Shadows should be drawn or not (Default Value: True)
        /// </summary>
        public static readonly DependencyProperty DrawShadowsProperty = DependencyProperty.Register("DrawShadows", typeof(bool),
            typeof(CubeRenderer), new PropertyMetadata(true));

        public bool DrawShadows
        {
            get { return (bool) this.GetValue(DrawShadowsProperty); }
            set
            {
                this.SetValue(DrawShadowsProperty, value);
                drawShadows = value;
            }
        }

        /// <summary>
        /// Sets the interval in Milliseconds to update the Square (Default Value: 10ms)
        /// </summary>
        DependencyProperty RuntimeFrequencyProperty = DependencyProperty.Register("RuntimeFrequency", typeof(int),
            typeof(CubeRenderer), new PropertyMetadata(1));

        public int RuntimeFrequency
        {
            get { return (int) this.GetValue(RuntimeFrequencyProperty); }
            set
            {
                this.SetValue(RuntimeFrequencyProperty, value);
                if (Timer != null)
                    Timer.Change(0, value);
            }
        }

        /// <summary>
        /// Sets the shadow Quality depending on an Integral Value (0 = Quality, 1 = Performance)
        /// </summary>
        DependencyProperty ShadowQualityProperty = DependencyProperty.Register("ShadowQuality", typeof(int),
            typeof(CubeRenderer), new PropertyMetadata(0));

        public int ShadowQuality
        {
            get { return (int) this.GetValue(ShadowQualityProperty); }
            set
            {
                this.SetValue(ShadowQualityProperty, value);
                var shadowQuality = System.Math.Clamp(value, 0, 1);
                this.shadowQuality = (ShadowQualityEnum) shadowQuality;
            }
        }

        /// <summary>
        /// Sets the Direction in degrees the Shadow is being cast
        /// </summary>
        DependencyProperty ShadowDirectionProperty = DependencyProperty.Register("ShadowDirection", typeof(double),
            typeof(CubeRenderer), new PropertyMetadata(190d));

        public double ShadowDirection
        {
            get { return (double)this.GetValue(ShadowDirectionProperty); }
            set
            {
                this.SetValue(ShadowDirectionProperty, value);
            }
        }

        /// <summary>
        /// Sets the Depth of the Shadow
        /// </summary>
        DependencyProperty ShadowDepthProperty = DependencyProperty.Register("ShadowDepth", typeof(double),
            typeof(CubeRenderer), new PropertyMetadata(10d));

        public double ShadowDepth
        {
            get { return (double)this.GetValue(ShadowDepthProperty); }
            set
            {
                this.SetValue(ShadowDepthProperty, value);
            }
        }

        /// <summary>
        /// Sets the Opacity of the Shadow (Max value)
        /// </summary>
        DependencyProperty ShadowOpacityProperty = DependencyProperty.Register("ShadowOpacity", typeof(double),
            typeof(CubeRenderer), new PropertyMetadata(0.7d));

        public double ShadowOpacity
        {
            get { return (double)this.GetValue(ShadowOpacityProperty); }
            set
            {
                this.SetValue(ShadowOpacityProperty, value);
            }
        }

        /// <summary>
        /// Sets the blur radius of the shadow (makes it softer or harder)
        /// </summary>
        DependencyProperty ShadowBlurRadiusProperty = DependencyProperty.Register("ShadowBlurRadius", typeof(double),
            typeof(CubeRenderer), new PropertyMetadata(40d));

        public double ShadowBlurRadius
        {
            get { return (double)this.GetValue(ShadowBlurRadiusProperty); }
            set
            {
                this.SetValue(ShadowBlurRadiusProperty, value);
            }
        }

        /// <summary>
        /// Sets the shadow Quality depending on an Integral Value (0 = Quality, 1 = Performance)
        /// </summary>
        DependencyProperty ColorCubeProperty = DependencyProperty.Register("ColorCube", typeof(bool),
            typeof(CubeRenderer), new PropertyMetadata(false));

        public bool ColorCube
        {
            get { return (bool) this.GetValue(ColorCubeProperty); }
            set
            {
                this.SetValue(ColorCubeProperty, value);
                if (value)
                    CubeSideBrushes = new List<SolidColorBrush>()
                    {
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[0].Color.A, 255, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[1].Color.A, 255, 255, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[2].Color.A, 255, 10, 150)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[3].Color.A, 0, 255, 255)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[4].Color.A, 0, 255, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[5].Color.A, 0, 0, 255))
                    };
                else
                    CubeSideBrushes = new List<SolidColorBrush>()
                    {
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[0].Color.A, 0, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[1].Color.A, 0, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[2].Color.A, 0, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[3].Color.A, 0, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[4].Color.A, 0, 0, 0)),
                        new SolidColorBrush(Color.FromArgb(CubeSideBrushes[5].Color.A, 0, 0, 0))
                    };
            }
        }


        /// <summary>
        /// Sets the blur radius of the shadow (makes it softer or harder)
        /// </summary>
        DependencyProperty CubeRotationSpeedProperty = DependencyProperty.Register("CubeRotationSpeed", typeof(double),
            typeof(CubeRenderer), new PropertyMetadata(0.1d));

        public double CubeRotationSpeed
        {
            get { return (double)this.GetValue(CubeRotationSpeedProperty); }
            set
            {
                this.SetValue(CubeRotationSpeedProperty, value);
                cubeRotationSpeed = value;
            }
        }






        #endregion Dependency Properties

        #region Required Fields

        /// <summary>
        /// The List for the Polygons used to draw them inside of the UI.
        /// </summary>
        private List<UIElement> Polygons = new List<UIElement>();

        /// <summary>
        /// List of Polygons used for rendering a cube.
        /// Do not change this if you don't wanna break everything.
        /// This is insanely tedious to get working and setup again.
        /// You could however change this to render different shapes.
        /// </summary>
        private List<KeyValuePair<int, Tuple<int, int, int, int>>> PolygonList =
            new List<KeyValuePair<int, Tuple<int, int, int, int>>>()
            {
                new KeyValuePair<int, Tuple<int, int, int, int>>(0,
                    new Tuple<int, int, int, int>(0b000, 0b100, 0b101, 0b001)),
                new KeyValuePair<int, Tuple<int, int, int, int>>(1,
                    new Tuple<int, int, int, int>(0b000, 0b100, 0b110, 0b010)),
                new KeyValuePair<int, Tuple<int, int, int, int>>(2,
                    new Tuple<int, int, int, int>(0b010, 0b110, 0b111, 0b011)),
                new KeyValuePair<int, Tuple<int, int, int, int>>(3,
                    new Tuple<int, int, int, int>(0b011, 0b111, 0b101, 0b001)),
                new KeyValuePair<int, Tuple<int, int, int, int>>(4,
                    new Tuple<int, int, int, int>(0b100, 0b110, 0b111, 0b101)),
                new KeyValuePair<int, Tuple<int, int, int, int>>(5,
                    new Tuple<int, int, int, int>(0b000, 0b010, 0b011, 0b001))
            };

        /// <summary>
        /// The brushes used for each of the different 6 Faces. These are sticky depending on index inside of the polygon array. (Key of the KVP)
        /// </summary>
        private List<SolidColorBrush> CubeSideBrushes = new List<SolidColorBrush>()
        {
            new SolidColorBrush(Color.FromArgb(124, 0, 0, 0)),
            new SolidColorBrush(Color.FromArgb(124, 0, 0, 0)),
            new SolidColorBrush(Color.FromArgb(124, 0, 0, 0)),
            new SolidColorBrush(Color.FromArgb(124, 0, 0, 0)),
            new SolidColorBrush(Color.FromArgb(124, 0, 0, 0)),
            new SolidColorBrush(Color.FromArgb(124, 0, 0, 0))
        };

        /// <summary>
        /// The Points of the Cube to outline how the cube should look like
        /// </summary>
        private List<Point3D> Cube = new List<Point3D>();

        /// <summary>
        /// The original Cube without any Transformation applied from rotation
        /// </summary>
        private List<Point3D> OriginCube = new List<Point3D>();

        /// <summary>
        /// Size of the Square
        /// </summary>

        private double SquareSize
        {
            get => _squareSize;
            set
            {
                _squareSize = value * sizeMult;
                // OnPropertyChanged();
            }
        }

        private double OriginX;
        private double OriginY;
        private static Timer Timer;
        private double _squareSize = 30;
        private double lineThickness;
        private double cubeRotationSpeed;
        private ShadowQualityEnum shadowQuality = ShadowQualityEnum.Quality;

        /// <summary>
        /// Multiplier of the Square Size usually only set through UI
        /// </summary>
        private double sizeMult = 1;

        private bool drawShadows = true;

        /// <summary>
        /// Used to track fps
        /// </summary>
        Stopwatch stpStopwatch = new Stopwatch();

        #endregion Required Fields

        #region Cube Render Stuff

        /// <summary>
        /// Update the Square... DUH
        /// </summary>
        /// <param name="state"></param>
        private void UpdateSquare(object? state)
        {
            if (Application.Current == null) return;
            var time = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            var matrix = Matrix3D.Identity;
            matrix.Rotate(new Quaternion(new Vector3D(1, 1, 1), (cubeRotationSpeed * time.TotalMilliseconds)));
            //Project square on 2d Space
            List<Point> TwoDEEPointList = new List<Point>();

            //Determine Rotation of the Origin Cube
            for (var i = 0; i < OriginCube.Count; i++)
            {
                var point = OriginCube[i];
                var rotatedPoint = matrix.Transform(point);
                Cube[i] = rotatedPoint;
                Point twodeepoint = new Point();
                twodeepoint.X = rotatedPoint.X * (SquareSize * sizeMult) + OriginX;
                twodeepoint.Y = rotatedPoint.Y * (SquareSize * sizeMult) + OriginY;
                TwoDEEPointList.Add(twodeepoint);
            }

            //Sort the Polygons to render those who are furthest away from the object last.
            PolygonList.Sort(new Comparison<KeyValuePair<int, Face>>(SortPolygonList));


            for (var i = 0; i < Polygons.Count; i++)
            {
                var uiElement = Polygons[i];
                if (!(uiElement is Polygon polygon)) continue;

                var value = PolygonList[i].Value;
                var key = PolygonList[i].Key;
                var currentIndex = i;
                var point1 = TwoDEEPointList[value.Item1];
                var point2 = TwoDEEPointList[value.Item2];
                var point3 = TwoDEEPointList[value.Item3];
                var point4 = TwoDEEPointList[value.Item4];


                if (Application.Current == null) return;

                //Update the Polygon Points
                Application.Current.Dispatcher.Invoke(() =>
                {
                    polygon.Points = new PointCollection(new List<Point>()
                    {
                        new Point(point1.X, point1.Y),
                        new Point(point2.X, point2.Y),
                        new Point(point3.X, point3.Y),
                        new Point(point4.X, point4.Y)
                    });

                    //This should drop shadows but currently it doesn't work properly. probably direction and other things mess it up
                    polygon.Fill = CubeSideBrushes[key];
                    if (polygon.StrokeThickness != lineThickness)
                    {
                        polygon.StrokeThickness = lineThickness;
                    }

                    if (currentIndex < 2 && drawShadows)
                    {
                        if (shadowQuality == ShadowQualityEnum.Quality)
                        {
                            polygon.Effect = new DropShadowEffect()
                            {
                                ShadowDepth = ShadowDepth,
                                BlurRadius = ShadowBlurRadius,
                                Direction = ShadowDirection,
                                Opacity = ShadowOpacity,
                                RenderingBias = RenderingBias.Quality,
                                Color = Color.FromArgb(150, 0, 0, 0)
                            };
                        }

                        if (shadowQuality == ShadowQualityEnum.Performance)
                        {
                            polygon.Effect = new DropShadowEffect()
                            {
                                ShadowDepth = ShadowDepth,
                                BlurRadius = ShadowBlurRadius,
                                Direction = ShadowDirection,
                                Opacity = ShadowOpacity,
                                RenderingBias = RenderingBias.Performance,
                                Color = Color.FromArgb(150, 0, 0, 0)
                            };
                        }
                    }
                    else
                    {
                        polygon.Effect = null;
                    }
                });
            }

            stpStopwatch.Stop();

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (stpStopwatch.ElapsedMilliseconds > 0)
                {
                    RenderTimeTextBlock.Text = "Time for last render: " + stpStopwatch.ElapsedMilliseconds +
                                               "ms\nCurrent FPS: " + 1000 / stpStopwatch.ElapsedMilliseconds;
                }
            });
            stpStopwatch = new Stopwatch();
            stpStopwatch.Start();
        }

        /// <summary>
        /// Convert Tuple to Enumerable
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private static IEnumerable TupleToEnumerable(object tuple)
        {
            // You can check if type of tuple is actually Tuple
            return tuple.GetType()
                .GetProperties()
                .Select(property => property.GetValue(tuple));
        }


        /// <summary>
        /// Sort list
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int SortPolygonList(KeyValuePair<int, Face> x, KeyValuePair<int, Face> y)
        {
            var sumofZX = 0d;
            foreach (var value in TupleToEnumerable(x.Value))
            {
                if (!(value is int i)) continue;
                var vertex = Cube[i];
                sumofZX += vertex.Z;
            }

            var sumOfZY = 0d;
            foreach (var value in TupleToEnumerable(y.Value))
            {
                if (!(value is int i)) continue;
                var vertex = Cube[i];
                sumOfZY += vertex.Z;
            }


            if (sumofZX < sumOfZY) return -1;
            if (sumofZX > sumOfZY) return 1;
            return 0;
        }


        /// <summary>
        /// Called once the UI elements are Initialized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CubeRenderer_OnLoaded(object sender, RoutedEventArgs e)
        {
            OriginX = ActualWidth / 2;
            OriginY = ActualHeight / 2;

            //Create the edges of the Square
            for (int i = 0; i < 8; i++)
            {
                Point3D point = new Point3D();
                point.X = ((i & 1) > 0) ? 1.0 : -1.0;
                point.Y = ((i & 2) > 0) ? 1.0 : -1.0;
                point.Z = ((i & 4) > 0) ? 1.0 : -1.0;
                Cube.Add(point);
                OriginCube.Add(point);
            }

            //Create the Polygons of the Square
            for (int i = 0; i < 6; i++)
            {
                Polygon polygon = new Polygon();
                polygon.Name = "Polygon" + i;
                polygon.StrokeThickness = CubeLineThickness;
                polygon.Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
                Polygons.Add(polygon);
                Grid.Children.Add(polygon);
            }

            //We are just re-setting everything here to make sure stuff is enabled. During runtime this would not be an issue because 
            //of PropertyChanged Handlers
            ColorCube = ColorCube;
            CubeOpacity = CubeOpacity;
            ShadowQuality = ShadowQuality;
            CubeMult = CubeMult;
            CubeLineThickness = CubeLineThickness;
            RuntimeFrequency = RuntimeFrequency;
            DrawShadows = DrawShadows;
            cubeRotationSpeed = CubeRotationSpeed;


            Timer = new Timer(UpdateSquare, null, 0, 10);



        }


        /// <summary>
        /// Called when Resizing the Window. This will Recenter the Cube back to the Origin and.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CubeRenderer_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OriginX = ActualWidth / 2;
            OriginY = ActualHeight / 2;
        }

        #endregion

        #region Interface Implementations

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Interface Implementations
    }
}