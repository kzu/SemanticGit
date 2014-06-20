namespace SemanticGitFlow
{
	using NuGet;
	using Xunit;

	public class VersionTests
	{
		[Fact]
		public void when_parsing_version_with_sha_then_succeeds()
		{
			var version = SemanticVersion.Parse("3.3.0-SHAc322392");
		}

		[Fact]
		public void when_parsing_version_with_sha_plus_dash_then_succeeds()
		{
			var version = SemanticVersion.Parse("0.2.5-SHA-ebcfc42");
		}
	}
}
