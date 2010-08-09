using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;

namespace RssBandit.Services
{
    /// <summary>
    /// Internal configuration class
    /// </summary>
    internal class UpdateInfoSectionHandler : IConfigurationSectionHandler {
        /*
		 * <rssBandit.UpdateService.UpdateInfos>
		 *     <updateInfos appID="{GUID}"> 
		 *        <updateInfo 
		 *            appVersion="1.3.0.27"
		 *            downloadLink="http://..." />
		 *		</updateInfos>
		 * </rssBandit.UpdateService.UpdateInfos>
		 */
        object IConfigurationSectionHandler.Create(object parent, object context, XmlNode sectionRoot ) {
            Hashtable configData = new Hashtable();

            XmlNodeList ndNodeList = ((XmlElement)sectionRoot).GetElementsByTagName("updateInfos");
            foreach( XmlElement elUpdateInfos in ndNodeList  ) {
				
                string appID = elUpdateInfos.GetAttribute("appID");
                ListDictionary infos = new ListDictionary();

                foreach( XmlElement elUpdateInfo in elUpdateInfos.GetElementsByTagName("updateInfo")  ) {

                    try {
                        UpdateInfo info = new UpdateInfo();
                        info.Version = new Version(elUpdateInfo.GetAttribute("appVersion"));
                        info.DownloadUri = new Uri(elUpdateInfo.GetAttribute("downloadLink"));
                        infos.Add(info.Version, info);
                    } catch (Exception) {}
                }

                configData.Add( appID, infos );

            }

            return configData;
        }

    }
}