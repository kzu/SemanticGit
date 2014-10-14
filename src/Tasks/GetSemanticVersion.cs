namespace SemanticGit
{
	using Microsoft.Build.Utilities;
	using Microsoft.Build.Framework;

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
	Major      - The MAJOR component of the semver git tag
	Minor      - The MINOR component of the semver git tag
	Patch      - The PATCH component of the semver git tag
	PreRelease - Optional pre-release component of a semver
	             git version tag including the initial '-' 
	             separator.
                    
                 "1.0.0" = Repo HEAD is at the tag 1.0.0 itself, 
                 with zero commits after the tag.
                 
                 "1.0.1-928e945" = Repo is a pre-release with 
                 one commit after the "1.0.0" release, with hash 
                 928e945.
	============================================================
	*/
	/// <summary>
	/// Provides http://semver.org/ compatible version number
	/// from gitflow tagged repositories, making it easy to 
	/// have ongoing pre-release software that has easily 
	/// reproducible builds.
	/// </summary>
	/// <remarks>
	/// <example>
	/// Examples:
	/// 
	/// - Given:
	/// 	TAG: v0.1.0-pre
	/// 	AssemblyInformationalVersionFormat: MAJOR.MINOR.PATCH-PRE-COMMIT
	/// - Result for the build two commits after creating the tag: 
	/// 	[assembly:AssemblyInformationalVersion("0.1.2-pre-8d07975")]
	/// 
	/// - Given:
	/// 	TAG: v0.1.0 (i.e. we removed the -pre for the current release branch)
	/// 	AssemblyInformationalVersionFormat: MAJOR.MINOR.PATCH-PRE-COMMIT
	/// - Result for the build two commits after creating the tag: 
	/// 	[assembly:AssemblyInformationalVersion("0.1.2-8d07975")]
	/// 	(NOTE: no '-pre' is present since it was not found in the tag)
	/// </example>
	/// </remarks>
	public partial class GetSemanticVersion : Task
	{
		#region Input

		/// <summary>
		/// Gets or sets the currently described tag.
		/// </summary>
		[Required]
		public string Tag { get; set; }

		#endregion

		#region Output

		/// <summary>
		/// Gets or sets the MAJOR component of the semver git tag.
		/// </summary>
		[Output]
		public string Major { get; set; }

		/// <summary>
		/// Gets or sets the MINOR component of the semver git tag.
		/// </summary>
		[Output]
		public string Minor { get; set; }
	
		/// <summary>
		/// Gets or sets the PATCH component of the semver git tag.
		/// </summary>
		[Output]
		public string Patch { get; set; }
		
		/// <summary>
		/// Gets or sets then optional pre-release component of a semver
		/// git version tag including the initial '-' separator.
		/// </summary>
		[Output]
		public string PreRelease { get; set; }

		#endregion

		/// <summary>
		/// Parses the tag and determines the individual components of the 
		/// version string and the PATCH value.
		/// </summary>
		public override bool Execute()
		{
			#region Code

			var semanticGitExpression = @"
# Initial 'v' prefix is optional, as suggested by GitHub
^v?
# Major, Minor and Patch are just digits
(?<Major>\d+)\.(?<Minor>\d+)\.(?<Patch>\d+)
# Then we can have optional commit information if there 
# are any commits on top of the most recent tag
((-(?<Commits>\d+)-g(?<Commit>[0-9a-z]+))?
# Or we can have an optional pre-release prefix followed 
# by that optional commit information. i.e.: v0.3.8-pre-5-gafa82bb
|(?<PreRelease>-.+?)(-(?<Commits>\d+)-g(?<Commit>[0-9a-z]+))?)$";

			var match = Regex.Match(Tag, semanticGitExpression, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

			if (!match.Success)
			{
				Log.LogError("Current head tag {0} does not comply to semantic versioning. Must be MAYOR.MINOR.PATCH[-PRERELEASE].", Tag);
				return false;
			}

			Major = match.Groups["Major"].Value;
			Minor = match.Groups["Minor"].Value;
			PreRelease = match.Groups["PreRelease"].Value;

			var patch = int.Parse(match.Groups["Patch"].Value);

			// If there are commits on top, we add them to the patch number.
			if (match.Groups["Commits"].Success)
				patch += int.Parse(match.Groups["Commits"].Value);

			Patch = patch.ToString();

			#endregion

			return true;
		}
	}
}
