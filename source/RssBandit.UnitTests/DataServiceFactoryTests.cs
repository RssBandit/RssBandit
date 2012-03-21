#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Linq;
using NUnit.Framework;
using NewsComponents;
using NewsComponents.Storage;

namespace RssBandit.UnitTests
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
        /// Tests that the Constructors throws IO exception if cache directory 
        /// does not exist.
        /// </summary>
        [Test]
        public void ConstructorThrowsIoExceptionIfCacheBaseDirectoryDoesNotExist()
        {
            var service = DataServiceFactory.GetService(StorageDomain.UserCacheData, new NewsComponentsConfiguration());
            Assert.IsNotNull(service, "No data service was created");
        }
    }
}
