namespace TodoHarvester.Infrastructure
{
    internal static class Constants
    {
	    public static readonly string TodoRegexGroup = "TODO";
	    public static readonly string DefaultTodoPattern = "(?si)(?<=\\W|^)(TODO)(\\W|$)(?<TODO>.*)";

    }
}