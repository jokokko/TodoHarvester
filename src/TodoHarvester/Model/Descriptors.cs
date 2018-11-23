using Microsoft.CodeAnalysis;
using TodoHarvester.Infrastructure;

namespace TodoHarvester.Model
{
    internal static class Descriptors
    {
        private static DiagnosticDescriptor Rule(string id, string title, RuleCategory category, DiagnosticSeverity defaultSeverity, string messageFormat, string description = null)
        {            
            return new DiagnosticDescriptor(id, title, messageFormat, category.Name, defaultSeverity, true, description);
        }
        	    
	    internal static readonly DiagnosticDescriptor TodoHarvesterTodoComment = Rule("TH1000", "Todo comment", RuleCategory.Usage, DiagnosticSeverity.Info, "Todo comment");
	}
}