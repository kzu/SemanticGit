namespace SemanticGitFlow
{
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	#region Using

	using System.Diagnostics;

	#endregion

	/*
    ============================================================
              RunGit Task
	
        [IN]
        Exe	       - The path to Git.exe
		Args       - The command line arguments to git
		WorkingDir - The base directory to use to run git.

        [OUT]
		Output     - The raw text output string from git.
	============================================================
	*/
	public class RunGit : Task
	{
		#region Input

		public string Exe { get; set; }
		public string Args { get; set; }
		public string WorkingDir { get; set; }
		
		#endregion

		#region Output

		public string Output { get; set; }

		#endregion

		public override bool Execute()
		{
			#region Code

			var psi = new ProcessStartInfo
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = WorkingDir,
				FileName = Exe,
				Arguments = Args
			};

			Log.LogMessage(MessageImportance.Low, "Executing: {0} {1}", Exe, Args);

			var p = Process.Start(psi);
			Output = p.StandardOutput.ReadToEnd().Trim();
			Log.LogMessage(MessageImportance.Low, Output);

			var errors = p.StandardError.ReadToEnd().Trim();
			if (errors.Length > 0)
			{
				Log.LogError(errors);
				return false;
			}

			#endregion

			return true;
		}
	}
}
