using System.Collections.Generic;

namespace TodoHarvester.Model
{
    public class TodoInput
    {
		[Oakton.Description("Solutions to analyze")]
	    public IEnumerable<string> Solutions;
	    [Oakton.Description("Target framework")]
		public string TargetFrameworkFlag;
	    [Oakton.Description("TODO regex patterns. If omitted, default pattern is used: (?si)(?<=\\W|^)(TODO)(\\W|$)(?<TODO>.*)")]
	    [Oakton.FlagAlias("tr", true)]
		public IEnumerable<string> TodoRegexesFlag;
		[Oakton.Description("Don't normalize whitespace and newlines")]
		[Oakton.FlagAlias("w", true)]
		public bool KeepWhitespaceFlag;
	    [Oakton.Description("Project regex. Only compile projects with name matching the regex")]
	    [Oakton.FlagAlias("pr", true)]
		public string ProjectRegexFlag;
	    [Oakton.FlagAlias("g", true)]
	    [Oakton.Description("Group issues per file.")]
		public bool GroupIssuesByFileFlag;
	}
}