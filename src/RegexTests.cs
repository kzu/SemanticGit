using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace SemanticGitFlow
{
	public class RegexTests
	{
		[Fact]
		public void when_parsing_class_then_can_retrieve_name()
		{
			var source = File.ReadAllText(@"..\..\GetSemanticVersion.cs");

			var classNameExpr = @"class (?<name>[^\s]+) :";

			var name = Regex.Match(source, classNameExpr)
				.Groups["name"].Value;

			Assert.Equal("GetSemanticVersion", name);
		}

		[Fact]
		public void when_parsing_usings_then_can_retrieve_existing()
		{
			var source = File.ReadAllText(@"..\..\GetSemanticVersion.cs");
			
			var usingExpr = @"\#region Using(?<using>[^\#]+)#endregion";

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
			var source = File.ReadAllText(@"..\..\GetSemanticVersion.cs");
			
			var docExpr = @"\/\*(?<doc>.+)\*\/";

			Assert.True(Regex.IsMatch(source, docExpr, RegexOptions.Singleline));

			var doc = Regex.Match(source, docExpr, RegexOptions.Singleline)
				.Groups["doc"].Value;

			Assert.NotEmpty(doc);
		}

		[Fact]
		public void when_parsing_inputs_then_can_retrieve_properties()
		{
			var source = File.ReadAllText(@"..\..\GetSemanticVersion.cs");
			
			var inputExpr = @"\#region Input(?<input>[^\#]+)#endregion";

			Assert.True(Regex.IsMatch(source, inputExpr, RegexOptions.Singleline));

			var inputs = Regex.Match(source, inputExpr, RegexOptions.Singleline)
				.Groups["input"].Value
				.Trim()
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(line => line.Trim())
				.Select(line => line.Substring(7))
				.Select(line => line.Substring(0, line.IndexOf('{')).Split(' '))
				.Select(line => new { Type = line[0], Name = line[1] })
				.ToList();

			Assert.Equal(1, inputs.Count);
			Assert.Equal("string", inputs[0].Type);
			Assert.Equal("Tag", inputs[0].Name);
		}

		[Fact]
		public void when_parsing_outputs_then_can_retrieve_properties()
		{
			var source = File.ReadAllText(@"..\..\GetSemanticVersion.cs");
			
			var outputExpr = @"\#region Output(?<output>[^\#]+)#endregion";

			Assert.True(Regex.IsMatch(source, outputExpr, RegexOptions.Singleline));

			var outputs = Regex.Match(source, outputExpr, RegexOptions.Singleline)
				.Groups["output"].Value
				.Trim()
				.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(line => line.Trim())
				.Select(line => line.Substring(7))
				.Select(line => line.Substring(0, line.IndexOf('{')).Split(' '))
				.Select(line => new { Type = line[0], Name = line[1] })
				.ToList();

			Assert.Equal(4, outputs.Count);
			Assert.Equal("string", outputs[0].Type);
			Assert.Equal("Major", outputs[0].Name);
		}

		[Fact]
		public void when_parsing_code_then_can_retrieve_existing()
		{
			var source = File.ReadAllText(@"..\..\GetSemanticVersion.cs");
			
			var codeExpr = @"\#region Code(?<code>[^\#]+)#endregion";

			Assert.True(Regex.IsMatch(source, codeExpr, RegexOptions.Singleline));

			var code = Regex.Match(source, codeExpr, RegexOptions.Singleline)
				.Groups["code"].Value;

			Assert.NotEmpty(code);
		}
	}
}
