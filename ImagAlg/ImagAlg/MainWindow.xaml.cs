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
            foreach (string alg in Consts.algorithms)
            {
                string path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Plugins\" + alg + ".dll");
                assemblies.Add(alg, Assembly.LoadFile(path));
            }
        }

        private void ExecuteAlgorithm(string alg, string method)
        {
            try
            {
                var type = assemblies[alg].GetType($"{alg}.{method}");
                var methodInfo = type.GetMethod(Consts.RunAlgMethod);
                if(methodInfo == null)
                {
                    MessageBox.Show("Could not find algorithm");
                    return;
                }
                var instance = Activator.CreateInstance(type);
                var result = methodInfo.Invoke(instance, new object[] { myImage.bitmap });
                ImageSource finalImage = ConvertBitmapToImageSource((Bitmap)result);
                if(finalImage != null)
                {
                    image.Source = finalImage;
                }
                else
                {
                    MessageBox.Show("Could not convert image");
                }
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
