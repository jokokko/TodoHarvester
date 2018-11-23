using System.Collections.Generic;
using TodoHarvester.Model;

namespace TodoHarvester.Services
{
	public interface ITodoReporter
	{
		void Report(IEnumerable<TodoComment> todos);
	}
}