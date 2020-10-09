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
using AlgInterface;
using System.Linq;

namespace ImagAlg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyImage myImage;
        List<Assembly> assemblies;
        public List<string> pluginNames { get; set; }

        public MainWindow()
        {

            InitializeComponent();
            assemblies = new List<Assembly>();
            pluginNames = new List<string>();
            LoadAlgorithms();
            AddPluginsToComboBox();
            myImage = null;
            DataContext = this;
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
            ExecuteAlgorithm();
        }

        /// <summary>
        /// Load algorithms from Plugins folder
        /// </summary>
        private void LoadAlgorithms()
        {

            string pluginsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Plugins\");
            string[] files = Directory.GetFiles(pluginsPath, "*.dll");
            foreach (string file in files)
            {
                //Read assembly info without loading it into memory.
                var asmDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(file);
                if(IsAssemblyImplementsInterface(asmDef))
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    assemblies.Add(assembly);
                    pluginNames.Add(assembly.GetName().Name);
                }
            }
        }

        private bool IsAssemblyImplementsInterface(Mono.Cecil.AssemblyDefinition asm)
        {
            Type interfaceType = typeof(IImageProcessingAlgorithm);
            // check if assembly contains a class that implements the IImageProcessingAlgorithm interface
            return asm.Modules.Any(m => m.Types.Any(t => t.IsClass &&
                    t.Interfaces.Any(i => i.InterfaceType.FullName.Equals(interfaceType.FullName)))); 
        }

        private void ExecuteAlgorithm()
        {
            try
            {

                /*var type = assemblies[alg].GetType($"{alg}.{method}");
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
                } */
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

        private void AddPluginsToComboBox()
        {
            
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
