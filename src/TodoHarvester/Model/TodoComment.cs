using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace TodoHarvester.Model
{
	public struct TodoComment
	{
		public TodoComment(string comment, Location location, IEnumerable<ISymbol> associatedWith)
		{
			Comment = comment;
			Location = location;
			AssociatedWith = associatedWith;
		}

		public string Comment { get; }
		public Location Location { get; }
		public IEnumerable<ISymbol> AssociatedWith { get; }
	}
}