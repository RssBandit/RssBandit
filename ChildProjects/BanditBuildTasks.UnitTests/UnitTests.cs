using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using Org.RssBandit.MSBuild;

namespace BanditBuildTasks.UnitTests
{
	/// <summary>
	/// Summary description for TaskUnitTest
	/// </summary>
	[TestFixture]
	public class TaskUnitTest
	{
		private string testDirectory;

		[TestFixtureSetUp]
		public void FixtureInit()
		{
			MockBuild buildEngine = new MockBuild();
			testDirectory = TaskUtility.makeTestDirectory(buildEngine);
		}

		[Test(Description = "Test SVN Repository Path Task")]
		public void TestSVNRepositoryTask()
		{
			string repoPath = @"d:\svn\repo\Test with spaces\trunk";

			SvnRepositoryPath repositoryPath = new SvnRepositoryPath();
			repositoryPath.BuildEngine = new MockBuild();

			Assert.IsNotNull(repositoryPath);

			repositoryPath.LocalPath = repoPath;
			bool result = repositoryPath.Execute();

			Assert.IsTrue(result);
			Assert.IsFalse(String.IsNullOrEmpty(repositoryPath.SVNRepository ));

			Console.WriteLine("Converted path '{0}' to '{1}'", repoPath, repositoryPath.SVNRepository);
		}
	}
}
