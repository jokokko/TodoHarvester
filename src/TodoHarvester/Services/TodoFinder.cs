﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using TodoHarvester.Analyzers;

namespace TodoHarvester.Services
{
	public sealed class TodoFinder
	{	  
		public async Task FindAndReportTodos(IEnumerable<string> solutions, IEnumerable<Regex> todoRegexes,
			ITodoReporter reporter, bool keepWhitespaceFlag, Dictionary<string, string> solutionProperties = null, string projectRegex = null)
		{
			if (solutions == null)
			{
				throw new ArgumentNullException(nameof(solutions));
			}

			if (todoRegexes == null)
			{
				throw new ArgumentNullException(nameof(todoRegexes));
			}

			if (reporter == null)
			{
				throw new ArgumentNullException(nameof(reporter));
			}

			var solutionsToAnalyze = solutions as string[] ?? solutions.ToArray();

			if (!solutionsToAnalyze.Any())
			{
				return;
			}		

			// ReSharper disable once ArgumentsStyleNamedExpression
			var collector = new TodoCommentAnalyzer(todoRegexes, normalizeWhitespaceNewlines: !keepWhitespaceFlag);

			await Task.WhenAll(solutionsToAnalyze.Select(x => AnalyzeSolution(x, collector, solutionProperties, projectRegex)).ToArray())
				.ConfigureAwait(false);
			
			reporter.Report(collector.GetTodoComments());	
		}

		private static async Task AnalyzeSolution(string solutionPath, DiagnosticAnalyzer analyzer,
			Dictionary<string, string> workspaceProperties = null, string projectRegex = null)
		{						
			MSBuildLocator.RegisterDefaults();
			
			var solution = await MSBuildWorkspace.Create(workspaceProperties ?? new Dictionary<string, string>()).OpenSolutionAsync(solutionPath).ConfigureAwait(false);
			var analyzers = ImmutableArray.Create(analyzer);

			Regex matchProject = null;

			if (projectRegex != null)
			{
				matchProject = new Regex(projectRegex);
			}

			bool MatchProject(string projectName)
			{				
				return matchProject == null || matchProject.IsMatch(projectName);
			}

			foreach (var s in solution.Projects.Where(x => MatchProject(x.Name)))
			{								
				var compilation = await s.GetCompilationAsync().ConfigureAwait(false);				
				await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);				
			}
		}
	}
}