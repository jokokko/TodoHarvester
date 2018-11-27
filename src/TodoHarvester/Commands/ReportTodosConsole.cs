using Oakton;
using TodoHarvester.Model;
using TodoHarvester.Services;

namespace TodoHarvester.Commands
{
	[Description("Search todo comments & report them in console", Name = "report-todos")]
	// ReSharper disable once UnusedMember.Global
	public sealed class ReportTodosConsole : ReportTodos<TodoInput>
	{
		protected override ITodoReporter ReporterFrom(TodoInput input)
		{
			var reporter = new TodoConsoleReporter(input.GroupIssuesByFileFlag);
			return reporter;
		}

	}
}