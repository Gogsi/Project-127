﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace Project_127_Launcher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			string ProjectInstallationPath = Process.GetCurrentProcess().MainModule.FileName.Substring(0, Process.GetCurrentProcess().MainModule.FileName.LastIndexOf('\\'));

			string filePath = ProjectInstallationPath.TrimEnd('\\') + @"\UglyFiles\Project 1.27.exe";

			string[] args = Environment.GetCommandLineArgs();

			string arg = string.Join(" ", args.Skip(1).ToArray());

			Process p = new Process();
			p.StartInfo.FileName = filePath;
			p.StartInfo.Arguments = arg;
			p.StartInfo.UseShellExecute = true;
			p.StartInfo.Verb = "runas";
			p.Start();

			this.Close();
		}
	}
}
