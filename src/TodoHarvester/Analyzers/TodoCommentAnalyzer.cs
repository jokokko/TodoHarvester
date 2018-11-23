using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using TodoHarvester.Infrastructure;
using TodoHarvester.Model;

namespace TodoHarvester.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class TodoCommentAnalyzer : DiagnosticAnalyzer
	{		
		private readonly Regex[] todoRegexes;
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.TodoHarvesterTodoComment);

		private readonly Func<string, string> normalizeComment;

		public TodoCommentAnalyzer(IEnumerable<Regex> todoRegexes, bool normalizeWhitespaceNewlines = true)
		{
			// ReSharper disable once ConstantNullCoalescingCondition
			this.todoRegexes = todoRegexes.ToArray() ?? throw new ArgumentNullException(nameof(todoRegexes));

			if (normalizeWhitespaceNewlines)
			{
				var r = new Regex("\\s+", RegexOptions.Compiled);
				normalizeComment = s => r.Replace(s, " ").Trim();
			}
			else
			{
				normalizeComment = s => s;
			}			
		}

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(c =>
			{								
				c.RegisterSyntaxTreeAction(n => HandleSyntaxTree(n, c.Compilation));
			});			
		}

		private void HandleSyntaxTree(SyntaxTreeAnalysisContext context, Compilation compilation)
		{
			var root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
			
			foreach (var node in root.DescendantTrivia(descendIntoTrivia: true))
			{
				switch (node.Kind())
				{
					case SyntaxKind.SingleLineCommentTrivia:
						HandleSingleLineComment(node, compilation);
						break;
					case SyntaxKind.MultiLineCommentTrivia:
						HandleMultiLineComment(node, compilation);
						break;
				}
			}
		}

		private void HandleMultiLineComment(SyntaxTrivia multiLineComment, Compilation compilation)
		{
			var nodeText = multiLineComment.ToString();
			
			var comment = nodeText.Substring(2, Math.Max(0, nodeText.Length - 4));

			if (string.IsNullOrWhiteSpace(comment))
			{
				return;
			}
			
			ProcessMatches(comment, multiLineComment, compilation);
		}

		private void HandleSingleLineComment(SyntaxTrivia singleLineComment, Compilation compilation)
		{
			var comment = singleLineComment.ToString().Substring(2);

			if (string.IsNullOrWhiteSpace(comment))
			{
				return;
			}

			ProcessMatches(comment, singleLineComment, compilation);
		}

		private void ProcessMatches(string comment, SyntaxTrivia commentTrivia, Compilation compilation)
		{
			var matches = todoRegexes.SelectMany(x => x.Matches(comment).OfType<Match>()).ToArray();			

			// ReSharper disable once ImplicitlyCapturedClosure
			ISymbol GetAssociatedSymbol<T>(Predicate<T> filter = null) where T : SyntaxNode
			{
				var value = TryGetContainingNode(commentTrivia.Token.Parent, filter);

				// ReSharper disable once InvertIf
				if (value != null)
				{
					var sm = compilation.GetSemanticModel(value.SyntaxTree);
					var sym = sm?.GetDeclaredSymbol(value);

					return sym;
				}

				return null;
			}

			var associatedWith = new List<ISymbol>()
				.AddIfNotNull(GetAssociatedSymbol<MemberDeclarationSyntax>(n =>
					!(n is NamespaceDeclarationSyntax) && !(n is TypeDeclarationSyntax)))
				.AddIfNotNull(GetAssociatedSymbol<TypeDeclarationSyntax>())
				.AddIfNotNull(GetAssociatedSymbol<NamespaceDeclarationSyntax>());

			var todos = matches.Where(x => x.Groups[Constants.TodoRegexGroup].Success).Select(x => new TodoComment(normalizeComment(x.Groups[Constants.TodoRegexGroup].Value), commentTrivia.GetLocation(), associatedWith));

			foreach (var todoComment in todos)
			{
				todoComments.Add(todoComment);
			}
		}

		private static T TryGetContainingNode<T>(SyntaxNode node, Predicate<T> filter = null)
			where T : SyntaxNode
		{			
			var currentNode = node;

			while (true)
			{
				if (currentNode is T nodeOfType)
				{
					if (filter == null || filter(nodeOfType))
					{
						return nodeOfType;
					}
				}

				if (currentNode.Parent == null)
				{
					break;
				}

				currentNode = currentNode.Parent;
			}
			return null;
		}

		private readonly ConcurrentBag<TodoComment> todoComments = new ConcurrentBag<TodoComment>();

		public IEnumerable<TodoComment> GetTodoComments()
		{
			return todoComments;
		}
	}
}