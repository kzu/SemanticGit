namespace SemanticGitFlow
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Xunit;

	public class RegexTests
	{
		const string SourceFile = @"..\..\..\Tasks\GetSemanticVersion.cs";
		
		string usingExpr = @"\#region Using(?<using>[^\#]+)#endregion";
		string docExpr = @"\/\*(?<doc>.+)\*\/";
		string classNameExpr = @"class (?<name>[^\s]+) :";
		string codeExpr = @"\#region Code(?<code>[^\#]+)#endregion";

		string inputBlockExpr = @"\#region Input(?<input>[^\#]+)\#endregion";
		string outputBlockExpr = @"\#region Output(?<output>[^\#]+)\#endregion";
       	string propertyExpr = @"(?<required>\[Required\].*?)?public (?<type>[^\s]+) (?<name>[^\s]+) { get; set; }";

		[Fact]
		public void when_parsing_class_then_can_retrieve_name()
		{
			var source = File.ReadAllText(SourceFile);

			var name = Regex.Match(source, classNameExpr)
				.Groups["name"].Value;

			Assert.Equal("GetSemanticVersion", name);
		}

		[Fact]
		public void when_parsing_usings_then_can_retrieve_existing()
		{
			var source = File.ReadAllText(SourceFile);

			Assert.True(Regex.IsMatch(source, usingExpr, RegexOptions.Singleline));

			var usings = Regex.Match(source, usingExpr, RegexOptions.Singleline)
				.Groups["using"].Value
				.Trim()
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(line => line.Trim())
				.Select(line => line.Substring(6, line.Length - 7))
				.ToList();

			Assert.Equal(1, usings.Count);
			Assert.Equal("System.Text.RegularExpressions", usings[0]);
		}

		[Fact]
		public void when_getting_documentation_then_can_retrieve_it()
		{
			var source = File.ReadAllText(SourceFile);

			var docExpr = @"\/\*(?<doc>.+)\*\/";

			Assert.True(Regex.IsMatch(source, docExpr, RegexOptions.Singleline));

			var doc = Regex.Match(source, docExpr, RegexOptions.Singleline)
				.Groups["doc"].Value;

			Assert.NotEmpty(doc);
		}

		[Fact]
		public void when_parsing_inputs_then_can_retrieve_properties()
		{
			var source = @"
public partial class Foo : Task
{
	#region Input

	[Required]
	public string Input { get; set; }

	[Required]
	public string HeadTag { get; set; }

	public string HeadTagText { get; set; }

	#endregion
}";

			Assert.True(Regex.IsMatch(source, inputBlockExpr, RegexOptions.Singleline));

			var block = Regex.Match(source, inputBlockExpr, RegexOptions.Singleline)
				.Groups["input"].Value
				.Trim();

			var inputs = Regex.Matches(block, propertyExpr, RegexOptions.Singleline)
				.OfType<Match>()
				.Select(match => new 
				{ 
					Name = match.Groups["name"].Value,
					Type = match.Groups["type"].Value, 
					Required = match.Groups["required"].Success,
				})
				.ToList();

			Assert.Equal(3, inputs.Count);
			Assert.Equal(3, inputs.Where(t => t.Type == "string").Count());
			Assert.Equal("Input", inputs[0].Name);
			Assert.Equal("HeadTag", inputs[1].Name);
			Assert.Equal("HeadTagText", inputs[2].Name);
		}

		[Fact]
		public void when_parsing_outputs_then_can_retrieve_properties()
		{
			var source = @"
	public partial class Foo : Task
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
}";

			var block = Regex.Match(source, outputBlockExpr, RegexOptions.Singleline)
				.Groups["output"].Value
				.Trim();

			var outputs = Regex.Matches(block, propertyExpr, RegexOptions.Singleline)
				.OfType<Match>()
				.Select(match => new 
				{ 
					Name = match.Groups["name"].Value,
					Type = match.Groups["type"].Value, 
					Required = match.Groups["required"].Success,
				})
				.ToList();

			Assert.Equal(4, outputs.Count);
			Assert.Equal("string", outputs[0].Type);
			Assert.Equal("Major", outputs[0].Name);
		}

		[Fact]
		public void when_parsing_code_then_can_retrieve_existing()
		{
			var source = File.ReadAllText(SourceFile);

			Assert.True(Regex.IsMatch(source, codeExpr, RegexOptions.Singleline));

			var code = Regex.Match(source, codeExpr, RegexOptions.Singleline)
				.Groups["code"].Value;

			Assert.NotEmpty(code);
		}

		[Fact]
		public void when_parsing_required_inputs_then_can_retrieve_required_properties()
		{
			var source = @"
public partial class Foo : Task
{
	#region Input

	[Required]
	public string Input { get; set; }

	[Required]
	public string HeadTag { get; set; }

	public string HeadTagText { get; set; }

	#endregion
}";

			Assert.True(Regex.IsMatch(source, inputBlockExpr, RegexOptions.Singleline));

			var block = Regex.Match(source, inputBlockExpr, RegexOptions.Singleline)
				.Groups["input"].Value
				.Trim();

			var inputs = Regex.Matches(block, propertyExpr, RegexOptions.Singleline)
				.OfType<Match>()
				.Select(match => new 
				{ 
					Name = match.Groups["name"].Value,
					Type = match.Groups["type"].Value, 
					Required = match.Groups["required"].Success,
				})
				.ToList();

			Assert.Equal(3, inputs.Count);
			Assert.Equal(2, inputs.Count(x => x.Required));
			Assert.Equal(1, inputs.Count(x => !x.Required));
		}

		[Fact]
		public void when_parsing_required_inputs_then_can_retrieve_non_required_properties()
		{
			var source = @"
public partial class Foo : Task
{
	#region Input

		[Required]
		public string Exe { get; set; }

		public string Args { get; set; }
	
		[Required]
		public string WorkingDir { get; set; }

	#endregion
}";

			Assert.True(Regex.IsMatch(source, inputBlockExpr, RegexOptions.Singleline));

			var block = Regex.Match(source, inputBlockExpr, RegexOptions.Singleline)
				.Groups["input"].Value
				.Trim();

			var inputs = Regex.Matches(block, propertyExpr, RegexOptions.Singleline)
				.OfType<Match>()
				.Select(match => new 
				{ 
					Name = match.Groups["name"].Value,
					Type = match.Groups["type"].Value, 
					Required = match.Groups["required"].Success,
				})
				.ToList();

			Assert.Equal(3, inputs.Count);
			Assert.Equal(2, inputs.Count(x => x.Required));
			Assert.Equal(1, inputs.Count(x => !x.Required));
		}
	}
}
