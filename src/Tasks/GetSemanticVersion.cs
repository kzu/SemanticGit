namespace SemanticGitFlow
{
	using Microsoft.Build.Utilities;

	#region Using

	using System.Text.RegularExpressions;

	#endregion

	/*
    ============================================================
              GetSemanticVersion Task
			  
	Provides http://semver.org/ compatible version number
	from gitflow tagged repositories, making it easy to 
	have ongoing pre-release software that has easily 
	reproducible builds.
	
    [IN]
	Tag        - The tag returned by git describe -tags

    [OUT]
	Major      - The MAJOR component of the semantic version tag
	Minor      - The MINOR component of the semantic version tag
	Patch      - The PATCH component of the semantic version tag
	Commit     - The optional SHA value of the last commit after 
                 tagging (none for the release immediate after tagging). 
                    
                 i.e.:
                 
                 "1.0.0" = Repo HEAD is at the tag 1.0.0 itself, 
                 with zero commits after the tag.
                 
                 "1.0.1-928e945" = Repo is a pre-release with 
                 one commit after the "1.0.0" release, with hash 
                 928e945.
	============================================================
	*/
	public partial class GetSemanticVersion : Task
	{
		#region Input

		public string Tag { get; set; }

		#endregion

		#region Output

		public string Major { get; set; }
		public string Minor { get; set; }
		public string Patch { get; set; }
		public string Commit { get; set; }

		#endregion

		public override bool Execute()
		{
			#region Code

			var semanticGitExpression = @"^v?(?<Major>\d+)\.(?<Minor>\d+)\.(?<Patch>\d+)(-(?<Revision>\d+)-g(?<Commit>[0-9a-z]+))?$";

			var match = Regex.Match(Tag, semanticGitExpression, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

			if (!match.Success)
			{
				Log.LogError("Current head tag {0} does not comply to semantic versioning. Must be MAYOR.MINOR.PATCH[-COMMITS-gHASH].", Tag);
				return false;
			}

			Major = match.Groups["Major"].Value;
			Minor = match.Groups["Minor"].Value;
			Commit = match.Groups["Commit"].Value;

			var patch = int.Parse(match.Groups["Patch"].Value);

			// If there are commits on top, we add them to the patch number.
			if (match.Groups["Revision"].Success)
				patch += int.Parse(match.Groups["Revision"].Value);

			Patch = patch.ToString();

			#endregion

			return true;
		}
	}
}
