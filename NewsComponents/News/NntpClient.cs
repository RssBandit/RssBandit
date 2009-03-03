using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using NewsComponents.Net;
using NewsComponents.Resources;
using NewsComponents.Utils;

namespace NewsComponents.News {

/// <summary>
/// Represents an NNTP message. 
/// </summary>
	internal class NntpMessage{		

		/// <summary>
		/// The NNTP headers
		/// </summary>
		private readonly NameValueCollection headers;
			
		/// <summary>
		/// The message Id
		/// </summary>
		private string id;      
            
		/// <summary>
		/// The message body
		/// </summary>
		private string body;     

		/// <summary>
		/// Default constructor. 
		/// </summary>
		internal NntpMessage(){
			headers = new NameValueCollection();
			id = body = String.Empty;
		}

		/// <summary>
		/// The message headers
		/// </summary>
		internal NameValueCollection Headers{
			get{
				return headers;
			}
		}

		/// <summary>
		/// The message ID
		/// </summary>
		internal string Id{
			get{
				return id;
			}
		}
		
		/// <summary>
		/// The message body. 
		/// </summary>
		internal string Body{
			get{
				return body;
			}
		}

		/// <summary>
		/// create one body string from the lines returned over the socket
		/// </summary>
		/// <param name="sLines">The body response to parse</param>
		internal void SetBody(string[] sLines){

			string sStat = sLines[0];
			sStat = sStat.Substring(sStat.IndexOf(" ") + 1);
			sStat = sStat.Substring(sStat.IndexOf(" ") + 1);
			sStat = sStat.Substring(sStat.IndexOf(" ") + 1);
			id = sStat;

			StringBuilder sBody = new StringBuilder();

			for(int i = 1; i < sLines.Length - 2; i++) {
				sBody.Append(sLines[i]);
				sBody.Append(Environment.NewLine);
			}
			body = sBody.ToString();
		}

		/// <summary>
		/// Take the headers response, parse it and add it to the headers collection
		/// </summary>
		/// <param name="sLines">The headers response to parse</param>
		internal void SetHeaders(string[] sLines) {
			string sStat;
			string sTmp;

			sStat = sLines[0];
			sStat = sStat.Substring(sStat.IndexOf(" ") + 1);
			sStat = sStat.Substring(sStat.IndexOf(" ") + 1);
			sStat = sStat.Substring(sStat.IndexOf(" ") + 1);
			id = sStat;

			for(int i = 1; i < sLines.Length - 2; i++) {
				sTmp = sLines[i];
				if(sTmp == ".") {
					break;
				}
				int colonPos = sTmp.IndexOf(":");
				if(colonPos > 0) {
					headers.Add(sTmp.Substring(0, colonPos).Trim(),
						sTmp.Substring(colonPos + 1).Trim());
				}
			}
		}

		/// <summary>
		/// Set a particular header. 
		/// </summary>
		/// <param name="pseudoId"></param>
		/// <param name="header"></param>
		/// <param name="value"></param>
		internal void SetXHeader(string pseudoId, string header, string value) {
			id = pseudoId;
			headers.Add(header, value);
		}
	}

	/// <summary>
	///  A collection of NntpMessage objects
	/// </summary>
	internal class NntpMessages
		: ReadOnlyCollectionBase{
            
		internal NntpMessages(){;}

		internal void Add(NntpMessage oMsg){
			InnerList.Add(oMsg);
		}
	}

	/// <summary>
	/// NNTP specific exception class
	/// </summary>
	[Serializable, ComVisible(false)]
	public class NntpWebException: WebException
	{
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NntpWebException"/> class.
		/// </summary>
		public NntpWebException()
		{}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NntpWebException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		public NntpWebException(string message): base(message) {}
		/// <summary>
		/// Initializes a new instance of the <see cref="NntpWebException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
		public NntpWebException(string message, Exception innerException): base(message, innerException) {}
	}
	
	/// <summary>
	/// NntpResourceAuthorizationException is raised if a NNTP request results in a statuscode 480.
	/// </summary>
	[Serializable, ComVisible(false)]
	public class NntpResourceAuthorizationException : ResourceAuthorizationException
	{
		/// <summary></summary>
		public NntpResourceAuthorizationException() : base(ComponentsText.ExceptionNntpResourceAuthorization) { }
		/// <summary></summary>
		public NntpResourceAuthorizationException(string message) : base(message) { }
		/// <summary></summary>
		public NntpResourceAuthorizationException(string message, Exception innerException) : base(message, innerException) { }
	}

    /// <summary>
    /// A class that understands the NNTP protocol.
    /// Could be useful for creating binary news reader,
    /// or even normal news readers.
    /// 
    /// Includes support for decoding uuencoded and yencoded single and multipart-message files.
    ///
    /// Does not include support for MIME attachments.
    ///
    /// Thanks to Patrick Steel (psteele@ipdsolution.co) for publishing code in microsoft.public.dotnet.languages.csharp
    /// on 2001-09-13 19:26:12 PST for getting me started on using NNTP in C#
    /// </summary>
    /// <remarks>
    /// Old spec. see: http://tools.ietf.org/html/rfc977
    /// New spec. see: http://tools.ietf.org/html/rfc3977 (replace 977)
    /// </remarks>
    internal class NntpClient: IDisposable{

		/// <summary>
		/// Matches headers
		/// </summary>
		static readonly Regex xheaderResult = new Regex(@"(?<1>\d+)\s(?<2>.*)");
             
		private static readonly log4net.ILog _log = RssBandit.Common.Logging.Log.GetLogger(typeof(NntpClient));

		/// <summary>
		/// Instantiates class
		/// </summary>
		/// <param name="Server">The server to connect to</param>
		/// <param name="Port">The port to connect on</param>
		internal NntpClient(string Server, int Port) {
			socket = new MyTcpClient(Server, Port);
			socket.NoDelay = true;
			connectResp = GetData(false);
		}

		/// <summary>
		/// Instantiates class. Port defaults to 119. 
		/// </summary>
		/// <param name="Server">The server to connect to</param>
		internal NntpClient(string Server): this(Server, 119){;}
		
			       
		/// <summary>
		/// The connection response. 
		/// </summary>
        internal string ConnectResponse{
            get{
                return connectResp;
            }
        }


		/// <summary>
		/// Used to set the Send and Receive timeout for the underlying TcpClient. 
		/// This does not set the connection time out. 
		/// </summary>
		internal int Timeout{
			set { 
					socket.SendTimeout = value;  
					socket.ReceiveTimeout = value; 	
			}		
		}

		/// <summary>
		/// Index of the first message.
		/// </summary>
        internal int FirstMsg{
            get{
                return firstMsg;
            }
        }

		/// <summary>
		/// Index of the last message. 
		/// </summary>
        internal int LastMsg{
            get{
                return lastMsg;
            }
        }

    	/// <summary>
        ///  Disconnect and cleanup
        /// </summary>
        public void Dispose(){
            if( socket != null){
                Send("QUIT");
                GetData(false);
                socket.Close(); 
            }
            socket = null;
        }

        /// <summary>
        /// Tries to authenticate the user
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="password">The password</param>
        /// <returns>true if login was successful</returns>
        internal bool AuthInfo(string userName, string password){
            int responseCode = SendAndParseResponse("AUTHINFO USER " + userName);
            
			if(responseCode == 281){
                return true;
            }else if(responseCode == 381){
                responseCode = SendAndParseResponse("AUTHINFO PASS " + password);
            
				if(responseCode < 300){
                    return true;
                }
            }
            return false;
        }

        // For short commands
        private int SendAndParseResponse(string command)
        {
            try
            {
                Send(command);
                string response = GetData(false);
                if(response.Length > 3)
                {
                    return Convert.ToInt32(response.Substring(0,3));
                }
                return 999;
            }
            catch
            {
                return 999;
            }
        }

        /// <summary>
        /// Writes a list of groups available on the server to the 
        /// </summary>
        /// <param name="writer"></param>      
        internal void  Groups(TextWriter writer)
        {           
            string response;
			bool firstTimeAround = true; 
            //string[] sList;
            //StringCollection oColl = new StringCollection();

			// send LIST command
            Send("LIST");  
            //string sDump = "";

            // get the response
			do{ 
                response = GetData(true);	
				if(firstTimeAround){
					if ((response.Length >= 3) && response.Substring(0, 3) == "480")
					{
						throw new NntpResourceAuthorizationException();
					}
					if ((response.Length < 3) || response.Substring(0, 3) != "215")
					{
						throw new NntpWebException(response);
					}
					firstTimeAround = false; 
				}
				writer.Write(response); 
                //sDump = sDump + sData;                
            }while((response.EndsWith(".\r\n")!= true) && (response.Length > 0));

            /*
            sList = Split.Split(sDump);
            for(int i = 0 ; i <= sList.Length - 3; i++)
            {
                oColl.Add(sList[i].Substring(0, sList[i].IndexOf(" ")));
            }
            return oColl;
			*/
        }

        // StreamCreator is a delegate that's used to create a stream to store
        // the decoded file. Set stream to null if you don't want to decode the file.
        //
        // Here's an example implementation:
        //
        // void MyStreamCreator(string fileName, out Stream stream)
        // {
        //     if( File.Exists(fileName))
        //     {
        //         Console.WriteLine("{0} already exists.", fileName);
        //         stream = null;
        //     }
        //     else
        //     {
        //         Console.WriteLine("{0}", fileName);
        //         stream = File.Create(fileName);
        //     }
        // }
        //
        //     Call it like this:
        //     Decode(message, new StreamCreator(MyStreamCreator));
        //

        internal delegate void StreamCreator(string fileName, out Stream stream);


		/// <summary>
		/// Posts a message to a particular newgroup 
		/// </summary>
		///<param name="message">The message to send</param>
		internal void Post(string message) {			


 			//see if the server supports posting 
			Send("POST");
			string response = GetData(false);
			if (response.Substring( 0, 3) != "340") {
				throw new NntpWebException(response);
			}
 		
			//send message 
			Send(message);
			response = GetData(false);
			if (response.Substring( 0, 3) != "240") {
				throw new NntpWebException(response);
			}
		}

        internal void Decode(NntpMessage message, StreamCreator streamCreator)
        {
            NntpMessage[] parts = new NntpMessage[1];
            parts[0] = message;
            DecodeMultiPart(parts, streamCreator);
        }

        internal void DecodeMultiPart(NntpMessage[] parts, StreamCreator streamCreator)
        {
            Regex split = new Regex(Environment.NewLine);
            Regex uuencodeFilePattern = new Regex(@"^begin\s\d\d\d\s+(?<1>.+)\s*");
            Regex yencodeFilePattern = new Regex(@"^\=ybegin .*name=(?<1>.+)$");
            bool bLookingForBegin = true;
            bool bIsYenc = false;
            Stream fileStream = null;
            try
            {
                // If parts are missing, don't decode...
                for(int i = 0; i < parts.Length; i++)
                {
                    if(parts[i] == null)
                    {
                        return;
                    }
                }

                for(int i = 0; i < parts.Length; i++)
                {
                    NntpMessage msg = parts[i];
                    EnsureBody(msg);
                    string body = msg.Body;
                    string[] lines = split.Split(body);
                    for(int j = 0; j < lines.Length; j++)
                    {
                        string line = lines[j];
                        if(bLookingForBegin)
                        {
                            Match m = uuencodeFilePattern.Match(line);
                            if(m.Success)
                            {
                                bLookingForBegin = false;
                                string fileName = CleanFileName(m.Groups[1].Value);
                                streamCreator(fileName, out fileStream);
                                if(fileStream == null)
                                {
                                    return;
                                }
                            }
                            m = yencodeFilePattern.Match(line);
                            if(m.Success)
                            {
                                bIsYenc = true;
                                bLookingForBegin = false;
                                string fileName = CleanFileName(m.Groups[1].Value);
                                streamCreator(fileName, out fileStream);
                                if(fileStream == null)
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if(bIsYenc)
                            {
                                if(line.StartsWith("=y"))
                                {
                                    continue;
                                }
                            {
                                char[] lineBufChars = line.ToCharArray();
                                byte[] lineBuf = new byte[lineBufChars.Length];
                                for(int k = 0; k < lineBuf.Length; k++)
                                {
                                    lineBuf[k] = (byte) lineBufChars[k];
                                }

                                byte[] decoded = new byte[lineBuf.Length];
                                int length = yydecode(decoded, 0, lineBuf, 0, lineBuf.Length);
                                fileStream.Write(decoded,0,length);
                            }
                            }
                            else
                            {
                                if(line.Length == 0 || line == "end")
                                {
                                    break;
                                }
                                byte[] lineBuf = Encoding.ASCII.GetBytes(line);
                                byte[] decoded = new byte[lineBuf.Length];
                                int length = uudecode(decoded, 0, lineBuf, 0, lineBuf.Length);
                                fileStream.Write(decoded,0,length);
                            }
                        }
                    }
                }
            }
            finally 
            {
                if(fileStream != null)
                {
                    fileStream.Close();
                }
            }
        }

        internal void EnsureBody(NntpMessage msg)
        {
            if(msg.Body == "")
            {
                // now ask for the body
                Send("BODY " + msg.Id);
                string sData = GetData(true);
                if(sData.Length > 1 && sData[0] == '2')
                {
                    msg.SetBody(Split.Split(sData));
                }
            }
        }

        // uuencode/uudecode is the original netnews way of encoding data

        private static byte uudecode(byte c)
        {
            return (byte) ((c - ' ') & 0x3f);
        }

        private static int uudecode(byte[] decoded, int decodedStart, byte[] encoded, int encodedStart, int encodedLength)
        {
            if(encodedLength < 1)
            {
                return 0;
            }
            int n = uudecode(encoded[encodedStart]);
            int expected = ((n+2)/3)*4;
            if( expected > (encodedLength-1))
            {
                // someone trimmed off trailing spaces
                byte[] newEncoded = new byte[expected+1];
                Array.Copy(encoded,encodedStart,newEncoded,0,encodedLength);
                for(int i = encodedLength; i <= expected;i++)
                {
                    newEncoded[i] = (byte) ' ';
                }
                return uudecode(decoded, decodedStart, newEncoded, 0, newEncoded.Length);
            }

            // decode in groups of four bytes
            int e = encodedStart + 1;
            int d = decodedStart;
            int c = n;
            while(c > 0)
            {
                byte s0 = uudecode(encoded[e]);
                byte s1 = uudecode(encoded[e+1]);
                byte s2 = uudecode(encoded[e+2]);
                byte s3 = uudecode(encoded[e+3]);
                byte d0 = (byte) ((s0 << 2) | (0x03 & (s1 >> 4)));
                byte d1 = (byte) ((s1 << 4) | (0x0f & (s2 >> 2)));
                byte d2 = (byte) ((s2 << 6) | s3);
                decoded[d] = d0;
                if(c>1)
                {
                    decoded[d+1] = d1;
                }
                if(c>2)
                {
                    decoded[d+2] = d2;
                }
                e += 4;
                d += 3;
                c -= 3;
            }
            return n;
        }

        // yenc is the new, improved way of encoding binary data for netnews

        private static byte yydecode(byte c)
        {
            return (byte) ((c - 42) & 0xff);
        }
        private static int yydecode(byte[] decoded, int decodedStart, byte[] encoded, int encodedStart, int encodedLength)
        {
            if(encodedLength < 1)
            {
                return 0;
            }
            int e = encodedStart;
            int n = encodedLength;
            int d = decodedStart;
            while(n > 0)
            {
                byte c = encoded[e++];
                --n;
                if(c == '=')
                {
                    if(n > 0)
                    {
                        c = (byte)(encoded[e++]-64);
                        --n;
                    }
                }
                decoded[d++] = yydecode(c);
            }
            return d-decodedStart;
        }

        internal static void GroupFiles(NntpMessages messages, SortedList multiFiles)
        {
            Regex[] multipartPatterns = new Regex[2];
            multipartPatterns[0] = new Regex(@"(?<1>.*)\s*\((?<2>\d+)/(?<3>\d+)\)"); // filename (#/#)
            multipartPatterns[1] = new Regex(@"(?<1>.*)\s*\[(?<2>\d+)/(?<3>\d+)\]"); // filename [#/#]
            foreach(NntpMessage oMsg in messages)
            {
                string subject = oMsg.Headers["subject"];
                if(subject != null)
                {
                    foreach(Regex multipart in multipartPatterns)
                    {
                        Match m = multipart.Match(subject);
                        if(m.Success)
                        {
                            string file = m.Groups[1].Value;
                            int part = Convert.ToInt32(m.Groups[2].Value);
                            int whole = Convert.ToInt32(m.Groups[3].Value);
                            if(part <= 0 || part > whole)
                            {
                                continue;
                            }
                            NntpMessage[] v;
                            if( multiFiles.Contains(file) )
                            {
                                v = (NntpMessage[]) multiFiles[file];
                            }
                            else
                            {
                                v = new NntpMessage[whole];
                                multiFiles.Add(file,v);
                            }
                            if(v[part-1] != null)
                            {
                                // Go with original over Reply
                                NntpMessage old = v[part-1];
                                string oldSubject = old.Headers["subject"];
                                if(! oldSubject.StartsWith("Re:"))
                                {
                                    continue;
                                }
                            }
                            v[part-1] = oMsg;
                            goto nextPart;
                        }
                    }

                    // If we got here, none of the multipart patterns matched
                {
                    NntpMessage[] v = new NntpMessage[1];
                    v[0] = oMsg;
                    multiFiles.Add(oMsg.Id,v);
                }

                nextPart:;
                }
            }
        }


        private readonly Encoding ASCII = Encoding.ASCII;
        private readonly Regex Split = new Regex("\r\n");


        private MyTcpClient socket;    // tcp socket
        private readonly String connectResp = "";  // connect response

        private String CurrGroup = "";   // current selected group
/*
        private int lastGroupMsgCount = -1;   // last number of messages in grp (prev. request)
*/
		//private int currGroupMsgCount;   // current number of messages in grp (this request)
		private int firstMsg;    // 1st message in group
        private int lastMsg;    // last message in group

        private readonly byte[] bRecv = new byte[4096];
        private readonly char[] bRecvChars = new Char[4096];
        private readonly StringBuilder sb = new StringBuilder();

        // class that lets us do partial reads
        private class MyTcpClient : TcpClient
        {
            internal MyTcpClient(string Server, int Port)
            : base(Server, Port)
            {
            }
            internal int Receive(byte[] bytes, int offset, int length)
            {
                return Client.Receive(bytes, offset, length,0);
            }
        }

        /// <summary>
        /// send a string of data over the socket
        /// </summary>
        /// <param name="sData">The data to send</param>
        private void Send(string sData)
        {
            byte[] bSend = ASCII.GetBytes(sData + Environment.NewLine);

            socket.GetStream().Write(bSend, 0, bSend.Length);
        }



		/// <summary>
		///  Get data from the socket
		/// </summary>
		/// <param name="expectLongResponse">flag indicates whether to expect a long response or not</param>
		/// <returns></returns>
		private string GetData(bool expectLongResponse){
			sb.Length = 0;
			StringWriter sw = new StringWriter(sb);
			
			//TODO: Decide if we propagate an exception to the user if server returns an error
			GetData(expectLongResponse, sw);
			sw.Flush();
			sw.Close(); 
			return sb.ToString(); 
		}

        /// <summary>
        ///  Get data from the socket
        /// </summary>
        /// <param name="expectLongResponse">flag indicates whether to expect a long response or not</param>
        /// <param name="writer">used for writing the response from the server</param>
        /// <returns>returns true if the operation was successful and false if an error occured</returns>
        private void GetData(bool expectLongResponse, TextWriter writer)
        {
            int iBytes;
            sb.Length = 0;
            bool bFirstLine = true;
            bool bLookForDotEnd = false;
            bool bError = false;
            do 
            {
                try 
                {
                    // We use Receive so we don't have to wait for timeouts at the
                    // end of variable-sized responses from the NNTP server.
                    iBytes = socket.Receive(bRecv, 0, bRecv.Length);
                    for(int i = 0; i < iBytes; i++)
                    {
                        bRecvChars[i] = (char) bRecv[i];
                    }

					writer.Write(bRecvChars,0,iBytes); 
                    //sb.Append(bRecvChars,0,iBytes);
                    
					if(bFirstLine)
                    {
                        if(sb.Length >= 3)
                        {
                            string codeString = sb.ToString(0,3);
                            int code = Convert.ToInt32(codeString);
                            bFirstLine = false;
                            if(code < 299)
                            {
                                if(expectLongResponse)
                                {
                                    bLookForDotEnd = true;
                                }
                            }
                            else
                            {
                                bError = true;
                            }
                        }
                    }
                    if(bLookForDotEnd && sb.Length >= 5 && sb.ToString(sb.Length-5, 5) == "\r\n.\r\n")
                    {
                        break;
                    }
                    else if((bError || !expectLongResponse) && sb.Length >= 2 && sb.ToString(sb.Length-2,2) == "\r\n")
                    {
                        break;
                    }
                }
                catch(Exception ex)
                {
                    throw new NntpWebException(ex.Message, ex); 
                }
            }
            while(iBytes > 0);
        }

        // select a particular group
        internal void SelectGroup(string GroupName)
        {
            string sData;

            Send("GROUP " + GroupName);
            sData = GetData(false);
            // check for errors
            if( sData.Substring(0,3) == "411")
            {
                throw new NntpWebException("No such group: " + GroupName);
			}
			else if (sData.Substring(0, 3) == "550")
			{
				throw new NntpWebException("Invalid newsgroup: " + GroupName);
			}
            
			string[] messageNumbers = sData.Split(' ');
        	// index 0 is the response code
			//currGroupMsgCount = Convert.ToInt32( messageNumbers[1] );
			firstMsg = Convert.ToInt32( messageNumbers[2] );
			lastMsg = Convert.ToInt32( messageNumbers[3] );
			CurrGroup = GroupName;
            
        }		

		/// <summary>
		/// Retrieves all the messages posted to the 
		/// newsgroup since the specified date. 
		/// </summary>
		/// <param name="since">The date</param>
		/// <param name="downloadCount">Indicates the number of articles that should be 
		/// downloaded if the news server doesn't understand the NEWNEWS request</param>
		/// <param name="sw">used for writing the server response</param>
		internal void GetNntpMessages(DateTime since, int downloadCount, TextWriter sw){

			IDictionary capabilities = this.GetCapabilities(null);
			string sData; 
			since = since.ToUniversalTime(); 
			
			Send(String.Format("NEWNEWS {0} {1} GMT", this.CurrGroup, since.ToString("yyMMdd HHmmss")));
			sData = GetData(true); 

			// capabilities are a RFC3977 feature, not widely impl. by servers
			// borland news server returns "230 newnews disabled by admin, empty list follows \r\n.\r\n"
			// so we fallback after checking capabilities to handle empty lists
			// with our backup technique
			if(sData.Length > 0 && sData[0] == '2' && sData.IndexOf("empty") == -1 &&
			   (capabilities.Count == 0 || capabilities.Contains("NEWNEWS")))
			{
				
				string sDump = sData; 
				StringCollection messageIds = new StringCollection(); 

				while((sData.EndsWith(".\r\n")!= true) && (sData.Length > 0)){
					sData = GetData(true);						
					sDump = sDump + sData;                
				}

				// split up list of message ids
				string[] sList = Split.Split(sDump);
				for(int i = 1 ; i <= sList.Length - 3; i++) { //start at 1 to skip '230' response 
					messageIds.Add(sList[i]);
				}

				GetNntpMessages(messageIds, downloadCount, sw); 

			}else{ //the NEWNEWS method may not be supported so we use our backup technique
				int last = this.LastMsg; //this.FirstMsg;
				int first =  Math.Max(last - downloadCount, this.FirstMsg); //Math.Min(first + downloadCount, this.LastMsg);
				GetNntpMessages(first, last, sw); 
			}

			//flush writer to ensure all data gets written to underlying stream
			sw.Flush(); 
		}

         /// <summary>
         /// Retrieve a message from the server
         /// </summary>
         /// <param name="id">the parameter that identifies the message to retrieve</param>
         /// <param name="sw">used for writing the server response</param>
        internal void GetNntpMessage(string id,  TextWriter sw){
                   
			try{
				string sDump = String.Empty, sData; 	

				Send("ARTICLE " + id);								

				do{ 
					sData = GetData(true);						
					sDump = sDump + sData;                
					
					//article with that ID not found, shouldn't happen but sometimes does
					if(sData.StartsWith("423") || sData.StartsWith("430")){
						break; 
					}

				}while((sData.EndsWith(".\r\n")!= true) && (sData.Length > 0));          

				//TODO: Decide whether we propagate an exception here on error             
				if(sData.Length > 0 && sData[0] == '2'){               
					sw.Write(sData); 
				}
			}catch(Exception e){_log.Error("GetNntpMessage() failed", e);}
        }

       
		 
		/// <summary>
		/// Retrieves a list of messages given their message ids. 
		/// </summary>
		/// <param name="oMsgs">a list of message ids</param>
		/// <param name="downloadCount">The maximum number of messages that should be downloaded</param>
		/// <param name="sw">the TextWriter for writing the servers response</param>
        internal void GetNntpMessages(StringCollection oMsgs, int downloadCount, TextWriter sw)
        {            
			int downloaded = 0; 

			for(int i = oMsgs.Count; i-->0;){
             
				string sId = oMsgs[i]; 								    
				GetNntpMessage(sId, sw);               
				downloaded++;    
				
				if(downloaded == downloadCount){
					break;
				}
            }//foreach          
        
		}
		

		/// <summary>
		/// Retrieves NNTP messages from the server within a specified range
		/// </summary>
		/// <param name="first">the start of the range</param>
		/// <param name="last">the end of the range</param>
		/// <param name="sw">the TextWriter for writing the servers response</param>
        internal void GetNntpMessages(int first, int last, TextWriter sw)
        {            

            for(int i = first; i <= last; i++){            
                GetNntpMessage(i.ToString(), sw);               
            }			     
        }

        internal NntpMessages GetNntpMessages(int first, int last, string header)
        {
            NntpMessages oList = new NntpMessages();
        	// http://tools.ietf.org/html/rfc2980 (XHDR extension):
            string result = SendCommand("XHDR " + header + " " + first + "-" + last, true);
           
            if(result.Length > 0 && result[0] == '2')
            {
                string[] lines = this.Split.Split(result);
                for(int i = 1; i < lines.Length - 2; i++)
                {
                    string line = lines[i];
                    Match m = xheaderResult.Match(line);
                    if(m.Success)
                    {
                        NntpMessage oMsg = new NntpMessage();
                        oMsg.SetXHeader(m.Groups[1].Value, header, m.Groups[2].Value);
                        oList.Add(oMsg);
                    }
                }
            }
            return oList;
        }

		/// <summary>
		/// Gets the capabilities of the NNTP server.
		/// </summary>
		/// <param name="additionalExtension">Optional: an additional extension to query for.</param>
		/// <returns>IDictionary with Capabilities listed in http://tools.ietf.org/html/rfc3977#section-5.2</returns>
    	internal IDictionary GetCapabilities(string additionalExtension) {
    		HybridDictionary dict = new HybridDictionary();
    		string cmd = "CAPABILITIES";
    		if (!string.IsNullOrEmpty(additionalExtension))
    			cmd += String.Format(" {0}", additionalExtension);
    		
    		string result = SendCommand(cmd, false);
			if(result.Length > 0 && result[0] == '1') {
				string[] lines = this.Split.Split(result);
				for(int i = 1; i < lines.Length - 2; i++) {
					dict.Add(lines[i], null);
				}
			}
    		return dict;
    	}
    	
        // for the rest of the unimplemented commands...		
        internal string SendCommand(string NNTPCommand, bool expectLongResponse)
        {
            Send(NNTPCommand);
            return GetData(true);
        }

        private static string CleanFileName(string fileName)
        {
            return Path.GetFileName(fileName);
        }
    }
}

#region CVS Version Log
/*
 * $Log: NntpClient.cs,v $
 * Revision 1.15  2007/05/10 17:02:33  t_rendelmann
 * fixed: borland NNTP server does not return posts
 *
 * Revision 1.14  2007/04/30 10:02:38  t_rendelmann
 * changed: replaced Console.Write with log messaging
 *
 */
#endregion