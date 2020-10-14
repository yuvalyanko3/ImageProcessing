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
using System.Windows.Controls;
using System.ComponentModel;

namespace ImagAlg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserImage userImage;
        public List<string> pluginNames  { get; set; }
    private Dictionary<string, Type> algorithms = new Dictionary<string, Type>(); 

        private BackgroundWorker worker;

        public MainWindow()
        {

            InitializeComponent();
            LoadAlgorithms();
            DataContext = this;
            pluginCombo.SelectionChanged += ExecuteAlgorithm;
        }

        private void InitWorker()
        {
            if (worker == null)
            {
                worker = new BackgroundWorker();
            }
            worker.DoWork += Worker_ExecuteAlg;
            worker.RunWorkerCompleted += Worker_ExecuteCompleted;
        }

        private void LoadImage_Clicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*.jpg;*.jpeg;*.jpe;*.jfif;*.png"
            };

            if (fileDialog.ShowDialog() ?? true)
            {
                image.Source = new BitmapImage(new Uri(fileDialog.FileName));
                userImage = new UserImage(fileDialog.FileName);
            }
        }

        /// <summary>
        /// Load algorithms from Plugins folder
        /// </summary>
        private void LoadAlgorithms()
        {

            string pluginsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Plugins\");
            string[] files = Directory.GetFiles(pluginsPath, "*.dll");
            pluginNames = new List<string>();
            foreach (string file in files)
            {
                //Read assembly info without loading it into memory.
                var asmDef = Mono.Cecil.AssemblyDefinition.ReadAssembly(file);
                if(IsAssemblyImplementsInterface(asmDef))
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    Type type = assembly.GetTypes().First();
                    string name = SeperateWords(assembly.GetName().Name);
                    algorithms.Add(name, type);
                    pluginNames.Add(name);
                }
                asmDef.Dispose();
            }
        }

        private bool IsAssemblyImplementsInterface(Mono.Cecil.AssemblyDefinition asm)
        {
            Type interfaceType = typeof(IImageProcessingAlgorithm);
            // check if assembly contains a class that implements the IImageProcessingAlgorithm interface
            return asm.Modules.Any(m => m.Types.Any(t => t.IsClass &&
                    t.Interfaces.Any(i => i.InterfaceType.FullName.Equals(interfaceType.FullName)))); 
        }

        private void ExecuteAlgorithm(object sender, SelectionChangedEventArgs e)
        {
            if(userImage != null)
            {
                int index = pluginCombo.SelectedIndex;
                if(index > -1)
                {
                    loadBtn.IsEnabled = false;
                    if (userImage.ProcessedImage != null)
                    {
                        image.Source = ConvertBitmapToImageSource(userImage.Bitmap);
                    }
                    if (worker == null)
                    {
                        InitWorker();
                    }
                    worker.RunWorkerAsync(pluginCombo.SelectedItem.ToString());
                }
            }
            else
            {
                RefreshComponents();
            }

        }

        private void Worker_ExecuteAlg(object sender, DoWorkEventArgs e)
        {
            try
            {
                string alg = (string)e.Argument;
                Type type = algorithms[alg];
                MethodInfo methodInfo = type.GetMethod(Consts.RunAlgMethod);
                var instance = Activator.CreateInstance(type);
                Bitmap result = (Bitmap)methodInfo.Invoke(instance, new object[] { userImage.Bitmap.Clone() });
                if (result != null)
                {
                    userImage.ProcessedImage = result;
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
            catch (Exception ex)
            {
                if(ex is AmbiguousMatchException || ex is ArgumentNullException)
                {
                    MessageBox.Show("Could not find algorithm");
                }
            }
        }

        private void Worker_ExecuteCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(userImage.ProcessedImage != null)
            {
                image.Source = ConvertBitmapToImageSource(userImage.ProcessedImage);
            }
            loadBtn.IsEnabled = true;
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

        private void RefreshComponents()
        {
            loadBtn.IsEnabled = true;
            pluginCombo.SelectedIndex = -1;
        }

        /// <summary>
        /// Helper method. Adds a space before every capital letter (except the first one)
        /// </summary>
        private string SeperateWords(string str)
        {
            return string.Concat(str.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }
    }
}
