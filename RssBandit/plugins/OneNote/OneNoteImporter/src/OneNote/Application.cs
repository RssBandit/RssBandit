using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Contains utility and helper methods applicable to the OneNote application.
	/// </summary>
	public sealed class Application
	{
		// Keeps users from instantiating:
		private Application()
		{
		}

		/// <summary>
		/// Activates the OneNote application, bringing it to the foreground.
		/// </summary>
		public static void Activate()
		{
			Process[] processes = Process.GetProcessesByName("onenote");

			if (processes.Length > 0)
			{
				IntPtr window = processes[0].MainWindowHandle;

				if (NativeMethods.IsIconic(window))
				{
					NativeMethods.ShowWindowAsync(window, NativeMethods.SW_RESTORE);
				}

				if (window == NativeMethods.GetForegroundWindow())
					return;

				// If the window isn't the foreground thread (which is likely)
				// then calling SetForegroundWindow will only make the taskbar
				// icon blink.  We can workaround this by attaching the OneNote
				// window's thread input to the foreground window's thread input.
				IntPtr thread = NativeMethods.GetWindowThreadProcessId(window, IntPtr.Zero);
				IntPtr foregroundThread = NativeMethods.GetWindowThreadProcessId(NativeMethods.GetForegroundWindow(), IntPtr.Zero);

				if (thread != foregroundThread)
				{
					NativeMethods.AttachThreadInput(foregroundThread, thread, 1 /* TRUE */);
					NativeMethods.SetForegroundWindow(window);
					NativeMethods.AttachThreadInput(foregroundThread, thread, 0 /* FALSE */);
				}
				else
				{
					NativeMethods.SetForegroundWindow(window);
				}
			}
			else
			{
				Process.Start(GetExecutablePath());
			}
		}

		/// <summary>
		/// Opens a new OneNote side note window.
		/// </summary>
		public static void StartSideNote()
		{
			Process.Start(GetExecutablePath(), "/sidenote");
		}

		/// <summary>
		/// Opens the specified section in OneNote. 
		/// </summary>
		/// <param name="sectionPath">
		/// The path to the section to be opened, expressed absolutely or relative 
		/// to the notebook.
		/// </param>
		public static void Open(string sectionPath)
		{
			Open(sectionPath, false);
		}

		/// <summary>
		/// Opens the specified section in OneNote. 
		/// </summary>
		/// <param name="sectionPath">
		/// The path to the section to be opened, expressed absolutely or relative 
		/// to the notebook.
		/// </param>
		/// <param name="openReadOnly">
		/// If true, the section will be opened in OneNote Read-Only.
		/// </param>
		public static void Open(string sectionPath, bool openReadOnly)
		{
			string arguments = RootSectionPath(sectionPath);

			if (openReadOnly)
				arguments = "/openro " + arguments;

			Process.Start(GetExecutablePath(), arguments);
		}

		/// <summary>
		/// Prints the specified OneNote section.
		/// </summary>
		/// <param name="sectionPath">
		/// The path to the section to be opened, expressed absolutely or relative 
		/// to the notebook.
		/// </param>
		public static void Print(string sectionPath)
		{
			string arguments = "/print " + RootSectionPath(sectionPath);
			Process.Start(GetExecutablePath(), arguments);
		}

		/// <summary>
		/// Starts video notebook.
		/// </summary>
		public static void StartVideoNote()
		{
			Process.Start(GetExecutablePath(), "/videonote");
		}

		/// <summary>
		/// Starts video notebook.
		/// </summary>
		/// <param name="recordingProfilePath">
		/// A fully qualified path to a recording profile file that contains
		/// information such as bit rate, sampling, and decoding.  
		/// </param>
		public static void StartVideoNote(string recordingProfilePath)
		{
			Process.Start(GetExecutablePath(), "/recordingprofile " + recordingProfilePath + " /videonote");
		}

		/// <summary>
		/// Starts video notebook.
		/// </summary>
		/// <param name="videoDevice">
		/// The name of the video device that should be used, or a substring that
		/// can be used to match against the set of devices enumerated from the
		/// system.
		/// </param>
		/// <param name="audioDevice">
		/// The name of the audio device that should be used, or a substring that
		/// can be used to match against the set of devices enumerated from the
		/// system.
		/// </param>
		public static void StartVideoNote(string videoDevice, string audioDevice)
		{
			Process.Start(GetExecutablePath(), "/videonote " + videoDevice + " " + audioDevice);
		}

		/// <summary>
		/// Starts video notebook.
		/// </summary>
		/// <param name="videoDevice">
		/// The name of the video device that should be used, or a substring that
		/// can be used to match against the set of devices enumerated from the
		/// system.
		/// </param>
		/// <param name="audioDevice">
		/// The name of the audio device that should be used, or a substring that
		/// can be used to match against the set of devices enumerated from the
		/// system.
		/// </param>
		/// <param name="recordingProfilePath">
		/// A fully qualified path to a recording profile file that contains
		/// information such as bit rate, sampling, and decoding.  
		/// </param>
		public static void StartVideoNote(string videoDevice, string audioDevice, string recordingProfilePath)
		{
			Process.Start(GetExecutablePath(), "/videonote " + videoDevice + " " + audioDevice + " " + recordingProfilePath);
		}

		/// <summary>
		/// Starts audio notebook.
		/// </summary>
		public static void StartAudioNote()
		{
			Process.Start(GetExecutablePath(), "/audionote");
		}

		/// <summary>
		/// Pauses video or audio recording in the current instance of OneNote.  You can resume
		/// recording by calling this method again.
		/// </summary>
		public static void PauseRecording()
		{
			Process.Start(GetExecutablePath(), "/pauserecording");
		}

		/// <summary>
		/// Stops recording the video or audio note in the current instance of OneNote.
		/// </summary>
		public static void StopRecording()
		{
			Process.Start(GetExecutablePath(), "/stoprecording");
		}

		/// <summary>
		/// Initiates a shared OneNote session.
		/// </summary>
		public static void StartSharedSession()
		{
			Process.Start(GetExecutablePath(), "/startsharing");
		}

		/// <summary>
		/// Initiates a shared OneNote session.
		/// </summary>
		/// <param name="password">
		/// The session password.  Note that the password should not contain spaces.
		/// </param>
		/// <param name="sectionPath">
		/// The path of the section that should be used to host the shared session.
		/// </param>
		public static void StartSharedSession(string password, string sectionPath)
		{
			string arguments = "/startsharing " + password + " " + RootSectionPath(sectionPath);
			Process.Start(GetExecutablePath(), arguments);
		}

		/// <summary>
		/// Joins a shared OneNote session.
		/// </summary>
		/// <param name="sessionAddress">
		/// The address of the session host.
		/// </param>
		public static void JoinSharedSession(string sessionAddress)
		{
			Process.Start(GetExecutablePath(), "/joinsharing " + sessionAddress);
		}

		/// <summary>
		/// Joins a shared OneNote session.
		/// </summary>
		/// <param name="sessionAddress">
		/// The address of the session host.
		/// </param>
		/// <param name="password">
		/// The password for the session.
		/// </param>
		/// <param name="sectionPath">
		/// The path of the section that should be used to join the shared session.
		/// </param>
		public static void JoinSharedSession(string sessionAddress, string password, string sectionPath)
		{
			string arguments = "/joinsharing " + password + " " + RootSectionPath(sectionPath);
			Process.Start(GetExecutablePath(), arguments);
		}

		/// <summary>
		/// Computes the path to the user's "My Notebook". 
		/// </summary>
		/// <returns>The notebook path as a string.</returns>
		public static string GetNotebookPath()
		{
			string notebookPath = "My Notebook";

			// The Notebook Path is stored in the Save options in the registry:
			string saveKey = "Software\\Microsoft\\Office\\11.0\\OneNote\\Options\\Save";
			using (RegistryKey saveOptions = Registry.CurrentUser.OpenSubKey(saveKey))
			{
				if (saveOptions != null)
					notebookPath = saveOptions.GetValue("My Notebook path", notebookPath).ToString();				
			}

			// This path is relative to the user's My Documents folder
			string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			return Path.Combine(documentsFolder, notebookPath);
		}

		/// <summary>
		/// Prompts the user to select a section file from their notebook.
		/// </summary>
		/// <returns>
		/// The file path (represented as a <see cref="String"/>) of the section 
		/// selected by the user, or null if they cancel the operation.
		/// </returns>
		public static string PromptForSectionPath()
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Title = "Insert into Section";
				openFileDialog.CheckFileExists = false;
				openFileDialog.CheckPathExists = false;
				openFileDialog.InitialDirectory = GetNotebookPath();
				openFileDialog.Filter = "OneNote Sections (*.one)|*.one|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					return openFileDialog.FileName;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns the fully qualified path to the registered OneNote executable.
		/// </summary>
		/// <returns>
		/// The fully qualified path to the registered OneNote executable.
		/// </returns>
		public static string GetExecutablePath()
		{
			string exePath = null;

			string comRegistration = "CLSID\\{22148139-F1FC-4EB0-B237-DFCD8A38EFFC}\\LocalServer32";
			using (RegistryKey localServer = Registry.ClassesRoot.OpenSubKey(comRegistration))
			{
				if (localServer != null)
					exePath = (string) localServer.GetValue(null);
			}

			if (exePath != null)
			{
				// Is the path quoted?
				if (exePath.StartsWith("\""))
					exePath = exePath.Remove(0, 1);
				if (exePath.EndsWith("\""))
					exePath = exePath.Remove(exePath.Length - 1, 1);
			}

			return exePath;
		}

		/// <summary>
		/// Returns the version of OneNote as a <see cref="FileVersionInfo"/>.
		/// </summary>
		/// <returns>
		/// The version of OneNote as a <see cref="FileVersionInfo"/>.
		/// </returns>
		public static FileVersionInfo GetVersion()
		{
			string exePath = GetExecutablePath();
			if (exePath != null)
			{
				return FileVersionInfo.GetVersionInfo(exePath);
			}

			return null;
		}

		/// <summary>
		/// Helper method to process the section path for spaces and convert 
		/// all relative paths into their absolute rooted form.
		/// </summary>
		private static string RootSectionPath(string sectionPath)
		{
			if (!Path.IsPathRooted(sectionPath))
			{
				sectionPath = Path.Combine(GetNotebookPath(), sectionPath);
			}

			sectionPath = sectionPath.Trim();
			if (!sectionPath.StartsWith("\""))
				sectionPath = "\"" + sectionPath;
			if (!sectionPath.EndsWith("\""))
				sectionPath = sectionPath + "\"";

			return sectionPath;
		}
	}
}