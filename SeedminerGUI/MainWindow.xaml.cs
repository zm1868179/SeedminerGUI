using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace SeedminerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window

        
    {

        //string output = string.Empty;
        private static StringBuilder output = new StringBuilder();
        private object syncGate = new object();
        private Process process;
        private bool outputChanged;


        public MainWindow()
        {
            InitializeComponent();
        }

        //If we click the button we copy the bin file to the work directory
        private void btn_SelMiiQR_Click(object sender, RoutedEventArgs e)
        {
            //Copy the encrypted.bin file to the working directory
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Input.bin (*.bin)|*.bin|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName;
                String exePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                //If the file exists delete the existing file and copy the newone.
                if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(exePath) + "\\App\\" + System.IO.Path.GetFileName(fileName)))
                {
                    System.IO.File.Delete(System.IO.Path.GetDirectoryName(exePath) + "\\App\\" + System.IO.Path.GetFileName(fileName));
                }
                    System.IO.File.Copy(fileName, System.IO.Path.GetDirectoryName(exePath) + "\\App\\" + System.IO.Path.GetFileName(fileName));
            }

        }

        //If the button was clicked use the input.bin file and attempt to brute force the movable_sedpart1.bin
        private void BTN_MIIBF_Click(object sender, RoutedEventArgs e)
        {
            //If the mfg has input year or no input use it
            if (TB_MFGYR.Text.Length == 0 || TB_MFGYR.Text.Length == 4)
            {
                string DStype = null;
                string MFGYR = null;
                //Grab the Year if it has value
                if (TB_MFGYR.Text.Length == 4)
                {
                     MFGYR = TB_MFGYR.Text;
                }
                else
                {
                    MFGYR = null;
                }

                if (RB_N3ds.IsChecked == true)
                {
                    DStype = "new";
                }

                else if (RB_O3DS.IsChecked == true)
                {
                    DStype = "old";
                }


                //Execute Command with Arguments
                String exePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
                string dir = System.IO.Path.GetDirectoryName(exePath)+"\\App\\";

                //Start the process and export thr console output to the textbox
                CreateProcess(dir + "seedminer_launcher.exe", "Mii " + DStype + " " + MFGYR, dir);


            }
            //Else display Error Message WIP
            else
            {
                tb_outputtext.Text = null;
                tb_outputtext.Text = "MFG Year must have 4 characters or none";
            }
        }

        //Execute a new process
        private void CreateProcess(string fileName, string arguments, string workdir)
        {
            // Process process = new Process();
            process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = workdir;
            process.OutputDataReceived += proc_OutputDataReceived;


            process.Start();
            process.BeginOutputReadLine();

        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                tb_outputtext.Text = tb_outputtext.Text + "\n" + e.Data;
                tb_outputtext.ScrollToEnd();
            }));

        }


        private void ReadData()
        {
            var input = process.StandardOutput;
            int nextChar;
            while ((nextChar = input.Read()) >= 0)
            {
                lock (syncGate)
                {
                    output.Append((char)nextChar);
                    if (!outputChanged)
                    {
                        outputChanged = true;
                        var dispatcher = Application.Current.MainWindow.Dispatcher;
                        Dispatcher.BeginInvoke(new Action(OnOutputChanged));
                    }
                }
            }
            lock (syncGate)
            {
                process.Dispose();
                process = null;
            }
        }

        private void OnOutputChanged()
        {
            lock (syncGate)
            {
                tb_outputtext.AppendText(output.ToString());
                outputChanged = false;
            }
        }

        //Add the ID0 to the moveable_sedpart1.bin
        private void BTN_ADDID0_Click(object sender, RoutedEventArgs e)
        {
            String exePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string dir = System.IO.Path.GetDirectoryName(exePath) + "\\App\\";

            if (TB_ID0.Text == null)
            {
                tb_outputtext.Clear();
                tb_outputtext.Text = "Please Enter your ID0 and make sure it is correct";
                return;

            }

            //if the movable_part1.sed exists then continue
            else if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(exePath) + "\\App\\" + "movable_part1.sed" ))
            {
                    //Execute Command with Arguments
                    //Start the process and export the console output to the textbox
                    CreateProcess(dir + "seedminer_launcher.exe", "id0 " + TB_ID0.Text, dir);
                return;
            }
            
                else
            {
                tb_outputtext.Clear();
                tb_outputtext.Text = "You must obtain your Movable_part1.sed";
                tb_outputtext.Text = Environment.NewLine + "place it in " + dir.ToString();
                tb_outputtext.Text = Environment.NewLine + "Or use the Mii Method to create it";
                return;
            }


           
        }

        //Brute Force the Moveable Sed with GPU
        private void btn_BruteForce_Click(object sender, RoutedEventArgs e)
        {
            String exePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string dir = System.IO.Path.GetDirectoryName(exePath) + "\\App\\";

            if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(exePath) + "\\App\\" + "movable_part1.sed"))
            {

                CreateProcess(dir + "seedminer_launcher.exe", "gpu ", dir);
            }

            else
            {
                tb_outputtext.Clear();
                tb_outputtext.Text = "You must obtain your Movable_part1.sed adn add your ID0 to it";
                tb_outputtext.Text = Environment.NewLine + "place it in " + dir.ToString();
                tb_outputtext.Text = Environment.NewLine + "Or use the Mii Method to create it";
                return;
            }
        }

        private void btn_BruteForceCPU_Click(object sender, RoutedEventArgs e)
        {
            String exePath = System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName;
            string dir = System.IO.Path.GetDirectoryName(exePath) + "\\App\\";

            if (System.IO.File.Exists(System.IO.Path.GetDirectoryName(exePath) + "\\App\\" + "movable_part1.sed"))
            {

                CreateProcess(dir + "seedminer_launcher.exe", "cpu ", dir);
            }

            else
            {
                tb_outputtext.Clear();
                tb_outputtext.Text = "You must obtain your Movable_part1.sed adn add your ID0 to it";
                tb_outputtext.Text = Environment.NewLine + "place it in " + dir.ToString();
                tb_outputtext.Text = Environment.NewLine + "Or use the Mii Method to create it";
                return;
            }
        }
    }
}
