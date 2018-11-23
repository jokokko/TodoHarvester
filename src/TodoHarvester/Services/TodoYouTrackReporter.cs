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
		private readonly YouTrackReporter reporter;

		public TodoYouTrackReporter(string youTrackUri, string username, string password, string project)
		{
			reporter = new YouTrackReporter(username, password, new Uri(youTrackUri), true);
			this.project = project;
		}

		public void Report(IEnumerable<TodoComment> todos)
		{
			foreach (var t in todos)
			{				
				string summary;
				string description;

				var ctx = t.AssociatedWith.FirstOrDefault();

				if (ctx != null)
				{
					summary = ($"{ctx.Kind}: {ctx.Name} - {t.Comment} ({Path.GetFileName(t.Location.SourceTree.FilePath)})");
					description = $"{t.Location}{Environment.NewLine}{Environment.NewLine}\t{string.Join($"{Environment.NewLine}\t", t.AssociatedWith.Select(x => $"{x.Kind}: {x.Name}"))}";
				}
				else
				{
					summary = ($"{t.Comment} ({Path.GetFileName(t.Location.SourceTree.FilePath)})");
					description = t.Location.ToString();
				}
				
				var i = reporter.CreateIssue(project, summary, description).Result;
				Console.WriteLine($"Created {i} ({summary})");
			}
		}

		public void Dispose()
		{
			reporter?.Dispose();
		}
	}
}