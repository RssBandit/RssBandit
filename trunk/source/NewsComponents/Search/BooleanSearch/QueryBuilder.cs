using System;
using System.Text;

namespace NewsComponents.Search.BooleanSearch
{
	/// <summary>
	/// QueryBuilder provides facilities for parsing and validating a boolean
	/// search string and building a QueryTree, which can subsequently be matched
	/// against one or more objects implenting the IDocument interface.
	/// </summary>
	public class QueryBuilder
	{
		enum Tokens
		{
			OpenBracket,
			CloseBracket,
			OperatorAnd,
			OperatorOr,
			OperatorNot,
			Text,
			Whitespace
		};

		private string m_query;
		/* private string[] m_tokens; */

		public string Query
		{
			get
			{
				return m_query;
			}
			set
			{
				m_query = value;
			}
		}

		public QueryBuilder(string query)
		{
			m_query = query;
		}

		public bool Validate()
		{
			// Fail if no query has been specified
			if (m_query == null)
				return false;

			// Parse the query a token at a time
			int unclosed_brackets = 0;
			bool in_inverted_commas = false;
			Tokens last_token = Tokens.Whitespace;
			
			for (int i = 0; i < m_query.Length; i ++)
			{
				// Get next token to process
				char cur_char = m_query[i];
				
				if (cur_char == '"')
					in_inverted_commas ^= true;

				// Do not apply any syntax rules while within
				// a set of inverted commas
				if (in_inverted_commas)
					continue;

				switch(cur_char)
				{
					case '(':
						// Fail if last token was CloseBracket,
						// Text or OperatorNot (for simplicity,
						// "NOT" is not distributive in this 
						// implementation
						if (last_token == Tokens.CloseBracket
							|| last_token == Tokens.Text
							|| last_token == Tokens.OperatorNot)
								return false;
						
						last_token = Tokens.OpenBracket;
						unclosed_brackets ++;
						break;

					case ')':
						// Fail if last token was an operator
						if (last_token == Tokens.OperatorAnd ||
							last_token == Tokens.OperatorOr ||
							last_token == Tokens.OperatorNot)
								return false;
						
						last_token = Tokens.CloseBracket;
						unclosed_brackets --;
						break;

					case '&':
						// Fail if last token was an operator
						if (last_token == Tokens.OperatorAnd ||
							last_token == Tokens.OperatorOr ||
							last_token == Tokens.OperatorNot)
							return false;
						
						last_token = Tokens.OperatorAnd;
						break;

					case '|':
						// Fail if last token was an operator
						if (last_token == Tokens.OperatorAnd ||
							last_token == Tokens.OperatorOr ||
							last_token == Tokens.OperatorNot)
							return false;
						
						last_token = Tokens.OperatorOr;
						break;

					case '!':
						last_token = Tokens.OperatorNot;
						break;

					case ' ':
						last_token = Tokens.Whitespace;
						break;
					
					default:
						last_token = Tokens.Text;
						break;
				}
				
				// Fail if closing brackets precede opening ones
				if (unclosed_brackets < 0)
					return false;
			}

			// Fail if inverted commas weren't closed
			if (in_inverted_commas)
				return false;
			
			// Fail if brackets don't match
			if (unclosed_brackets != 0)
				return false;
		
			// Otherwise the query is valid
			return true;
		}
		
		public QueryTree BuildTree()
		{
			// Abort tree building if validation fails
			if (Validate() == false)
				return null;
			
			System.Text.StringBuilder word_token = new StringBuilder();
			
			// Build a tree rooted on tree_root; "node" holds the current
			// node currently undergoing processing
			QueryNode tree_root = new QueryNode();
			QueryNode node = tree_root.AddChild();
			bool in_inverted_commas = false;

			for (int i = 0; i < m_query.Length; i ++)
			{
				char cur_char = m_query[i];
				
				if (cur_char == '"')
					in_inverted_commas ^= true;

				// Do not apply any syntax rules while within
				// a set of inverted commas
				if ("()&|!".IndexOf(cur_char) != -1 && in_inverted_commas == false)
				{
					// Finish with our last word_token, if there was
					// one
					string token = word_token.ToString().Trim();
					
					// Assign the node a value
					if (token.Length > 0)
						node.Value = token;
					
					// Reset word_token
					word_token.Length = 0;

					if (cur_char == '(' || cur_char == '&')
					{
						// Extend the active branch a level deeper;
						// if we already have children, this means
						// inserting a new parent immediately above
						// us; otherwise, we just add a new child
						if (node.Children.Length > 0)
							node = node.InsertAbove();
						else
							node = node.AddChild();
					}
					else if (cur_char == ')')
					{
						// Backtrack up the active branch one level
						// above the last word(s) token
						while (node.Value.Length > 0)
							node = node.Parent;
					}
					else if (cur_char == '|')
					{
						// Add a sibling to the active branch
						node = node.Parent.AddChild();
					}
					else if (cur_char =='!')
					{
						// Invert the processing for this node only
						node.Inverted ^= true;
					}
				}
				else
				{
					// Otherwise it was either a character, inverted commas
					// or a space; treat inverted commas as spaces; 
					// this way spaces within multiple words will get preserved
					// when we trim later
					if (cur_char == '"')
						cur_char = ' ';

					word_token.Append(cur_char, 1);
				}
			}

			// Add the last token, if any
			string final_token = word_token.ToString().Trim();
			
			if (final_token.Length > 0)
				node.Value = final_token;

			// And return the final tree
			return new QueryTree(tree_root);
		}
	}
}
