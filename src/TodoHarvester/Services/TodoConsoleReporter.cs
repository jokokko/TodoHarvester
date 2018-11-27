using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TodoHarvester.Model;
using Colorful;
using Console = Colorful.Console;

namespace TodoHarvester.Services
{
	public sealed class TodoConsoleReporter : ITodoReporter
	{		
		private readonly Action<IEnumerable<TodoComment>> reportImpl;

		public TodoConsoleReporter(bool groupByFile = false)
		{
			reportImpl = groupByFile ? (Action<IEnumerable<TodoComment>>)ReportImplGrouped : ReportImpl;
		}

		public void Report(IEnumerable<TodoComment> todos)
		{
			reportImpl(todos);
		}

		private void ReportImpl(IEnumerable<TodoComment> todos)
		{
			var todoComments = todos as TodoComment[] ?? todos.ToArray();

			Console.WriteLine($"Total issues: {todoComments.Length}");

			foreach (var t in todoComments)
			{
				var ctx = t.AssociatedWith.FirstOrDefault();
				if (ctx != null)
				{
					Console.WriteLineFormatted(@"{0}: {1} - {2} ({3})", Color.White, new Formatter(ctx.Kind, Color.LightGray),
						new Formatter(ctx.Name, Color.LightCyan), new Formatter(t.Comment, Color.LightCyan),
						new Formatter(Path.GetFileName(t.Location.SourceTree.FilePath), Color.LightGray));
				}
				else
				{
					Console.WriteLineFormatted(@"{0} ({1})", Color.White, new Formatter(t.Comment, Color.LightCyan),
						new Formatter(Path.GetFileName(t.Location.SourceTree.FilePath), Color.LightGray));
				}
			}
		}

		private void ReportImplGrouped(IEnumerable<TodoComment> todos)
		{
			var todoComments = todos as TodoComment[] ?? todos.ToArray();

			var toEnumerate = todoComments.GroupBy(x => x.Location.SourceTree.FilePath).ToArray();

			Console.WriteLine($"Total issues: {todoComments.Length}, Total files: {toEnumerate.Length}{Environment.NewLine}");

			foreach (var gt in toEnumerate)
			{
				Console.WriteLineFormatted(@"{0}", Color.White, new Formatter(gt.Key, Color.LightGray));
				foreach (var t in gt)
				{
					var ctx = t.AssociatedWith.FirstOrDefault();
					if (ctx != null)
					{
						Console.WriteLineFormatted(@"	{0}: {1} - {2}", Color.White,
							new Formatter(ctx.Kind, Color.LightGray),
							new Formatter(ctx.Name, Color.LightCyan), new Formatter(t.Comment, Color.LightCyan));
					}
					else
					{
						Console.WriteLineFormatted(@"	{0}", Color.White, new Formatter(t.Comment, Color.LightCyan));
					}
				}
			}
		}
	}
}