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
		public void Report(IEnumerable<TodoComment> todos)
		{
			foreach (var t in todos)
			{
				var ctx = t.AssociatedWith.FirstOrDefault();				
				if (ctx != null)
				{					
					Console.WriteLineFormatted(@"{0}: {1} - {2} ({3})", Color.White, new Formatter(ctx.Kind, Color.LightGray), new Formatter(ctx.Name, Color.LightCyan), new Formatter(t.Comment, Color.LightCyan), new Formatter(Path.GetFileName(t.Location.SourceTree.FilePath), Color.LightGray));
				}
				else
				{
					Console.WriteLineFormatted(@"{0} ({1})", Color.White, new Formatter(t.Comment, Color.LightCyan), new Formatter(Path.GetFileName(t.Location.SourceTree.FilePath), Color.LightGray));
				}
			}
		}
	}
}