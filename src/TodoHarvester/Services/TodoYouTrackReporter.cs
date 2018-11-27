using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TodoHarvester.Model;

namespace TodoHarvester.Services
{
	public sealed class TodoYouTrackReporter : ITodoReporter, IDisposable
	{
		private readonly string project;
		private readonly Action<IEnumerable<TodoComment>> reportImpl;
		private readonly IEnumerable<string> issueCommands;
		private readonly YouTrackReporter reporter;

		public TodoYouTrackReporter(string youTrackUri, string username, string password, string project,
			IEnumerable<string> issueCommands = null, bool groupByFile = false)
		{
			reporter = new YouTrackReporter(username, password, new Uri(youTrackUri), true);
			this.project = project;		
			this.issueCommands = issueCommands ?? Enumerable.Empty<string>();
			reportImpl = groupByFile ? (Action<IEnumerable<TodoComment>>)ReportImplGrouped : ReportImpl;
		}

		public void Report(IEnumerable<TodoComment> todos)
		{
			reportImpl(todos);
		}

		private void ReportImpl(IEnumerable<TodoComment> todos)
		{
			var todoComments = todos as TodoComment[] ?? todos.ToArray();

			Colorful.Console.WriteLine($"Total issues: {todoComments.Length}");

			foreach (var t in todoComments)
			{
				string summary;
				string description;

				var ctx = t.AssociatedWith.FirstOrDefault();

				if (ctx != null)
				{
					summary = ($"{ctx.Kind}: {ctx.Name} - {t.Comment} ({Path.GetFileName(t.Location.SourceTree.FilePath)})");
					description =
						$"{t.Location}{Environment.NewLine}{Environment.NewLine}\t{string.Join($"{Environment.NewLine}\t", t.AssociatedWith.Select(x => $"{x.Kind}: {x.Name}"))}";
				}
				else
				{
					summary = ($"{t.Comment} ({Path.GetFileName(t.Location.SourceTree.FilePath)})");
					description = t.Location.ToString();
				}

				var i = reporter.CreateIssue(project, summary, description).Result;

				foreach (var c in issueCommands)
				{
					reporter.ExecuteAgainstIssue(i, c).Wait();
				}

				Console.WriteLine($"Created {i} ({summary})");
			}
		}

		private void ReportImplGrouped(IEnumerable<TodoComment> todos)
		{
			var todoComments = todos as TodoComment[] ?? todos.ToArray();

			var toEnumerate = todoComments.GroupBy(x => x.Location.SourceTree.FilePath).ToArray();

			Colorful.Console.WriteLine($"Total issues: {todoComments.Length}, Total files: {toEnumerate.Length}{Environment.NewLine}");

			Tuple<string, string> SummaryAndDesc(TodoComment t)
			{
				string summary;
				string description;

				var ctx = t.AssociatedWith.FirstOrDefault();

				if (ctx != null)
				{
					summary = ($"{ctx.Kind}: {ctx.Name} - {t.Comment} ({Path.GetFileName(t.Location.SourceTree.FilePath)})");
					description =
						$"{t.Location}{Environment.NewLine}\t{string.Join($"{Environment.NewLine}\t", t.AssociatedWith.Select(x => $"{x.Kind}: {x.Name}"))}";
				}
				else
				{
					summary = ($"{t.Comment} ({Path.GetFileName(t.Location.SourceTree.FilePath)})");
					description = t.Location.ToString();
				}

				return Tuple.Create(summary, description);
			}

			foreach (var gt in toEnumerate)
			{
				var issues = gt.Select(SummaryAndDesc).ToArray();

				string summary;
				string description;

				if (issues.Length == 1)
				{
					summary = issues[0].Item1;
					description = issues[0].Item2;
				}
				else
				{
					summary = $"{Path.GetFileName(gt.Key)}: {gt.Count()} todos";

					description = string.Join($"{Environment.NewLine}{Environment.NewLine}",
						issues.Select(x => $"{x.Item1}:{Environment.NewLine}{x.Item2}"));
				}

				var i = reporter.CreateIssue(project, summary, description).Result;

				foreach (var c in issueCommands)
				{
					reporter.ExecuteAgainstIssue(i, c).Wait();
				}

				Console.WriteLine($"Created {i} ({summary})");			
			}
		}

		public void Dispose()
		{
			reporter?.Dispose();
		}
	}
}