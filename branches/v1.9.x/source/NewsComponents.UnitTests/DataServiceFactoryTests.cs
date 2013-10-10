#region Version Info Header
/*
 * $Id: DataServiceFactoryTests.cs 1098 2012-03-24 10:15:52Z t_rendelmann $
 * $HeadURL: https://rssbandit.svn.sourceforge.net/svnroot/rssbandit/trunk/source/RssBandit.UnitTests/DataServiceFactoryTests.cs $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2012-03-24 11:15:52 +0100 (Sa, 24 Mrz 2012) $
 * $Revision: 1098 $
 */
#endregion

using System;
using NewsComponents.Storage;
using NUnit.Framework;

namespace NewsComponents.UnitTests
{
    /// <summary>
    /// Data service factory tests
    /// </summary>
    [TestFixture]
    public class DataServiceFactoryTests
    {

        /// <summary>
        /// Test the factory method to create a service, that should throw argument null exception if configuration is missing.
        /// </summary>
        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorThrowsArgumentNullExceptionIfConfigurationIsMissing()
		{
            DataServiceFactory.GetService(StorageDomain.UserCacheData, null);
		}

        /// <summary>
        /// Tests that the Constructors does not throw IO exception if cache directory 
        /// does not exist.
        /// </summary>
		[Test]
        public void ConstructorThrowsIoExceptionIfCacheBaseDirectoryDoesNotExist()
        {
            DataServiceFactory.GetService(StorageDomain.UserCacheData, new NewsComponentsConfiguration());
        }
    }
}
