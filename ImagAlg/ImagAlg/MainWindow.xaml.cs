using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using ImagAlg.Model;
using ImagAlg.Utils;

namespace ImagAlg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyImage myImage;
        Dictionary<String, Assembly> assemblies;
        public MainWindow()
        {

            InitializeComponent();
            assemblies = new Dictionary<string, Assembly>();
            LoadAlgorithms();
            myImage = null;
        }

        private void LoadImage_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*.jpg;*.jpeg;*.jpe;*.jfif;*.png";
            bool? imageSelected = fileDialog.ShowDialog();
            if (imageSelected == true)
            {
                image.Source = new BitmapImage(new Uri(fileDialog.FileName));
                myImage = new MyImage(new Bitmap(fileDialog.FileName));
            }
            ExecuteAlgorithm("GrayScaleAlg", "GrayScale");
        }

        /// <summary>
        /// Load algorithms from Plugins folder
        /// </summary>
        private void LoadAlgorithms()
        {
            //assemblies.Add("test", Assembly.LoadFile(@"C:\workspace\ImagAlg\GrayScaleAlg\bin\Debug\AlgInterface.dll"));
            foreach (string alg in Consts.algorithms)
            {
                string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Plugins\" + alg + ".dll");
                assemblies.Add(alg, Assembly.LoadFile(@"C:\workspace\ImagAlg\GrayScaleAlg\bin\Debug\GrayScaleAlg.dll"));
            }
        }

        private Bitmap ExecuteAlgorithm(string alg, string method)
        {
            try
            {
                var type = assemblies[alg].GetType($"{alg}.{method}");
                var methodInfo = type.GetMethod(Consts.RunAlgMethod);
                if(methodInfo == null)
                {
                    return null;
                }
                var instance = Activator.CreateInstance(type);
                var result = methodInfo.Invoke(instance, new object[] { myImage.bitmap });
                image.Source = ConvertBitmapToImageSource((Bitmap)result);

            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                MessageBox.Show(errorMessage);
            }
            return null;
        }

        private ImageSource ConvertBitmapToImageSource(Bitmap map)
        {
            var handle = map.GetHbitmap();
            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, 
                    Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                return null;
            }
        }
    }
}
