using System.Linq;
using System.Text.RegularExpressions;
using TodoHarvester.Analyzers;
using TodoHarvester.Tests.Infrastructure;
using Xunit;

namespace TodoHarvester.Tests.Analyzers
{
	public sealed class TodoCommentAnalyzerTests
	{
		[Fact]
		public async void CanFindSingleAndMultilineTodos()
		{
			var cmnt1 = "Refactor to a simpler model";
			var cmnt2 = "Rename variable\nSecond line";
			var cmnt3 = "Something else";

			var cmnts = new[] {cmnt1, cmnt2.Replace("\n", " "), cmnt3};

			var analyzer = new TodoCommentAnalyzer(new [] { "(?si)(?<=\\W|^)(TODO)(\\W|$)(?<TODO>.*)" }.Select(x => new Regex(x, RegexOptions.IgnoreCase)));
			await TestHelper.GetDiagnosticsAsync(analyzer,
				$@"
class TestClass
{{
	// TODO:{cmnt1}
	void TestMethod()
	{{
/*
	TODO: {cmnt2}
*/
		var n = 1;
		// TODO {cmnt3}
	}}
}}");
			var comments = analyzer.GetTodoComments();

			Assert.Equal(cmnts.OrderBy(x => x), comments.Select(x => x.Comment).OrderBy(x => x));
		}

		[Fact]
		public async void CanFindCommentContext()
		{
			var analyzer = new TodoCommentAnalyzer(new[] { "(?si)(?<=\\W|^)(TODO)(\\W|$)(?<TODO>.*)" }.Select(x => new Regex(x, RegexOptions.IgnoreCase)));
			await TestHelper.GetDiagnosticsAsync(analyzer,
				@"
// TODO Root
namespace Root
{
	// TODO: Move to other namespace
	namespace ContainsTodos
	{
		/*
		 *
		 * TODO Refactor to separate concerns
		 */
		public class ComplexType
		{
			// TODO: Refactor method to smaller pieces
			public void LongMethod()
			{

			}

			public void OtherMethod()
			{
				/*
				 * TODO rename variable to express intent of use
				 */
				var n = 0;
			}
		}
	}
}");

			var comments = analyzer.GetTodoComments();

			Assert.True(comments.All(x => x.AssociatedWith.Any()));
		}
	}
}