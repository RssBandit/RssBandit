using System;
using System.IO;
using System.Security;
using System.Xml.XPath;
using Syndication.Extensibility;
using System.Windows.Forms;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Net; 

namespace BlogExtension.Delicious 
{
	public class DeliciousPlugin : IBlogExtension
	{		
		string configFile;	
		delicious configInfo; 
		TripleDESCryptoServiceProvider cryptoProvider; 
		XmlSerializer serializer; 

		public DeliciousPlugin()
		{
			//setup path to config file
			string assemblyUri = this.GetType().Assembly.CodeBase;
			string assemblyPath = new Uri(assemblyUri).LocalPath;
			string assemblyDir = Path.GetDirectoryName(assemblyPath);
			configFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"RssBandit\delicious.xml");
			
			// This is a one-time operation used to move the users old 
			// delicious file to the new location.
			try
			{
				if(!File.Exists(configFile) && File.Exists(Path.Combine(assemblyDir, "delicious.xml")))
				{
					string oldConfigPath = Path.Combine(assemblyDir, "delicious.xml");
					File.Copy(oldConfigPath, configFile, false);
					
					//This might throw an exception since the user is possibly an LUA.
					File.Delete(oldConfigPath);
				}
			}
			catch(IOException)
			{
				//Swallow this.
			}
			catch(SecurityException)
			{
				//Swallow this.
			}
		
			//set up crypto provider for encrypting password in config file
			cryptoProvider = new TripleDESCryptoServiceProvider();
			cryptoProvider.Key = CalculateHash();
			cryptoProvider.Mode = CipherMode.ECB;

			//setup XML serializer for config file
			serializer = new XmlSerializer(typeof(delicious));		
		}

		public string DisplayName { get { return Resource.Manager["RES_MenuDeliciousCaption"]; } }

		public bool HasConfiguration { get { return true; } }

		public bool HasEditingGUI { get { return true; } }

		public void Configure(IWin32Window parent)
		{
			this.LoadConfig(); 

			using (DeliciousPluginConfigurationForm configForm = new DeliciousPluginConfigurationForm(configInfo.username, this.Decrypt(configInfo.password), configInfo.apiurl))
			{
				if (configForm.ShowDialog(parent) == DialogResult.OK)
				{					
					configInfo.apiurl        = configForm.textUri.Text;
					configInfo.username      = configForm.textUser.Text;
					configInfo.password      = this.Encrypt(configForm.textPwd.Text);

					XmlTextWriter writer     = new XmlTextWriter(configFile, Encoding.UTF8); 
					serializer.Serialize(writer, configInfo); 
				}
			}
		}


		public void BlogItem(IXPathNavigable rssFragment, bool edited) {

			HttpWebRequest request = null;
			HttpWebResponse response = null;

			try{ 

				this.LoadConfig(); 				

				string tags, description = rssFragment.CreateNavigator().Evaluate("string(//item/title/text())").ToString(), 
				  url = rssFragment.CreateNavigator().Evaluate("string(//item/link/text())").ToString();


				using (DeliciousPostForm postForm = new DeliciousPostForm(url, description)) {
					if (postForm.ShowDialog() == DialogResult.OK) {					
						tags = postForm.textTags.Text; 
						url = postForm.textUri.Text;
						description = postForm.textDescription.Text;
					
						Uri postUrl = new Uri(configInfo.apiurl + "?url=" + url + "&tags=" + tags + "&description=" + description); 
						request = (HttpWebRequest) WebRequest.Create(postUrl);
						request.UserAgent			= "del.icio.usIBlogExtensionPlugin/1.0"; 
						
						NetworkCredential nc = new NetworkCredential(configInfo.username, this.Decrypt(configInfo.password)); 						
						request.Credentials = nc; 
	
						response = (HttpWebResponse) request.GetResponse(); 
						
						if(response.StatusCode != HttpStatusCode.OK){
							throw new Exception(new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd()); 
						}
						
					}//if (postForm.ShowDialog() == DialogResult.OK) {	
				}//using 

			}catch(Exception e){
				MessageBox.Show(e.Message, e.GetType().Name,MessageBoxButtons.OK, MessageBoxIcon.Error);
			}finally{
			
				if(response != null){ 
					response.Close(); 
				}
			}
			
		}

		private void LoadConfig(){
			
			if(configInfo == null){

				if(File.Exists(configFile)){

					XmlTextReader reader = new XmlTextReader(configFile);
					configInfo = (delicious) serializer.Deserialize(reader); 					
					reader.Close();
			
				}else{
					configInfo = new delicious(); 
					configInfo.apiurl = "https://api.del.icio.us/v1/posts/add"; 
				}
			}
		}
		
		private static byte[] CalculateHash() {
			string salt = "DeliciousPlugin.4711";
			byte[] b = Encoding.Unicode.GetBytes(salt);
			int bLen = b.GetLength(0);
				
			// just to make the key somewhat "invisible" in Anakrino, we use the random class.
			// the seed (a prime number) makes it repro
			Random r = new Random(1500450271);	
			// result array
			byte[] res = new Byte[500];
			int i = 0;
				
			for (i = 0; i < bLen && i < 500; i++)
				res[i] = (byte)(b[i] ^ r.Next(30, 127));
				
			// padding:
			while (i < 500) {
				res[i] = (byte)r.Next(30, 127);
				i++;
			}

			MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider();
			return csp.ComputeHash(res);
		}
	
		private byte[] Encrypt(string str) {
			byte[] inBytes;
			byte[] ret;

			if (str == null)
				ret = null;
			else {
				if (str.Length == 0)
					ret = null;
				else {
					try {
						inBytes = Encoding.Unicode.GetBytes(str);
						ret = cryptoProvider.CreateEncryptor().TransformFinalBlock(inBytes, 0, inBytes.GetLength(0));
					}
					catch (Exception e) {
						MessageBox.Show("Exception in Encrypt: "+e.ToString(), "CryptHelper");
						ret = null;
					}
				}
			}
			return ret;
		}

		private string Decrypt(byte[] bytes) {
			byte[] tmp;
			string ret;

			if ((bytes == null) || (bytes.GetLength(0) == 0))
				ret = String.Empty;
			else {
				try {
					tmp = cryptoProvider.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.GetLength(0));
					ret = Encoding.Unicode.GetString(tmp);
				}
				catch (Exception e) {
					MessageBox.Show("Exception in Decrypt: "+e.ToString(), "CryptHelper");
					ret = String.Empty;
				}
			}
			return ret;
		}			
	
	}


	/// <remarks/>
	[System.Xml.Serialization.XmlRootAttribute("del.icio.us", Namespace="", IsNullable=false)]
	public class delicious {
    
		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("api-url")]
		public string apiurl;
    
		/// <remarks/>
		public string username;
    
		/// <remarks/>
		public byte[] password;
	}

}
