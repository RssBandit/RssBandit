using System;
using System.Collections;

namespace NewsComponents.Search.BooleanSearch
{
	/// <summary>
	/// A QueryTree manages a tree QueryNode objects, providing facilities
	/// to match a boolean search against one or more objects that implement
	/// IDocument, and return a list of positive matches.
	/// </summary>
	public class QueryTree
	{
		private QueryNode m_root;
	
		public QueryTree(QueryNode root)
		{
			m_root = root;
		}

		private void RecurseTree(QueryNode node, ref string text, int level)
		{
			text += new string(' ', level * 3);
			
			if (node.Inverted)
				text += "[NOT]";

			text += node.Value;
			text += "\n";
		
			for (int i = 0; i < node.Children.Length; i ++)
			{
				level ++;
				RecurseTree(node.Children[i], ref text, level);
				level --;
			}
		}

		public override string ToString()
		{
			string text = "Tree:\n\n";
			
			RecurseTree(m_root, ref text, 1);

			return text;
		}

		public bool IsMatch(IDocument doc)
		{
			// Start at the top of the tree, and traverse through the
			// nodes, backtracking each time we fail to satisfy a
			// constraint. If we end up at a leaf, the search conditions
			// were satisfied). If we run out of nodes, we've failed
			// to find a match
			Stack stack = new Stack();
			stack.Push(m_root);
			
			while (stack.Count > 0)
			{
				QueryNode node = (QueryNode) stack.Pop();
							
				// If the node doesn't have a value, add its
				// children to the stack, and repeat
				if (node.Value.Length == 0)
				{
					foreach(QueryNode child in node.Children)
						stack.Push(child);
				
					continue;
				}

				// If we can't match the search term, backtrack
				if (doc.Find(node.Value) ^ node.Inverted == false)
					continue;
			
				// Otherwise we matched the search term at this node; was
				// it a leaf node?
				if (node.Children.Length == 0)
				{
					// Yes, so we've finished
					return true;
				}

				// Otherwise continue to descend this branch
				foreach(QueryNode child in node.Children)
					stack.Push(child);
			}
		
			return false;
		}

		public IDocument[] GetMatches(IDocument[] candidates)
		{
			ArrayList documents = new ArrayList();
		
			foreach(IDocument candidate in candidates)
			{
				if (IsMatch(candidate))
					documents.Add(candidate);
			}
		
			return (IDocument[]) documents.ToArray(typeof(IDocument));
		}
	}

}
