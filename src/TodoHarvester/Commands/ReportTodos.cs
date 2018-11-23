using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Oakton;
using TodoHarvester.Infrastructure;
using TodoHarvester.Model;
using TodoHarvester.Services;

namespace TodoHarvester.Commands
{
	public abstract class ReportTodos<T> : OaktonAsyncCommand<T> where T : TodoInput
	{
		protected abstract ITodoReporter ReporterFrom(T input);

		public override async Task<bool> Execute(T input)
		{
			var service = new TodoFinder();

			var solutionProperties = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(input.TargetFrameworkFlag))
			{
				solutionProperties["TargetFramework"] = input.TargetFrameworkFlag;
			}

			var reporter = ReporterFrom(input);

			var flags = RegexOptions.Compiled;

			var regexes =
				(input.TodoRegexesFlag ?? new List<string> { Constants.DefaultTodoPattern }).Select(x =>
					new Regex(x, flags));

			await service.FindAndReportTodos(input.Solutions.Where(File.Exists), regexes, reporter, input.KeepWhitespaceFlag, solutionProperties);

			if (reporter is IDisposable disposable)
			{
				disposable.Dispose();
			}

			return true;
		}
	}
}