namespace SemanticGitFlow
{
	using Microsoft.Build.Framework;
	using Moq;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Xunit;

	public class ParseSemanticTagsTests
	{
		[Fact]
		public void when_parsing_tags_then_orders_by_descending_tag()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "1.0.2",
				Input = @"1.0.0           Initial release of nothing.
1.0.1           New release test
1.0.2           Added simple task tests."
			};

			task.Execute();

			Assert.Equal("1.0.2", task.Tags[0].ItemSpec);
			Assert.Equal("1.0.1", task.Tags[1].ItemSpec);
			Assert.Equal("1.0.0", task.Tags[2].ItemSpec);
		}

		[Fact]
		public void when_parsing_tags_then_adds_revision_range_from_previous_tag_to_current()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "1.0.2",
				Input = @"1.0.0           Initial release of nothing.
1.0.1           New release test
1.0.2           Added simple task tests."
			};

			task.Execute();

			Assert.Equal("1.0.1..1.0.2", task.Tags[0].GetMetadata("Range"));
			Assert.Equal("1.0.0..1.0.1", task.Tags[1].GetMetadata("Range"));
		}

		[Fact]
		public void when_parsing_tags_then_oldest_tag_has_no_revision_range()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "1.0.2",
				Input = @"1.0.0           Initial release of nothing.
1.0.1           New release test
1.0.2           Added simple task tests."
			};

			task.Execute();

			Assert.Equal("1.0.0", task.Tags[2].GetMetadata("Range"));
		}

		[Fact]
		public void when_head_is_different_than_newest_tag_then_adds_it_to_list()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "1.0.2-2-gfeee9ba",
				Input = @"1.0.0           Initial release of nothing.
1.0.1           New release test
1.0.2           Added simple task tests."
			};

			task.Execute();

			Assert.Equal(4, task.Tags.Length);
			Assert.Equal("1.0.2-2-gfeee9ba", task.Tags[0].ItemSpec);
			Assert.Equal("- HEAD", task.Tags[0].GetMetadata("Description"));
		}

		[Fact]
		public void when_head_has_commits_then_tag_has_title_semver()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "1.1.0-2-gfeee9ba",
				Input = @"1.0.0           Initial release of nothing.
1.0.1           New release test
1.1.0           Added simple task tests."
			};

			task.Execute();

			Assert.Equal(4, task.Tags.Length);
			Assert.Equal("1.1.0-2-gfeee9ba", task.Tags[0].ItemSpec);
			Assert.Equal("1.1.2-feee9ba", task.Tags[0].GetMetadata("Title"));
		}

		[Fact]
		public void when_head_has_commits_then_semver_adds_to_patch()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "v1.0.2-2-gfeee9ba",
				Input = @"v1.0.0           Initial release of nothing.
v1.0.1           New release test
v1.0.2           Added simple task tests."
			};

			task.Execute();

			Assert.Equal(4, task.Tags.Length);
			Assert.Equal("v1.0.2-2-gfeee9ba", task.Tags[0].ItemSpec);
			Assert.Equal("v1.0.4-feee9ba", task.Tags[0].GetMetadata("Title"));
		}

		[Fact]
		public void when_head_has_commits_then_semver_adds_to_patch2()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "v1.0.0-6-g778787d",
				Input = @"v0.1.0
v0.2.0
v1.0.0"
			};

			task.Execute();

			Assert.Equal(4, task.Tags.Length);
			Assert.Equal("v1.0.6-778787d", task.Tags[0].GetMetadata("Title"));
		}

		[Fact]
		public void when_head_has_commits_but_no_semver_then_skips_all_tags()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "KK-2-gfeee9ba",
				Input = @"KK
JB
ICS"
			};

			task.Execute();

			Assert.Equal(0, task.Tags.Length);
		}

		[Fact]
		public void when_tags_contains_non_semver_then_skips_tag()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "1.0.2",
				Input = @"1.0.0
BETA
1.0.2"
			};

			task.Execute();

			Assert.Equal(2, task.Tags.Length);
		}

		[Fact]
		public void when_head_is_same_as_newest_tag_then_no_new_head_is_added()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "1.0.2",
				Input = @"1.0.0           Initial release of nothing.
1.0.1           New release test
1.0.2           Added simple task tests."
			};

			task.Execute();

			Assert.Equal(3, task.Tags.Length);
		}

		[Fact]
		public void when_head_is_simple_tag_then_can_retrieve_semantic_version()
		{
			var task = new GetSemanticVersion
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				Tag = "1.1.0"
			};

			task.Execute();

			Assert.Equal("1.1.0", task.FullVersion);
		}

		[Fact]
		public void when_head_uses_v_prefix_then_can_retrieve_semantic_version()
		{
			var task = new GetSemanticVersion
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				Tag = "v1.1.0"
			};

			task.Execute();

			Assert.Equal("1.1.0", task.FullVersion);
		}

		[Fact]
		public void when_head_has_commits_after_tag_then_can_retrieve_semantic_version_with_hash()
		{
			var task = new GetSemanticVersion
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				Tag = "1.1.0-1-g928e945"
			};

			task.Execute();

			Assert.Equal("1.1.1-928e945", task.FullVersion);
		}

		[Fact]
		public void when_head_tag_smaller_than_newest_tag_then_newest_tags_skipped()
		{
			// This case happens when we are in the 2.0 release branch 
			// and a 3.0 release has already shipped.
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "2.0.0",
				Input = @"1.0.0
1.1.0
2.0.0
3.0.0"
			};

			task.Execute();

			Assert.Equal(3, task.Tags.Length);

			Assert.Equal("2.0.0", task.Tags[0].ItemSpec);
			Assert.Equal("1.1.0", task.Tags[1].ItemSpec);
			Assert.Equal("1.0.0", task.Tags[2].ItemSpec);
		}

		[Fact]
		public void when_head_tag_smaller_than_newest_tag_then_newest_tags_skipped_but_patch_included()
		{
			// This case happens when we are in the 2.0 release branch 
			// and a 3.0 release has already shipped.
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "2.0.0-2-gasdfasdf",
				Input = @"1.0.0
1.1.0
2.0.0
3.0.0"
			};

			task.Execute();

			Assert.Equal(4, task.Tags.Length);

			Assert.Equal("2.0.0-2-gasdfasdf", task.Tags[0].ItemSpec);
			Assert.Equal("2.0.0", task.Tags[1].ItemSpec);
			Assert.Equal("1.1.0", task.Tags[2].ItemSpec);
			Assert.Equal("1.0.0", task.Tags[3].ItemSpec);
		}
	}
}
