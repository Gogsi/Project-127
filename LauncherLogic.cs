﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using GSF;

namespace Project_127
{
	/// <summary>
	/// Class for the launching
	/// </summary>
	public static class LauncherLogic
	{
		/// <summary>
		/// Enum we use to change the Auth Button Image (lock)
		/// </summary>
		public enum AuthStates
		{
			NotAuth = 0,
			Auth = 1
		}

		/// <summary>
		/// Enum for InstallationStates
		/// </summary>
		public enum InstallationStates
		{
			Upgraded,
			Downgraded,
			Unsure
		}

		/// <summary>
		/// Enum for GameStates
		/// </summary>
		public enum GameStates
		{
			Running,
			NonRunning
		}

		/// <summary>
		/// AuthState Property
		/// </summary>
		public static AuthStates AuthState
		{
			get
			{
				if (ROSCommunicationBackend.SessionValid)
				{
					return AuthStates.Auth;
				}
				else
				{
					return AuthStates.NotAuth;
				}
			}
		}

		/// <summary>
		/// Property of our GameState
		/// </summary>
		public static GameStates GameState
		{
			get
			{
				if (HelperClasses.ProcessHandler.IsGtaRunning())
				{
					return GameStates.Running;
				}
				else
				{
					return GameStates.NonRunning;
				}
			}
		}

		/// <summary>
		/// Property of what InstallationState we are in. I want to access this from here
		/// </summary>
		public static InstallationStates InstallationState
		{
			get
			{
				long SizeOfGTAV = HelperClasses.FileHandling.GetSizeOfFile(GTAVFilePath.TrimEnd('\\') + @"\GTA5.exe");
				long SizeOfUpdate = HelperClasses.FileHandling.GetSizeOfFile(GTAVFilePath.TrimEnd('\\') + @"\update\update.rpf");

				// if Sizes in GTA V Installation Path match what files we use from ZIP for downgrading
				if (SizeOfGTAV == SizeOfDowngradedGTAV && SizeOfUpdate == SizeOfDowngradedUPDATE)
				{
					return InstallationStates.Downgraded;
				}
				else
				{
					// if both Files in the GTA V Install Path exist
					if (SizeOfGTAV > 0 && SizeOfUpdate > 0)
					{
						// If both are NOT downgrad
						if (SizeOfGTAV != SizeOfDowngradedGTAV && SizeOfUpdate != SizeOfDowngradedUPDATE)
						{
							return InstallationStates.Upgraded;
						}
						else
						{
							return InstallationStates.Unsure;
						}
					}
					return InstallationStates.Unsure;
				}
			}
		}

		/// <summary>
		/// Just a reference to the GameVersion we are running. GameVersion as in Retailer
		/// </summary>
		public static Settings.Retailers GameVersion { get { return Settings.Retailer; } }

		/// <summary>
		/// Size of Downgraded GTAV.exe. So I can detect the InstallationState (Upgrade / Downgrade)
		/// </summary>
		public static long SizeOfDowngradedGTAV { get { return HelperClasses.FileHandling.GetSizeOfFile(DowngradeFilePath.TrimEnd('\\') + @"\GTA5.exe"); } }

		/// <summary>
		/// Size of Downgraded upgrade\upgrade.rpf . So I can detect the InstallationState (Upgrade / Downgrade)
		/// </summary>
		public static long SizeOfDowngradedUPDATE { get { return HelperClasses.FileHandling.GetSizeOfFile(DowngradeFilePath.TrimEnd('\\') + @"\update\update.rpf"); } }

		/// <summary>
		/// Path of where the ZIP File is extracted
		/// </summary>
		public static string ZIPFilePath { get { return Settings.ZIPExtractionPath.TrimEnd('\\') + @"\"; } }

		/// <summary>
		/// Property of often used variable. (UpgradeFilePath)
		/// </summary>
		public static string UpgradeFilePath { get { return LauncherLogic.ZIPFilePath.TrimEnd('\\') + @"\Project_127_Files\UpgradeFiles\"; } }

		/// <summary>
		/// Property of often used variable. (DowngradeFilePath)
		/// </summary>
		public static string DowngradeFilePath { get { return LauncherLogic.ZIPFilePath.TrimEnd('\\') + @"\Project_127_Files\DowngradeFiles\"; } }

		/// <summary>
		/// Property of often used variable. (SupportFilePath)
		/// </summary>
		public static string SupportFilePath { get { return LauncherLogic.ZIPFilePath.TrimEnd('\\') + @"\Project_127_Files\SupportFiles\"; } }

		/// <summary>
		/// Property of often used variable. (SupportFilePath)
		/// </summary>
		public static string SaveFilesPath { get { return LauncherLogic.SupportFilePath.TrimEnd('\\') + @"\SaveFiles\"; } }

		/// <summary>
		/// Property of often used variable. (GTAVFilePath)
		/// </summary>
		public static string GTAVFilePath { get { return Settings.GTAVInstallationPath.TrimEnd('\\') + @"\"; } }

		/// <summary>
		/// Method for Upgrading the Game back to latest Version
		/// </summary>
		public static void Upgrade()
		{
			// Saving all the File Operations I want to do, executing this at the end of this Method
			List<MyFileOperation> MyFileOperations = new List<MyFileOperation>();

			KillRelevantProcesses();

			// Creates Hardlink Link in GTAV Installation Folder to all the files of Upgrade Folder
			// If they exist in GTAV Installation Folder,  we delete them from GTAV Installation folder

			HelperClasses.Logger.Log("Initiating Upgrade", 0);
			HelperClasses.Logger.Log("GTAV Installation Path: " + GTAVFilePath, 1);
			HelperClasses.Logger.Log("InstallationLocation: " + Globals.ProjectInstallationPath, 1);
			HelperClasses.Logger.Log("ZIP File Location: " + LauncherLogic.ZIPFilePath, 1);
			HelperClasses.Logger.Log("DowngradeFilePath: " + DowngradeFilePath, 1);
			HelperClasses.Logger.Log("UpgradeFilePath: " + UpgradeFilePath, 1);

			// Those are WITH the "\" at the end
			string[] FilesInUpgradesFiles = HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(UpgradeFilePath);
			string[] CorrespondingFilePathInGTALocation = new string[FilesInUpgradesFiles.Length];
			string[] CorrespondingFilePathInDowngradeFiles = new string[FilesInUpgradesFiles.Length];

			HelperClasses.Logger.Log("Found " + FilesInUpgradesFiles.Length.ToString() + " Files in Upgrade Folder.");

			// Loop through all Files in Upgrade Files Folder
			for (int i = 0; i <= FilesInUpgradesFiles.Length - 1; i++)
			{
				// Build the Corresponding theoretical Filenames for Upgrade Folder and GTA V Installation Folder
				CorrespondingFilePathInGTALocation[i] = GTAVFilePath + FilesInUpgradesFiles[i].Substring(UpgradeFilePath.Length);
				CorrespondingFilePathInDowngradeFiles[i] = UpgradeFilePath + FilesInUpgradesFiles[i].Substring(DowngradeFilePath.Length);

				// If the File exists in GTA V Installation Path
				if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInGTALocation[i]))
				{
					// Delete from GTA V Installation Path
					MyFileOperations.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "File found in GTA V Installation Path and the Upgrade Folder. Will delete '" + CorrespondingFilePathInGTALocation[i] + "'", 1));
				}

				// Creates actual Hard Link (this will further down check if we should copy based on settings in MyFileOperation.Execute())
				MyFileOperations.Add(new MyFileOperation(MyFileOperation.FileOperations.Hardlink, FilesInUpgradesFiles[i], CorrespondingFilePathInGTALocation[i], "Will create HardLink in '" + CorrespondingFilePathInGTALocation[i] + "' to the file in '" + FilesInUpgradesFiles[i] + "'", 1));
			}

			// Actually executing the File Operations
			new PopupProgress(PopupProgress.ProgressTypes.FileOperation, "Upgrade", MyFileOperations).ShowDialog();

			// We dont need to mess with social club versions since the launch process doesnt depend on it

			HelperClasses.Logger.Log("Done Upgrading");
		}

		/// <summary>
		/// Method for "Repairing" our setup
		/// </summary>
		public static void Repair()
		{
			// Saving all the File Operations I want to do, executing this at the end of this Method
			List<MyFileOperation> MyFileOperations = new List<MyFileOperation>();

			HelperClasses.Logger.Log("Initiating Repair.", 0);
			HelperClasses.Logger.Log("GTAV Installation Path: " + GTAVFilePath, 1);
			HelperClasses.Logger.Log("InstallationLocation: " + Globals.ProjectInstallationPath, 1);
			HelperClasses.Logger.Log("ZIP File Location: " + LauncherLogic.ZIPFilePath, 1);
			HelperClasses.Logger.Log("DowngradeFilePath: " + DowngradeFilePath, 1);
			HelperClasses.Logger.Log("UpgradeFilePath: " + UpgradeFilePath, 1);

			KillRelevantProcesses();

			string[] FilesInUpgradeFiles = Directory.GetFiles(UpgradeFilePath, "*", SearchOption.AllDirectories);
			HelperClasses.Logger.Log("Found " + FilesInUpgradeFiles.Length.ToString() + " Files in Upgrade Folder. Will try to delete them", 1);
			foreach (string myFileName in FilesInUpgradeFiles)
			{
				MyFileOperations.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, myFileName, "", "Deleting '" + (myFileName) + "' from the $UpgradeFolder", 2));
			}

			// Actually executing the File Operations
			new PopupProgress(PopupProgress.ProgressTypes.FileOperation, "Repair", MyFileOperations).ShowDialog();

			// We dont need to mess with social club versions since the launch process doesnt depend on it

			HelperClasses.Logger.Log("Repair is done. Files in Upgrade Folder deleted.");
		}

		/// <summary>
		/// Method for Downgrading
		/// </summary>
		public static void Downgrade()
		{
			// Saving all the File Operations I want to do, executing this at the end of this Method
			List<MyFileOperation> MyFileOperations = new List<MyFileOperation>();

			KillRelevantProcesses();

			// Creates Hardlink Link in GTAV Installation Folder to all the files of Downgrade Folder
			// If they exist in GTAV Installation Folder, and in Upgrade Folder, we delete them from GTAV Installation folder
			// If they exist in GTAV Installation Folder, and NOT in Upgrade Folder, we move them there

			HelperClasses.Logger.Log("Initiating Downgrade", 0);
			HelperClasses.Logger.Log("GTAV Installation Path: " + GTAVFilePath, 1);
			HelperClasses.Logger.Log("InstallationLocation: " + Globals.ProjectInstallationPath, 1);
			HelperClasses.Logger.Log("ZIP File Location: " + LauncherLogic.ZIPFilePath, 1);
			HelperClasses.Logger.Log("DowngradeFilePath: " + DowngradeFilePath, 1);
			HelperClasses.Logger.Log("UpgradeFilePath: " + UpgradeFilePath, 1);

			// Those are WITH the "\" at the end
			string[] FilesInDowngradeFiles = HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(DowngradeFilePath);
			string[] CorrespondingFilePathInGTALocation = new string[FilesInDowngradeFiles.Length];
			string[] CorrespondingFilePathInUpgradeFiles = new string[FilesInDowngradeFiles.Length];

			HelperClasses.Logger.Log("Found " + FilesInDowngradeFiles.Length.ToString() + " Files in Downgrade Folder.");

			// Loop through all Files in Downgrade Files Folder
			for (int i = 0; i <= FilesInDowngradeFiles.Length - 1; i++)
			{
				// Build the Corresponding theoretical Filenames for Upgrade Folder and GTA V Installation Folder
				CorrespondingFilePathInGTALocation[i] = GTAVFilePath + FilesInDowngradeFiles[i].Substring(DowngradeFilePath.Length);
				CorrespondingFilePathInUpgradeFiles[i] = UpgradeFilePath + FilesInDowngradeFiles[i].Substring(DowngradeFilePath.Length);

				// If the File exists in GTA V Installation Path
				if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInGTALocation[i]))
				{
					// If the File Exists in Upgrade Folder
					if (HelperClasses.FileHandling.doesFileExist(CorrespondingFilePathInUpgradeFiles[i]))
					{
						// Delete from GTA V Installation Path
						MyFileOperations.Add(new MyFileOperation(MyFileOperation.FileOperations.Delete, CorrespondingFilePathInGTALocation[i], "", "Found '" + CorrespondingFilePathInGTALocation[i] + "' in GTA V Installation Path and $UpgradeFiles. Will delelte from GTA V Installation", 1));
					}
					else
					{
						// If its not the same file as in DownGradeFiles
						if (HelperClasses.FileHandling.GetHashFromFile(CorrespondingFilePathInGTALocation[i]) !=
							HelperClasses.FileHandling.GetHashFromFile(FilesInDowngradeFiles[i]))
						{
							// Move File from GTA V Installation Path to Upgrade Folder
							MyFileOperations.Add(new MyFileOperation(MyFileOperation.FileOperations.Move, CorrespondingFilePathInGTALocation[i], CorrespondingFilePathInUpgradeFiles[i], "Found '" + CorrespondingFilePathInGTALocation[i] + "' in GTA V Installation Path and NOT in $UpgradeFiles. Will move it from GTA V Installation to $UpgradeFiles", 1));
						}
					}
				}

				// Creates actual Hard Link (this will further down check if we should copy based on settings in MyFileOperation.Execute())
				MyFileOperations.Add(new MyFileOperation(MyFileOperation.FileOperations.Hardlink, FilesInDowngradeFiles[i], CorrespondingFilePathInGTALocation[i], "Will create HardLink in '" + CorrespondingFilePathInGTALocation[i] + "' to the file in '" + FilesInDowngradeFiles[i] + "'", 1));
			}

			// Actually executing the File Operations
			new PopupProgress(PopupProgress.ProgressTypes.FileOperation, "Downgrade", MyFileOperations).ShowDialog();

			// We dont need to mess with social club versions since the launch process doesnt depend on it

			HelperClasses.Logger.Log("Done Downgrading");
		}



		/// <summary>
		/// This actually launches the game
		/// </summary>
		public static async void Launch()
		{
			HelperClasses.Logger.Log("Trying to Launch the game.");

			// If Upgraded
			if (LauncherLogic.InstallationState == InstallationStates.Upgraded)
			{
				HelperClasses.Logger.Log("Installation State Upgraded Detected.", 1);
			}
			else if (LauncherLogic.InstallationState == InstallationStates.Downgraded)
			{
				HelperClasses.Logger.Log("Installation State Downgraded Detected.", 1);

				// If already Authed
				if (AuthState == AuthStates.Auth)
				{
					HelperClasses.Logger.Log("You are already Authenticated. Will Launch Game Now");
				}

				// If not Authed
				else
				{
					HelperClasses.Logger.Log("You are NOT already Authenticated. Throwing up Window now.");

					// Trying to Auth User
					Globals.PageState = Globals.PageStates.Auth;

					// If still not authed
					if (AuthState == AuthStates.NotAuth)
					{
						// Throw Error and Quick
						HelperClasses.Logger.Log("Manual User Auth on Launch click did not work. Launch method will exit");
						new Popup(Popup.PopupWindowTypes.PopupOk, "Authentication not sucessfull. Will abort Launch Function. Please Try again");
						return;
					}
					else
					{
						HelperClasses.Logger.Log("Auth inside of Launch Click worked");
					}
				}


				// Generates Token needed to Launch Downgraded GTAV
				HelperClasses.Logger.Log("Letting Dragon work his magic");
				await ROSCommunicationBackend.GenToken();
			}
			else
			{
				HelperClasses.Logger.Log("Installation State Broken");
				HelperClasses.Logger.Log("    Size of GTA5.exe in GTAV Installation Path: " + HelperClasses.FileHandling.GetSizeOfFile(LauncherLogic.GTAVFilePath.TrimEnd('\\') + @"\GTA5.exe"));
				HelperClasses.Logger.Log("    Size of GTA5.exe in Downgrade Files Folder: " + LauncherLogic.SizeOfDowngradedGTAV);
				HelperClasses.Logger.Log("    Size of update.rpf in GTAV Installation Path: " + HelperClasses.FileHandling.GetSizeOfFile(LauncherLogic.GTAVFilePath.TrimEnd('\\') + @"\update\update.rpf"));
				HelperClasses.Logger.Log("    Size of update.rpf in Downgrade Files Folder: " + LauncherLogic.SizeOfDowngradedUPDATE);

				new Popup(Popup.PopupWindowTypes.PopupOkError, "Installation State is broken for some reason. Try to repair.");
				return;
			}


			// If Steam
			if (GameVersion == Settings.Retailers.Steam)
			{
				HelperClasses.Logger.Log("Trying to start Game normally through Steam.", 1);

				// If we dont want to launch through Steam
				if (Settings.EnableDontLaunchThroughSteam)
				{
					// Launch through non retail
					HelperClasses.ProcessHandler.StartGameNonRetail();
				}
				else
				{
					// Launch through steam
					HelperClasses.ProcessHandler.StartProcess(Globals.SteamInstallPath.TrimEnd('\\') + @"\steam.exe", pCommandLineArguments: "-applaunch 271590 -uilanguage " + Settings.ToMyLanguageString(Settings.LanguageSelected).ToLower());
				}

			}

			// If Epic Games
			else if (GameVersion == Settings.Retailers.Epic)
			{
				// If upgraded, launch through epic
				if (InstallationState == InstallationStates.Upgraded)
				{
					HelperClasses.Logger.Log("Trying to start Game normally through EpicGames.", 1);

					// This does not work with custom wrapper StartProcess in ProcessHandler...i guess this is fine
					Process.Start(@"com.epicgames.launcher://apps/9d2d0eb64d5c44529cece33fe2a46482?action=launch&silent=true");
				}
				// If downgraded launch through non retail
				else
				{
					HelperClasses.ProcessHandler.StartGameNonRetail();
				}
			}

			// If Rockstar
			else
			{
				// Launch through Non Retail re
				HelperClasses.ProcessHandler.StartGameNonRetail();
			}

			HelperClasses.Logger.Log("Game should be launched");

			PostLaunchEvents();
		}


		/// <summary>
		/// Method which gets called after Starting GTAV
		/// </summary>
		public async static void PostLaunchEvents()
		{
			HelperClasses.Logger.Log("Post Launch Events started");
			await Task.Delay(2500);
			HelperClasses.Logger.Log("Waited a good bit");

			if (Settings.EnableAutoSetHighPriority)
			{
				HelperClasses.Logger.Log("Trying to Set GTAV Process Priority to High");
				Process[] processes = HelperClasses.ProcessHandler.GetProcesses("gta5");
				HelperClasses.Logger.Log(processes.Length + " Processes containing 'gta5' found");
				try
				{
					processes[0].PriorityClass = ProcessPriorityClass.High;
					HelperClasses.Logger.Log("I changed the priority of one process...");
					if (processes[0].PriorityClass == ProcessPriorityClass.High)
					{
						HelperClasses.Logger.Log("Did so sucessfully.");
					}
					else
					{
						HelperClasses.Logger.Log("Failed to Set priority. This sucks.");
					}
				}
				catch
				{
					HelperClasses.Logger.Log("Failed to get GTA5 Process...");
				}
			}

			// If we DONT only auto start when downgraded OR if we are downgraded
			if (Settings.EnableOnlyAutoStartProgramsWhenDowngraded == false || LauncherLogic.InstallationState == InstallationStates.Downgraded)
			{
				HelperClasses.Logger.Log("Either we are Downgraded or EnableOnlyAutoStartProgramsWhenDowngraded is set to false");
				if (Settings.EnableAutoStartFPSLimiter)
				{
					HelperClasses.Logger.Log("We are trying to auto Start FPS Limiter");
					string ProcessName = HelperClasses.FileHandling.PathSplitUp(Settings.PathFPSLimiter)[1];
					if (!HelperClasses.ProcessHandler.IsProcessRunning(ProcessName))
					{
						HelperClasses.Logger.Log("Process is not already running...", 1);
						if (HelperClasses.FileHandling.doesFileExist(Settings.PathFPSLimiter))
						{
							HelperClasses.Logger.Log("File does exist, lets start it...",1);
							HelperClasses.ProcessHandler.StartProcess(Settings.PathFPSLimiter);
						}
						else
						{
							HelperClasses.Logger.Log("Path (File) seems to not exist.", 1);
						}
					}
					else
					{
						HelperClasses.Logger.Log("Seems to be running already", 1);
					}
				}
				if (Settings.EnableAutoStartLiveSplit)
				{
					HelperClasses.Logger.Log("We are trying to auto Start LiveSplit");
					string ProcessName = HelperClasses.FileHandling.PathSplitUp(Settings.PathLiveSplit)[1];
					if (!HelperClasses.ProcessHandler.IsProcessRunning(ProcessName))
					{
						HelperClasses.Logger.Log("Process is not already running...", 1);
						if (HelperClasses.FileHandling.doesFileExist(Settings.PathLiveSplit))
						{
							HelperClasses.Logger.Log("File does exist, lets start it...", 1);
							HelperClasses.ProcessHandler.StartProcess(Settings.PathLiveSplit);
						}
						else
						{
							HelperClasses.Logger.Log("Path (File) seems to not exist.", 1);
						}
					}
					else
					{
						HelperClasses.Logger.Log("Seems to be running already", 1);
					}
				}
				if (Settings.EnableAutoStartStreamProgram)
				{
					HelperClasses.Logger.Log("We are trying to auto Start Stream Program");
					string ProcessName = HelperClasses.FileHandling.PathSplitUp(Settings.PathStreamProgram)[1];
					if (!HelperClasses.ProcessHandler.IsProcessRunning(ProcessName))
					{
						HelperClasses.Logger.Log("Process is not already running...", 1);
						if (HelperClasses.FileHandling.doesFileExist(Settings.PathStreamProgram))
						{
							HelperClasses.Logger.Log("File does exist, lets start it...", 1);
							HelperClasses.ProcessHandler.StartProcess(Settings.PathStreamProgram);
						}
						else
						{
							HelperClasses.Logger.Log("Path (File) seems to not exist.", 1);
						}
					}
					else
					{
						HelperClasses.Logger.Log("Seems to be running already", 1);
					}
				}
				if (Settings.EnableAutoStartNohboard)
				{
					HelperClasses.Logger.Log("We are trying to auto Start Nohboard");
					string ProcessName = HelperClasses.FileHandling.PathSplitUp(Settings.PathNohboard)[1];
					if (!HelperClasses.ProcessHandler.IsProcessRunning(ProcessName))
					{
						HelperClasses.Logger.Log("Process is not already running...", 1);
						if (HelperClasses.FileHandling.doesFileExist(Settings.PathNohboard))
						{
							HelperClasses.Logger.Log("File does exist, lets start it...", 1);
							HelperClasses.ProcessHandler.StartProcess(Settings.PathNohboard);
						}
						else
						{
							HelperClasses.Logger.Log("Path (File) seems to not exist.", 1);
						}
					}
					else
					{
						HelperClasses.Logger.Log("Seems to be running already", 1);
					}
				}
			}


		}



		/// <summary>
		/// Method to import Zip File
		/// </summary>
		public static void ImportZip(string pZipFileLocation, bool deleteFileAfter = false)
		{
			// Creating all needed Folders
			HelperClasses.FileHandling.CreateAllZIPPaths(Settings.ZIPExtractionPath);

			// Getting some Info of the current Installation
			LauncherLogic.InstallationStates OldInstallationState = LauncherLogic.InstallationState;
			string OldHash = HelperClasses.FileHandling.CreateDirectoryMd5(LauncherLogic.DowngradeFilePath);

			HelperClasses.Logger.Log("Importing ZIP File: '" + pZipFileLocation + "'");
			HelperClasses.Logger.Log("Old ZIP File Version: '" + Globals.ZipVersion + "'");
			HelperClasses.Logger.Log("Old Installation State: '" + OldInstallationState + "'");
			HelperClasses.Logger.Log("Old Hash of Downgrade Folder: '" + OldHash + "'");
			HelperClasses.Logger.Log("Settings.ZIPPath: '" + Settings.ZIPExtractionPath + "'");


			string[] myFiles = HelperClasses.FileHandling.GetFilesFromFolderAndSubFolder(LauncherLogic.ZIPFilePath.TrimEnd('\\') + @"\Project_127_Files");



			// Dont need this for now, lets keep it in case its needed again
			//foreach (string myFile in myFiles)
			//{
			//	if (!myFile.Contains("UpgradeFiles"))
			//	{
			//		// Deleting all Files which are NOT in the UpgradeFiles Folder of the ZIP Extract Path
			//		HelperClasses.FileHandling.deleteFile(myFile);
			//	}
			//}

			// Actually Extracting the ZIP File
			HelperClasses.Logger.Log("Extracting ZIP File: '" + pZipFileLocation + "' to the path: '" + LauncherLogic.ZIPFilePath + "'");
			new PopupProgress(PopupProgress.ProgressTypes.ZIPFile, pZipFileLocation).ShowDialog();


			// Deleting the ZIP File
			if (deleteFileAfter)
			{
				HelperClasses.Logger.Log("Deleting ZIP File: '" + pZipFileLocation + "'");
				HelperClasses.FileHandling.deleteFile(pZipFileLocation);
			}

			LauncherLogic.InstallationStates NewInstallationState = LauncherLogic.InstallationState;
			string NewHash = HelperClasses.FileHandling.CreateDirectoryMd5(LauncherLogic.DowngradeFilePath);

			HelperClasses.Logger.Log("Done Importing ZIP File: '" + pZipFileLocation + "'");
			HelperClasses.Logger.Log("New ZIP File Version: '" + Globals.ZipVersion + "'");
			HelperClasses.Logger.Log("New Installation State: '" + NewInstallationState + "'");
			HelperClasses.Logger.Log("New Hash of Downgrade Folder: '" + NewHash + "'");


			// If the state was Downgraded before Importing ZIP-File
			if (OldInstallationState == LauncherLogic.InstallationStates.Downgraded)
			{
				// If old and new Hash (of downgrade folder) dont match
				if (OldHash != NewHash)
				{
					// Downgrade again
					LauncherLogic.Downgrade();
				}
			}

			new Popup(Popup.PopupWindowTypes.PopupOk, "ZIP File imported (Version: '" + Globals.ZipVersion + "')").ShowDialog();
		}



		/// <summary>
		/// "Cleanest" way of getting the GTA V Path automatically
		/// </summary>
		/// <returns></returns>
		public static string GetGTAVPathMagicEpic()
		{
			HelperClasses.Logger.Log("GTAV Path Magic by epic", 2);

			string[] MyFiles = HelperClasses.FileHandling.GetFilesFromFolder(@"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests");

			foreach (string MyFile in MyFiles)
			{
				Regex MyRegex = new Regex(@"C:\\ProgramData\\Epic\\EpicGamesLauncher\\Data\\Manifests\\[0-9A-F]*.item");
				Match MyMatch = MyRegex.Match(MyFile);

				// Regex Match them to see if we like them
				if (MyMatch.Success)
				{
					// Get all Lines of that File
					string[] MyLines = HelperClasses.FileHandling.ReadFileEachLine(MyFile);

					// Loop through those Lines
					for (int i = 0; i <= MyLines.Length - 1; i++)
					{
						// Clear them of Tabs and Spaces
						MyLines[i] = MyLines[i].Replace("\t", "").Replace(" ", "");
						MyLines[i] = MyLines[i].TrimEnd(',').TrimEnd('"');

						// if DisplayName is something else, lets exit
						if (MyLines[i].Contains("\"DisplayName\":"))
						{
							if (!MyLines[i].Contains("GrandTheftAutoV"))
							{
								break;
							}
						}


						if (MyLines[i].Contains("\"InstallLocation\":"))
						{
							string path = MyLines[i].Substring(MyLines[i].LastIndexOf('"')).Replace(@"\\", @"\");
							HelperClasses.Logger.Log("GTAV Path Magic by Epic detected to be: '" + path + "'", 3);
							return path;
						}
					}
				}
			}
			HelperClasses.Logger.Log("GTAV Path Magic by Epic didnt work", 3);
			return "";
		}


		/// <summary>
		/// "Cleanest" way of getting the GTA V Path automatically
		/// </summary>
		/// <returns></returns>
		public static string GetGTAVPathMagicRockstar()
		{
			HelperClasses.Logger.Log("GTAV Path Magic by Rockstar", 2);
			RegistryKey myRK2 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{5EFC6C07-6B87-43FC-9524-F9E967241741}");
			return HelperClasses.RegeditHandler.GetValue(myRK2, "InstallLocation");
		}


		/// <summary>
		/// "Cleanest" way of getting the GTA V Path automatically
		/// </summary>
		/// <returns></returns>
		public static string GetGTAVPathMagicSteam()
		{
			HelperClasses.Logger.Log("GTAV Path Magic by steam", 2);

			// Get all Lines of that File
			string[] MyLines = HelperClasses.FileHandling.ReadFileEachLine(Globals.SteamInstallPath.TrimEnd('\\') + @"\steamapps\libraryfolders.vdf");

			// Loop through those Lines
			for (int i = 0; i <= MyLines.Length - 1; i++)
			{
				// Clear them of Tabs and Spaces
				MyLines[i] = MyLines[i].Replace("\t", "").Replace(" ", "");

				// String from Regex: #"\d{1,4}""[a-zA-Z\\:]*"# (yes we are matching ", I used # as semicolons for string beginnign and end
				Regex MyRegex = new Regex("\"\\d{1,4}\"\"[a-zA-Z\\\\:]*\"");
				Match MyMatch = MyRegex.Match(MyLines[i]);

				// Regex Match them to see if we like them
				if (MyMatch.Success)
				{
					// Do some other stuff to get the actual path from it
					MyLines[i] = MyLines[i].TrimEnd('"');
					MyLines[i] = MyLines[i].Substring(MyLines[i].LastIndexOf('"') + 1);
					MyLines[i] = MyLines[i].Replace(@"\\", @"\");

					// If the Path contains this file, it is the GTA V Path
					if (HelperClasses.FileHandling.doesFileExist(MyLines[i].TrimEnd('\\') + @"\steamapps\appmanifest_271590.acf"))
					{
						// Build the Path to GTAV
						MyLines[i] = MyLines[i].TrimEnd('\\') + @"\steamapps\common\Grand Theft Auto V\";

						// Check if we can find a file from the game
						if (IsGTAVInstallationPathCorrect(MyLines[i]))
						{
							HelperClasses.Logger.Log("GTAV Path Magic by steam detected to be: '" + MyLines[i] + "'", 3);
							return MyLines[i];
						}
					}
				}
			}
			HelperClasses.Logger.Log("GTAV Path Magic by steam didnt work", 3);
			return "";
		}


		/// <summary>
		/// Helper Method which kills all Rockstar / GTA / Social Club Processes
		/// </summary>
		public static void KillRelevantProcesses()
		{
			HelperClasses.Logger.Log("Trying to kill all Rockstar related Processes");
			HelperClasses.ProcessHandler.KillRockstarProcesses();
		}

		/// <summary>
		/// Checks if Parameter Path is a correct GTA V Installation Path
		/// </summary>
		/// <param name="pPath"></param>
		/// <returns></returns>
		public static bool IsGTAVInstallationPathCorrect(string pPath, bool pLogThis = true)
		{
			if (pLogThis) { HelperClasses.Logger.Log("Trying to see if GTAV Installation Path ('" + pPath + "') is a theoretical valid Path", 3); }
			if (HelperClasses.FileHandling.doesFileExist(pPath.TrimEnd('\\') + @"\x64b.rpf"))
			{
				if (pLogThis) { HelperClasses.Logger.Log("It is", 4); }
				return true;
			}
			else
			{
				if (pLogThis) { HelperClasses.Logger.Log("It is not", 4); }
				return false;
			}
		}

		/// <summary>
		/// Checks if Settings.GTAVInstallationPath is a correct GTA V Installation Path
		/// </summary>
		/// <returns></returns>
		public static bool IsGTAVInstallationPathCorrect()
		{
			return IsGTAVInstallationPathCorrect(Settings.GTAVInstallationPath);
		}


	} // End of Class
} // End of NameSpace

