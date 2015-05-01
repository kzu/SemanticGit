using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;
using System.Text.RegularExpressions;

namespace SemanticGit
{
	/// <summary>
	/// Fixes up missing semi-colons in xbuild since it does not recognize any of the 
	/// escape characters for the ';' character.
	/// </summary>
	public class FixupCode : Task
	{
		/// <summary>
		/// The file to apply the fixups to.
		/// </summary>
		[Required]
		public Microsoft.Build.Framework.ITaskItem File { get; set; }

		/// <summary>
		/// Fixes up missing semi-colons in xbuild since it does not recognize any of the 
		/// escape characters for the ';' character.
		/// </summary>
		public override bool Execute()
		{
			var missingSemicolon = new Regex("\"$", RegexOptions.Multiline);
			var contents = System.IO.File.ReadAllText(File.ItemSpec);
			var replaced = missingSemicolon.Replace(contents, "\";");

			if (contents != replaced) {
				Log.LogMessage (MessageImportance.Normal, "Found missing semi-colons in the input file {0}. Applying fix.", File.ItemSpec);
				System.IO.File.WriteAllText (File.ItemSpec, replaced);

				Log.LogMessage (MessageImportance.Low, @"Replaced: 
{0}

With:
{1}", contents, replaced);

			}

			return true;
		}
	}
}
