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
    }
}