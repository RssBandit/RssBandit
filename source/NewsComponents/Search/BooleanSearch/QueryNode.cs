using System;
using System.Collections;

namespace NewsComponents.Search.BooleanSearch
{
	/// <summary>
	/// A QueryNode is the fundamental element used to represent data within
	/// a QueryTree. Each QueryNode has one immediate parent and zero or more 
	/// children. QueryNode objects can be inserted at any level within the
	/// tree, and the tree itself can be navigated using the Parent and
	/// Children properties
	/// </summary>
	public class QueryNode
	{
		private ArrayList m_children = new ArrayList();
		private QueryNode m_parent;
		private string m_data = "";
		private bool m_inverted = false;

		public string Value
		{
			get
			{
				return m_data;
			}
			set
			{
				m_data = value;
			}
		}

		public QueryNode Parent
		{
			get
			{
				return m_parent;
			}
		}

		public QueryNode[] Children
		{
			get
			{
				return (QueryNode[]) m_children.ToArray(this.GetType());
			}
		}

		public bool Inverted
		{
			get
			{
				return m_inverted;
			}
			set
			{
				m_inverted = value;
			}
		}

		public QueryNode AddChild()
		{
			QueryNode child = new QueryNode();
			m_children.Add(child);	
			child.m_parent = this;
		
			return child;
		}

		public QueryNode InsertAbove()
		{
			// Insert a node immediately above the current node,
			// and return a reference to it

			// Locate the child in the parent's collection
			// of children (must exist)
			for(int i = 0; i < m_parent.m_children.Count; i ++)
			{
				if (m_parent.m_children[i] == this)
				{
					QueryNode new_node = new QueryNode();
					new_node.m_parent = m_parent;
					m_parent.m_children[i] = new_node;
					
					new_node.m_children.Add(this);
					m_parent = new_node;
					
					return new_node;
				}
			}
		
			return null;
		}
	}
}
