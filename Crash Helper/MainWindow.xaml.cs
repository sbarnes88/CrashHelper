using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Crash_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Process Process { get; set; }

        private bool _isRunning;
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; NotifyPropertyChanged(); NotifyPropertyChanged("IsNotRunning"); }
        }

        public bool IsNotRunning
        {
            get { return !IsRunning; }
            set { NotifyPropertyChanged();  }
        }

        private string _executable;
        public string Executable
        {
            get { return _executable; }
            set { _executable = value; NotifyPropertyChanged(); }
        }

        private string _arguments;
        public string Arguments
        {
            get { return _arguments; }
            set { _arguments = value; NotifyPropertyChanged(); }
        }

        private string _workingPath;
        public string WorkingPath
        {
            get { return _workingPath; }
            set { _workingPath = value; NotifyPropertyChanged(); }
        }

        private string _output;
        public string Output
        {
            get { return _output; }
            set { _output = value; NotifyPropertyChanged(); }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
            IsRunning = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Exe (*.exe)|*.exe";
            var result = openFileDialog.ShowDialog();
            if (!result.HasValue || result == false)
                return;
            Executable = openFileDialog.FileName;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Process == null || !IsRunning)
                return;
            Process.Kill();
            IsRunning = false;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
                return;
            Output = "";
            Output = $"Starting: {Executable}\r\nArguments: {Arguments}\r\nWorking Directory: {WorkingPath}";

            Process = new Process();
            Process.StartInfo = new ProcessStartInfo();
            Process.StartInfo.FileName = Executable;
            Process.StartInfo.WorkingDirectory = WorkingPath;
            Process.StartInfo.Arguments = Arguments;
            Process.OutputDataReceived += Process_OutputDataReceived;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.UseShellExecute = false;
            Process.ErrorDataReceived += Process_ErrorDataReceived;
            Process.Exited += Process_Exited;
            Process.StartInfo.RedirectStandardError = true;
            Process.EnableRaisingEvents = true;
            Process.Start();
            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();
            IsRunning = true;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Output += $"{Environment.NewLine}Program has stopped execution.";
            IsRunning = false;
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Output += $"{Environment.NewLine}{e.Data}";
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Output += $"{Environment.NewLine}{e.Data}";
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Output += e.Exception.Message;
        }

        private string GetFullException(Exception ex)
        {
            if (ex == null)
                return string.Empty;
            return $"{ex.Message}{Environment.NewLine}{GetFullException(ex.InnerException)}";
        }
    }
}
