#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace NewsComponents.Utils
{
	internal class CryptHelper {
		//FxCop CA1810
		// this algorithm is FIPS compliant:
		private static readonly TripleDESCryptoServiceProvider _des = new TripleDESCryptoServiceProvider();

		private CryptHelper(){}

		static CryptHelper() {
			_des.Key = _calcHash();
			_des.Mode = CipherMode.ECB;
			/* 
			Trace.Write("DES uses key: ", "CryptHelper");
			for (int i = 0; i < _des.Key.GetLength(0); i++)
				Trace.Write(_des.Key[i].ToString("x"));

			Trace.WriteLine(" Mode: "+((int)_des.Mode).ToString());
			*/
		}

		public static string Decrypt(string str) {
			byte[] base64;
		    string ret;

			if (str == null)
				ret = null;
			else {
				if (str.Length == 0)
					ret = String.Empty;
				else {
					try {
						base64 = Convert.FromBase64String(str);
						byte[] bytes = _des.CreateDecryptor().TransformFinalBlock(base64, 0, base64.GetLength(0));
						ret = Encoding.Unicode.GetString(bytes);
					}
					catch (Exception e) {
						Trace.WriteLine("Exception in Decrypt: "+e, "CryptHelper");
						ret = String.Empty;
					}
				}
			}
			return ret;
		}

		public static string Decrypt(byte[] bytes) {
		    string ret;

			if (bytes.GetLength(0) == 0)
				ret = String.Empty;
			else {
				try
				{
				    byte[] tmp = _des.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.GetLength(0));
				    ret = Encoding.Unicode.GetString(tmp);
				}
				catch (Exception e) {
					Trace.WriteLine("Exception in Decrypt: "+e, "CryptHelper");
					ret = String.Empty;
				}
			}
			return ret;
		}

		public static string Encrypt(string str) {
			byte[] inBytes;
		    string ret;

			if (str == null)
				ret = null;
			else {
				if (str.Length == 0)
					ret = String.Empty;
				else {
					try {
						inBytes = Encoding.Unicode.GetBytes(str);
						byte[] bytes = _des.CreateEncryptor().TransformFinalBlock(inBytes, 0, inBytes.GetLength(0));
						ret = Convert.ToBase64String(bytes);
					}
					catch (Exception e) {
						Trace.WriteLine("Exception in Encrypt: "+e, "CryptHelper");
						ret = String.Empty;
					}
				}
			}
			return ret;
		}

		public static byte[] EncryptB(string str) {
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
						ret = _des.CreateEncryptor().TransformFinalBlock(inBytes, 0, inBytes.GetLength(0));
					}
					catch (Exception e) {
						Trace.WriteLine("Exception in Encrypt: "+e, "CryptHelper");
						ret = null;
					}
				}
			}
			return ret;
		}

		public static string GenerateKey(int length)
		{
			RandomNumberGenerator generator = RandomNumberGenerator.Create();
			byte[] data = new byte[length];
			generator.GetNonZeroBytes(data);
			return Convert.ToBase64String(data);
		}
		
		public static uint GenerateIntKey()
		{
			RandomNumberGenerator generator = RandomNumberGenerator.Create();
			byte[] data = new byte[4];
			generator.GetNonZeroBytes(data);
			return BitConverter.ToUInt32(data, 0);
		}
		public static ushort GenerateShortKey()
		{
			RandomNumberGenerator generator = RandomNumberGenerator.Create();
			byte[] data = new byte[2];
			generator.GetNonZeroBytes(data);
			return BitConverter.ToUInt16(data, 0);
		}

		private static byte[] _calcHash() {
			string salt = "NewsComponents.4711";
			byte[] b = Encoding.Unicode.GetBytes(salt);
			int bLen = b.GetLength(0);
				
			// just to make the key somewhat "invisible" in Anakrino, we use the random class.
			// the seed (a prime number) makes it repro
			Random r = new Random(1500450271);	
			// result array
			byte[] res = new Byte[500];
			int i;
				
			for (i = 0; i < bLen && i < 500; i++)
				res[i] = (byte)(b[i] ^ r.Next(30, 127));
				
			// padding:
			while (i < 500) {
				res[i] = (byte)r.Next(30, 127);
				i++;
			}

			//TODO: this hash algorithm is NOT FIPS compliant: replace by a comliant impl.
			MD5CryptoServiceProvider csp = new MD5CryptoServiceProvider();
			return csp.ComputeHash(res);
		}

	}
}
