using Oakton;
using TodoHarvester.Model;
using TodoHarvester.Services;

namespace TodoHarvester.Commands
{
	[Description("Search todo comments & create issues of them in YouTrack", Name = "report-todos-youtrack")]
	// ReSharper disable once UnusedMember.Global
	public sealed class ReportTodosYouTrack : ReportTodos<YouTrackInput>
	{
		protected override ITodoReporter ReporterFrom(YouTrackInput input)
		{
			var reporter = new TodoYouTrackReporter(input.YouTrackUriFlag, input.YouTrackUserFlag, input.YouTrackPasswordFlag, input.YouTrackProjectFlag);			
			return reporter;
		}
	}
}