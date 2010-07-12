using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;

namespace Microsoft.Office.OneNote
{
	/// <exclude/>
	/// <summary>
	/// Abstract base class which all "nodes" (pages, pageobjects, data etc.) in the import "tree" extend for common 
	/// functionality. This provides common functionality for serialization and should not need to be directly accessed.
	/// </summary>
	[Serializable]
	public abstract class ImportNode : ICloneable
	{
		/// <summary>
		/// Default constructor for a new node in the import tree.
		/// </summary>
		protected ImportNode()
		{
			CommitPending = true;
		}

		/// <summary>
		/// All nodes that can be imported into OneNote need to be able to 
		/// serialize themselves into their appropriate XML representation.
		/// </summary>
		/// <param name="parentNode">
		/// A reference to the parent XML node that we should serialize ourselves
		/// as a child of.
		/// </param>
		protected internal abstract void SerializeToXml(XmlNode parentNode);

		/// <summary>
		/// ImportNodes need to be cloneable so that implicit memory management
		/// can occurr when adding a node to the import tree.
		/// </summary>
		/// <returns></returns>
		public abstract object Clone();

		/// <summary>
		/// Adds the specified node as a child of this node in the import tree.
		/// </summary>
		/// <param name="child">The child node.</param>
		protected internal void AddChild(ImportNode child)
		{
			AddChild(child, null);
		}

		/// <summary>
		/// Adds the specified node as a child of this node in the import tree
		/// with a given name.  The child can subsequently be retrieved 
		/// via this name.
		/// </summary>
		/// <param name="child">The child node.</param>
		/// <param name="childName">The name of the child.</param>
		protected internal ImportNode AddChild(ImportNode child, String childName)
		{
			Debug.Assert(!children.Contains(child));

			if (child.Parent != null && child.Parent != this)
			{
				child = (ImportNode) child.Clone();
			}

			children.Add(child);
			child.Parent = this;
			child.Name = childName;

			// Update our dirtiness:
			child.CommitPending = true;

			return child;
		}

		/// <summary>
		/// Counts the number of child nodes.
		/// </summary>
		protected internal int GetChildCount()
		{
			return children.Count;
		}

		/// <summary>
		/// Retrieves the child as the specified index.
		/// </summary>
		/// <param name="index">The 0-based index of the child to be return.</param>
		/// <returns>The ImportNode at the specified index.</returns>
		protected internal ImportNode GetChild(int index)
		{
			return (ImportNode) children[index];
		}

		/// <summary>
		/// Retrieves the child with the given name.
		/// </summary>
		/// <param name="childName">The name of the child.</param>
		/// <returns>The first ImportNode found with the given name, or null if none are found.</returns>
		protected internal ImportNode GetChild(string childName)
		{
			foreach (ImportNode node in children)
			{
				if (childName.Equals(node.Name))
				{
					return node;
				}
			}

			return null;
		}

		/// <summary>
		/// Removes the specified child from the import tree.
		/// </summary>
		/// <param name="child">The ImportNode to be removed.</param>
		protected internal void RemoveChild(ImportNode child)
		{
			Debug.Assert(child.Parent == this);
			Debug.Assert(children.Contains(child));

			children.Remove(child);
			child.Parent = null;
			child.Name = null;

			// Update our dirtiness:
			CommitPending = true;
		}

		/// <summary>
		/// The name of the ImportNode, if it has one.  By default,
		/// ImportNodes are unnamed when constructed and "tagged" with a name
		/// when being added as a child of another node.
		/// </summary>
		protected internal string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		/// <summary>
		/// A reference to our parent ImportNode in import tree.
		/// </summary>
		protected internal ImportNode Parent
		{
			get
			{
				return parent;
			}
			set
			{
				parent = value;
			}
		}

		/// <summary>
		/// The containing page of this import node, if our subtree has been
		/// added to a <see cref="Page"/>.  Otherwise null.
		/// </summary>
		protected internal Page ContainingPage
		{
			get
			{
				ImportNode node = this;

				do
				{
					// Are we at a Page?
					Page containingPage = node as Page;
					if (containingPage != null)
					{
						return containingPage;
					}

					// Iterate:
					node = node.Parent;
				} while (node != null);

				return null;
			}
		}

		/// <summary>
		/// By definition, setting commitPending to true invalidates all parent
		/// nodes as well, and setting commitPending to false validates all
		/// child nodes.
		/// </summary>
		protected internal bool CommitPending
		{
			get
			{
				return commitPending;
			}
			set
			{
				commitPending = value;

				if (commitPending)
				{
					if (parent != null)
						parent.CommitPending = true;
				}
				else
				{
					for (int i = 0; i < children.Count; i++)
					{
						ImportNode child = (ImportNode) children[i];
						child.CommitPending = false;
					}
				}
			}
		}

		private string name;
		private ImportNode parent;
		private ArrayList children = new ArrayList();
		private bool commitPending;
	}
}