using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Org.RssBandit.MSBuild
{
	/// <summary>
    /// Task gets the correct SVN repository path from a local windows path
    /// </summary>
    /// <example>
	/// <para>Generate correct SVN repository path from a local file path.</para>
    /// <code><![CDATA[
	/// <SvnRepositoryPath LocalPath="$(MSBuildProjectDirectory)\trunk" >
	///   <Output TaskParameter="SVNRepository" PropertyName="SVNRepositoryPath" />
	/// </SvnRepositoryPath>
    /// ]]></code>
	/// <para>Fill the property named 'SVNRepositoryPath' with a path like this:</para>
	/// <para>Input:  d:\svn\repo\Test\trunk</para>
	/// <para>Output: file:///d:/svn/repo/Test/trunk</para>
    public sealed class SvnRepositoryPath : Task
	{
		// Fields
		private string svnRepository = "";

		#region Input Parameters
		
		private string localPath;

		/// <summary>
		/// Gets or sets the local file path.
		/// </summary>
		/// <example>d:\svn\repo\Test\trunk</example>
		/// <value>The local file path.</value>
		[Required]
		public string LocalPath
		{
			get { return localPath; }
			set { localPath = value; }
		}

		#endregion

		/// <summary>
		/// When overridden in a derived class, executes the task.
		/// </summary>
		/// <returns>
		/// true if the task successfully executed; otherwise, false.
		/// </returns>
		public override bool Execute()
		{
			Uri uri;
			if (Uri.TryCreate(LocalPath, UriKind.Absolute, out uri))
			{
				this.svnRepository = uri.ToString();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the SVN repository file path.
		/// </summary>
		/// <example>file:///d:/svn/repo/Test/trunk</example>
		/// <value>The SVN repository path.</value>
		[Output]
		public string SVNRepository
		{
			get { return this.svnRepository; }
		}
	}


}
