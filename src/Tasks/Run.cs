namespace SemanticGit
{
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	#region Using

	using System.Diagnostics;

	#endregion

	/*
    ============================================================
              Run Task
	
        [IN]
        Exe	       - The path to the executable
		Args       - The optional command line arguments to pass
		WorkingDir - The base directory to use

        [OUT]
		Output     - The raw text output string from a 
	                 successful run
	============================================================
	*/
	/// <summary>
	/// Runs the specified executable, redirecting standard 
	/// output and error so that it's visible on MSBuild.
	/// </summary>
	public class Run : Task
	{
		#region Input

		/// <summary>
		/// Gets or sets the path to the executable.
		/// </summary>
		[Required]
		public string Exe { get; set; }

		/// <summary>
		/// Gets or sets the optional command line arguments to pass.
		/// </summary>
		public string Args { get; set; }

		/// <summary>
		/// Gets or sets the base directory to use.
		/// </summary>
		[Required]
		public string WorkingDir { get; set; }
		
		#endregion

		#region Output

		/// <summary>
		/// Gets or sets the raw text output string from a 
		/// successful run.
		/// </summary>
		[Output]
		public string Output { get; set; }

		#endregion

		/// <summary>
		/// Executes the specified executable in the given working 
		/// directory, passing the optional arguments and capturing
		/// the output.
		/// </summary>
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
