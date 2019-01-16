using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace PAObfuscator
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly Config config = Config.Instance;
        private readonly MVVM viewModel = new MVVM();
        private Obfuscator obfuscator = null;
        private Crypt crypt = null;
        private readonly string _appVersion;
        public MainWindow()
        {
            var thisVersion = Assembly.GetExecutingAssembly().Location;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(thisVersion);
            _appVersion = fvi.FileVersion;
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!config.Init())
            {
               
            }

            viewModel.AppVersion = _appVersion;
            viewModel.ExportFolder = config.ExportFolder;
            viewModel.MissionFolder = config.MissionFolder;
            viewModel.MakePboFolder = config.MakePboFolder;

            obfuscator = new Obfuscator(viewModel.MissionFolder);
            obfuscator.OnSendDebug += Obfuscator_OnSendDebug;
            
            rtbDebug.AppendText("[System]: Anwendung gestartet. Version: "+_appVersion);

        }

        private void Obfuscator_OnSendDebug(string caller, string message, string color = "0x000000")
        {
            rtbDebug.AppendText(DateTime.Now.ToString("hh:mm:ss") + " - [" + caller + "]: " + message + Environment.NewLine); rtbDebug.ScrollToEnd();
        }

        private void FolderSelect(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            var dlg = new System.Windows.Forms.FolderBrowserDialog();
            if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (btn.Name.ToLower())
                {
                    case "btnsetmission":
                        {
                            viewModel.MissionFolder = dlg.SelectedPath;
                        }break;
                    case "btnsetexport":
                        {
                            viewModel.ExportFolder = dlg.SelectedPath;
                        }break;
                    case "btnsetmakepbo":
                        {
                            viewModel.MakePboFolder = dlg.SelectedPath;
                        }break;
                }
            }
        }

        #region RTB Extension
        #endregion

        private void btnObfuscate_Click(object sender, RoutedEventArgs e)
        {
            obfuscator.Obfuscate(Directory.GetFiles(viewModel.MissionFolder, "*", SearchOption.AllDirectories));
            rtbDebug.AppendText("[System]: Obfuscator abgeschlossen");

            string cryptWorkPath = System.IO.Path.Combine(config.AppPath, "AntiHack");
            crypt = new Crypt(cryptWorkPath);
            crypt.OnSendDebug += Obfuscator_OnSendDebug;
            StreamReader cfgReader = new StreamReader(viewModel.MissionFolder + "\\description.ext");
            StreamWriter cfgWriter = new StreamWriter(obfuscator.obfuPath + "\\config\\cfgpa_crypt.hpp");
            cfgWriter.WriteLine("class ParadiseCryptSys {");
            string line;
            string tag = "";
            string fileP = "";
            string crntClass = "";
            while ((line = cfgReader.ReadLine()) != null)
            {
                bool containsClass = Regex.Match(line.ToLower(), "(\\s)?class").Success;
                bool containsTag = Regex.Match(line.ToLower(), "tag\\s?=\\s?\\\"").Success;
                bool containsPath = Regex.Match(line.ToLower(), "file\\s?=\\s?\\\"").Success;
                bool containsEnd = Regex.Match(line.ToLower(), ".*};-*").Success;

                if (tag != "" && fileP != "" && containsClass)
                {
                    var regex = new Regex("\\s.*class\\s?|\\s?{};\\s?", RegexOptions.Multiline);
                    crntClass = regex.Replace(line, "");
                    if (crntClass.ToLower().Contains("preinit"))
                    {
                        continue;
                    }

                    try
                    {
                        rtbDebug.AppendText("[System]: " + obfuscator.obfuPath + "\\" + fileP + "\\fn_" + crntClass + ".sqf" + " wird versucht umzuwandeln.");
                        cfgWriter.WriteLine("\tclass " + crntClass + " {");
                        cfgWriter.WriteLine("\t\ttag = \"" + tag + "\";\n\t\tcrypted = \"" + fileP + "\\" + crypt.cryptSqf(obfuscator.obfuPath + "\\" + fileP + "\\fn_" + crntClass + ".sqf") + "\";");
                        cfgWriter.WriteLine("\t};");
                        rtbDebug.AppendText("[System]: " + obfuscator.obfuPath + "\\" + fileP + "\\fn_" + crntClass + ".sqf" + " wurde umgewandelt.");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        rtbDebug.AppendText("[System]: " + ex.Message);
                        return;
                    }
                }

                if (containsTag)
                {
                    var regex = new Regex(".*tag\\s?=\\s\"|\\\";\\s?", RegexOptions.Multiline);
                    tag = regex.Replace(line, "");
                    continue;
                }

                if (containsPath)
                {
                    var regex = new Regex(".*file\\s?=\\s\"|\\\";\\s?", RegexOptions.Multiline);
                    fileP = regex.Replace(line, "");
                    continue;
                }

                if (containsEnd)
                {
                    if (fileP != "")
                    {
                        fileP = "";
                        continue;
                    }
                    tag = "";
                }

                cfgWriter.WriteLine("};");
                cfgWriter.Close();
                cfgReader.Close();

                StreamWriter fncWriter = new StreamWriter(obfuscator.obfuPath + "description.ext");
                fncWriter.WriteLine("#include \"config\\cfgpa_crypt.hpp\"");
                fncWriter.WriteLine("class CfgFunctions {\n\tclass Life_Client_Core {\n\t\ttag = \"life\";\n\t\tclass Functions {\n\t\t\tfile = \"core\\functions\";\n\t\t\tclass deCrypt {preInit = 1;};\n\t\t};\n\t};\n};");
                fncWriter.Close();

            }
        }
    }

    //public static class Extension
    //{
    //    public static void AppendText(this RichTextBox box, string text, string color)
    //    {
    //        BrushConverter bc = new BrushConverter();
    //        TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
    //        tr.Text = text;
    //        try
    //        {
    //            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
    //                bc.ConvertFromString(color));
    //        }
    //        catch (FormatException) { }
    //    }
    //}
}
