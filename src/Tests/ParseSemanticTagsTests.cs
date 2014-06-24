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
		public void when_head_has_commits_then_tag_has_title_version()
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
			Assert.Equal("1.1.2", task.Tags[0].GetMetadata("Title"));
		}

		[Fact]
		public void when_head_has_commits_then_tag_has_title_version_with_prefix()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "v1.1.0-2-gfeee9ba",
				Input = @"v1.0.0           Initial release of nothing.
v1.0.1           New release test
v1.1.0           Added simple task tests."
			};

			task.Execute();

			Assert.Equal(4, task.Tags.Length);
			Assert.Equal("v1.1.0-2-gfeee9ba", task.Tags[0].ItemSpec);
			Assert.Equal("v1.1.2", task.Tags[0].GetMetadata("Title"));
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
			Assert.Equal(4, new Version(task.Tags[0].GetMetadata("Version")).Build);
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
			Assert.Equal("v1.0.6", task.Tags[0].GetMetadata("Title"));
		}

		[Fact]
		public void when_head__not_semver_then_fails()
		{
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "KK-2-gfeee9ba",
				Input = @"KK
JB
ICS"
			};

			Assert.False(task.Execute());
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
		public void when_head_is_simple_tag_then_can_retrieve_semantic_version_parts()
		{
			var task = new GetSemanticVersion
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				Tag = "1.2.3"
			};

			task.Execute();

			Assert.Equal("1", task.Major);
			Assert.Equal("2", task.Minor);
			Assert.Equal("3", task.Patch);
		}

		[Fact]
		public void when_head_uses_v_prefix_then_can_retrieve_semantic_version()
		{
			var task = new GetSemanticVersion
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				Tag = "v1.2.3"
			};

			task.Execute();

			Assert.Equal("1", task.Major);
			Assert.Equal("2", task.Minor);
			Assert.Equal("3", task.Patch);
		}

		[Fact]
		public void when_head_has_commits_after_tag_then_can_retrieve_commit_hash()
		{
			var task = new GetSemanticVersion
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				Tag = "1.2.3-1-g928e945"
			};

			task.Execute();

			Assert.Equal("928e945", task.Commit);
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

		[Fact]
		public void when_overriding_head_text_then_sets_tag_title()
		{
			// This is useful when an upcoming release wants to generate 
			// up-to-date changelog but not use the auto-generated 
			// semantic version, but an upcoming tag (i.e. we're at 
			// 1.0.55 but we are about to release 1.1.0 and we want the 
			// changelog to contain all current commits and be the 
			// definite content for the 1.1.0 release. If we just 
			// tagged and run the task, the generated (new) content 
			// with the updated label wouldn't be included in the tag 
			// since we already added on top of the tag. We'd have to 
			// delete the tag and re-apply it with the updated changelog.
			// None of this is needed if we're tagging with the same 
			// value as the changelog-generated one.
			var task = new ParseSemanticTags
			{
				BuildEngine = Mock.Of<IBuildEngine>(),
				HeadTag = "2.0.0-2-gasdfasdf",
				HeadTagText = "2.1.0",
				Input = @"1.0.0
1.1.0
2.0.0"
			};

			task.Execute();

			Assert.Equal(4, task.Tags.Length);

			Assert.Equal("2.0.0-2-gasdfasdf", task.Tags[0].ItemSpec);
			Assert.Equal("2.1.0", task.Tags[0].GetMetadata("Title"));
		}

	}
}
