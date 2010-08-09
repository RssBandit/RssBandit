using System;

namespace RssBandit.Services
{
	/// <summary>
	/// Summary description for UpdateInfo.
	/// </summary>
	internal class UpdateInfo
	{
		public UpdateInfo():this(new Version("0.0.0.0"), null){}
		public UpdateInfo(Version version):this(version, null){}
		public UpdateInfo(Version version, Uri downloadUri)	{
			this.Version = version;
			this.DownloadUri = downloadUri;
		}

		public Version Version;
		public Uri DownloadUri;
	}


}
