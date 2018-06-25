using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
//using System.Windows.Media.Media3D.Point3D;
//using System.Windows.Media.Media3D.Vector3D;
//using System.Windows.Media.Media3D.Transform3D;
//using System.Windows.Media.Media3D.TranslateTransform3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Math;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Vision.GlyphRecognition;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HelixToolkit.Wpf;
using HelixToolkit;
using AForge.Math.Geometry;

namespace GlyphStartup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GlyphDatabase glyphDatabase;
        private VideoCaptureDevice camera;
        private Gallery CurrentGallery;
        FileModelVisual3D mod;
        private string MODEL_PATH;

        public MainWindow(Gallery currentGallery)
        {
            CurrentGallery = currentGallery;
            WindowInit(CurrentGallery);
        }

        //public MainWindow()
        //{
        //    WindowInit();
        //}

        private void WindowInit(Gallery currentGallery)
        {
            InitializeComponent();
            CurrentGallery = currentGallery;
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            camera = new VideoCaptureDevice(videoDevices[0].MonikerString);
            camera.NewFrame += new NewFrameEventHandler(camera_NewFrame);
            camera.Start();
            glyphDatabase = new GlyphDatabase(5);

            //BoxVisual3D box = new BoxVisual3D();
            //Point3D position = new Point3D(3, 4, 5);
            //Point3D.Transform position = new TranslateTransform3D(new Vector3(3, 4, 5));
            //box.Center = position;
            //viewPort3d.Children.Add(box);
            //TranslateManipulator manipulator1 = new TranslateManipulator();
            //manipulator1.Bind(box);
            //manipulator1.Color = Colors.Red;
            //manipulator1.Direction = new Vector3D(1, 0, 0);
            //manipulator1.Diameter = 0.1;
            //manipulator1.Position = position; 
            //viewPort3d.Children.Add(manipulator1);



            String glyphInformationPath = CurrentGallery.UrlPrefix + "\\" + CurrentGallery.GlyphsInformation;
            String json = File.ReadAllText(@glyphInformationPath);
            dynamic dynJson = JsonConvert.DeserializeObject(json);
            List<HelpGlyph> glyphsFromJson = JsonConvert.DeserializeObject<List<HelpGlyph>>(json);
            createGlyphDatabase(glyphsFromJson);

            //glyphDatabase = new GlyphDatabase(5);
            //glyphDatabase.Add(new Glyph("glyph1", new byte[5,5]{
            //    {0,0,0,0,0},
            //    {0,0,1,0,0},
            //    {0,1,1,1,0},
            //    {0,0,0,1,0},
            //    {0,0,0,0,0}
            //}));     
            //Za suzdavane na json s glyphs
            //String testJson = JsonConvert.SerializeObject(glyphDatabase);
            //Console.WriteLine(testJson);

            //Model3DGroup group = new ModelImporter().Load(@"C:\\deer-3ds.3ds");
            //mod = new FileModelVisual3D();
            //mod.ModelLoaded += new RoutedEventHandler(mod_ModelLoaded);
            //mod.Source = "C:\\deer-3ds.3ds";


          

        }

        void createModelPath(Gallery CurrentGallery, String recognizedGlyphName)
        {
            MODEL_PATH = CurrentGallery.UrlPrefix + "\\" + recognizedGlyphName + ".3ds";

        }

        void visualisation3Dmodel(String MODEL_PATH, double x, double y, double z, Vector3D translateVector)
        {            
            ModelVisual3D device3D = new ModelVisual3D();
            var center = new TranslateTransform3D(0, 0, 0);
            device3D.Content = Display3d(MODEL_PATH);            
            // Add to view port
            viewPort3d.Children.Clear();
            viewPort3d.Children.Add(new DefaultLights());
            //viewPort3d.CameraController.AddZoomForce(-0.1);

            viewPort3d.Children.Add(device3D);
            var rot_x = new AxisAngleRotation3D(new Vector3D(1, 0, 0), x * 180/ Math.PI);
            var rot_y = new AxisAngleRotation3D(new Vector3D(0, 1, 0), y * 180 / Math.PI);
            var rot_z = new AxisAngleRotation3D(new Vector3D(0, 0, 1), z * 180 / Math.PI); //  z * 180 / Math.PI = 1 0.5....
            //device width and height
            double width = System.Windows.SystemParameters.WorkArea.Width;            
            double height = System.Windows.SystemParameters.WorkArea.Height; ;
            var zoom = new ScaleTransform3D( x - width / y - 10, height/ 10, 0.5 );
            //device3D.Transform = new ScaleTransform3D(width / 10, height/ 10, 100);
           
            Transform3DGroup t = new Transform3DGroup();
            var t1 = new TranslateTransform3D(translateVector);
            //t.Children.Add(zoom);
          //  t.Children.Add(t1);            
            t.Children.Add(new RotateTransform3D(rot_y));
            t.Children.Add(new RotateTransform3D(rot_x));
            t.Children.Add(new RotateTransform3D(rot_z));
            t.Children.Add(center);           
            viewPort3d.Camera.Transform = t;
       }

        void createGlyphDatabase(List<HelpGlyph> glyphsFromJson)
        {
            foreach (var glyph in glyphsFromJson)
            {
                byte[,] currentArrayByte = new byte[5, 5];
                int i = 0;
                int j = 0;
                byte currentByte;
                Type type = glyph.GetType();

                foreach (var row in glyph.Data)
                {
                    type = currentArrayByte.GetType();
                    foreach (var element in row)
                    {
                        currentByte = Convert.ToByte(element);
                        currentArrayByte[i, j] = currentByte;
                        j++;
                        type = currentByte.GetType();
                    }
                    j = 0;
                    i++;
                }
                glyphDatabase.Add(new Glyph(glyph.Name, currentArrayByte));
                currentArrayByte = null;
            }
        }

        private Model3D Display3d(string model)
        {
            Model3D device = null;
            try
            {
                //Adding a gesture here
                viewPort3d.RotateGesture = new MouseGesture(MouseAction.LeftClick);                

                //Import 3D model file
                ModelImporter import = new ModelImporter();

                //Load the 3D model file                
                device = import.Load(model);
            }
            catch (Exception e)
            {
                // Handle exception in case can not find the 3D model file
                MessageBox.Show("Exception Error : " + e.StackTrace);
            }
            return device;
        }

        void mod_ModelLoaded(object sender, RoutedEventArgs e)
        {
            ModelArea.Children.Add(mod);
        }


        void camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            float sizeHalf = 113.0f / 2;
            Vector3[] glyphModel = new Vector3[]
            {
                new Vector3( -sizeHalf, 0,  sizeHalf ),
                new Vector3(  sizeHalf, 0,  sizeHalf ),
                new Vector3(  sizeHalf, 0, -sizeHalf ),
                new Vector3( -sizeHalf, 0, -sizeHalf ),
            };
                  try
            {
                GlyphRecognizer gr = new GlyphRecognizer(glyphDatabase);
                List<ExtractedGlyphData> glyphs = gr.FindGlyphs(eventArgs.Frame);

                System.Drawing.Image img = (Bitmap)eventArgs.Frame.Clone();
                double x = 0;
                double y = 0;
                String glyphName = String.Empty;
                AForge.Point[] glyphPoints = new AForge.Point[4];
                foreach (var glyph in glyphs)
                {
                    if (glyph.RecognizedGlyph != null)
                    {
                        glyphName = glyph.RecognizedGlyph.Name;
                        x = (glyph.Quadrilateral[0].X + glyph.Quadrilateral[2].X) / 2;
                        y = (glyph.Quadrilateral[0].Y + glyph.Quadrilateral[2].Y) / 2;
                        
                        for (int i = 0; i < 4; i++)
                        {
                            glyphPoints[i] = new AForge.Point(
                                (int)(glyph.RecognizedQuadrilateral[i].X - x),
                                (int)y - glyph.RecognizedQuadrilateral[i].Y);
                        }
                        break;
                    }

                }
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    //Canvas.SetLeft(pictureGlyph, x-60);
                    //Canvas.SetTop(pictureGlyph, y-60); 
                    image1.Source = bi;
                    Title = "Found glyphs " + glyphs.Count + " " + glyphName;                    
                    if (glyphName != "")
                    {
                        Dispatcher.BeginInvoke(new ThreadStart(delegate
                        {
                            Console.WriteLine(MODEL_PATH);
                            MODEL_PATH = CurrentGallery.UrlPrefix + "\\" + glyphName + ".3ds";
                            float yaw, pitch, roll; 
                            CoplanarPosit posit = new CoplanarPosit(glyphModel, 1);
                            Matrix3x3 positRotation;
                            Vector3 positTranslation;
                            posit.EstimatePose(glyphPoints, out positRotation, out positTranslation);
                            positRotation.ExtractYawPitchRoll(out yaw, out pitch, out roll);
                            visualisation3Dmodel(MODEL_PATH, yaw, pitch, roll, new Vector3D(positTranslation.X, positTranslation.Y, positTranslation.Z));
                        }));
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            camera.Stop();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            GalleryChooser galleryChooser = new GalleryChooser();
            this.Close();
            galleryChooser.Show();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
