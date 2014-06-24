namespace SemanticGitFlow
{
	using Microsoft.Build.Utilities;
	using Microsoft.Build.Framework;

	#region Using

	using System.Text.RegularExpressions;

	#endregion

	/*
    ============================================================
              RegexReplace Task
			  
	Performs regular expression-based replacements according 
	to http://msdn.microsoft.com/en-us/library/e7f5w83z(v=vs.110).aspx and 
	http://msdn.microsoft.com/en-us/library/ewy2t5e0(v=vs.110).aspx
	
    [IN]
	Input         - The string to search for a match.
	Pattern       - The regular expression pattern to match.
	Replacement   - The replacement string.

    [OUT]
	Result        - A new string that is identical to the input 
                    string, except that the replacement string takes 
					the place of each matched string. If pattern is 
					not matched in the current instance, the task 
					returns the original Input unchanged.                    
	============================================================
	*/
	public class RegexReplace : Task
	{
		#region Input

		[Required]
		public string Input { get; set; }

		[Required]
		public string Pattern { get; set; }

		[Required]
		public string Replacement { get; set; }

		#endregion

		#region Output

		public string Output { get; set; }

		#endregion

		public override bool Execute()
		{
			#region Code

			Output = Regex.Replace(Input, Pattern, Replacement);

			#endregion

			return true;
		}
	}
}
