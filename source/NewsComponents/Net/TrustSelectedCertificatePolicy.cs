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
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace NewsComponents.Net
{
    #region CertificateIssue enum; CertificateIssueCancelEventArgs - used by Certificate policy handling

    /// <summary>
    /// Possible Certificate issues.
    /// </summary>
    /// <remarks> The .NET Framework should expose these, but they don't.</remarks>
    [Serializable]
    public enum CertificateIssue : long
    {
        /// <summary>
        /// 
        /// </summary>
        CertEXPIRED = 0x800B0101,
        /// <summary>
        /// 
        /// </summary>
        CertVALIDITYPERIODNESTING = 0x800B0102,
        /// <summary>
        /// 
        /// </summary>
        CertROLE = 0x800B0103,
        /// <summary>
        /// 
        /// </summary>
        CertPATHLENCONST = 0x800B0104,
        /// <summary>
        /// 
        /// </summary>
        CertCRITICAL = 0x800B0105,
        /// <summary>
        /// 
        /// </summary>
        CertPURPOSE = 0x800B0106,
        /// <summary>
        /// 
        /// </summary>
        CertISSUERCHAINING = 0x800B0107,
        /// <summary>
        /// 
        /// </summary>
        CertMALFORMED = 0x800B0108,
        /// <summary>
        /// 
        /// </summary>
        CertUNTRUSTEDROOT = 0x800B0109,
        /// <summary>
        /// 
        /// </summary>
        CertCHAINING = 0x800B010A,
        /// <summary>
        /// 
        /// </summary>
        CertREVOKED = 0x800B010C,
        /// <summary>
        /// 
        /// </summary>
        CertUNTRUSTEDTESTROOT = 0x800B010D,
        /// <summary>
        /// 
        /// </summary>
        CertREVOCATION_FAILURE = 0x800B010E,
        /// <summary>
        /// 
        /// </summary>
        CertCN_NO_MATCH = 0x800B010F,
        /// <summary>
        /// 
        /// </summary>
        CertWRONG_USAGE = 0x800B0110,
        /// <summary>
        /// 
        /// </summary>
        CertUNTRUSTEDCA = 0x800B0112
    }

    /// <summary>
    /// Cancelable Event Argument class to handle certificate issues on web requests.
    /// </summary>
    public class CertificateIssueCancelEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Problem/Issue caused
        /// </summary>
        public CertificateIssue CertificateIssue;

        /// <summary>
        /// The certificate, that casued the problem
        /// </summary>
        public X509Certificate Certificate;

        /// <summary>
        /// The involved WebRequest.
        /// </summary>
        public WebRequest WebRequest;

        /// <summary>
        /// Designated initializer
        /// </summary>
        /// <param name="issue">CertificateIssue</param>
        /// <param name="cert">X509Certificate</param>
        /// <param name="request">WebRequest</param>
        /// <param name="cancel">bool</param>
        public CertificateIssueCancelEventArgs(CertificateIssue issue, X509Certificate cert, WebRequest request,
                                               bool cancel)
            : base(cancel)
        {
            CertificateIssue = issue;
            Certificate = cert;
            WebRequest = request;
        }
    }

    #endregion

    ///// <summary>
    ///// Does enable certificate acceptance. 
    ///// See also http://weblogs.asp.net/tgraham/archive/2004/08/12/213469.aspx
    ///// and http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpguide/html/cpconhostingremoteobjectsininternetinformationservicesiis.asp
    ///// </summary>
    //internal class TrustSelectedCertificatePolicy : ICertificatePolicy
    //{
    //    // this is marked obsolete by MS in the CLR 2.0
    //    public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem)
    //    {
    //        try
    //        {
    //            if (problem != 0)
    //            {
    //                // move bits around to get it casted from an signed int to a normal long enum type:
    //                CertificateIssue issue = (CertificateIssue)(((problem << 1) >> 1) + 0x80000000);

    //                // this is marked obsolete by MS in the CLR 2.0
    //                // It seems also they has broken the old impl., we don't get a valid cert object now (handle is 0) on WinXP SP2
    //                // via parameter, so we now use that of the servicepoint as a workaround:
    //                CertificateIssueCancelEventArgs args = new CertificateIssueCancelEventArgs(issue, sp.Certificate, req, true);
    //                AsyncWebRequest.RaiseOnCertificateIssue(sp, args);
    //                return !args.Cancel;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Trace.WriteLine("TrustSelectedCertificatePolicy.CheckValidationResult() error: " + ex.Message);
    //        }
    //        // The 1.1 framework calls this method with a problem of 0, even if nothing is wrong
    //        return (problem == 0);
    //    }

    //    /// <summary>
    //    /// Checks the server certificate.
    //    /// </summary>
    //    /// <param name="sender">The sender.</param>
    //    /// <param name="certificate">The certificate.</param>
    //    /// <param name="chain">The chain.</param>
    //    /// <param name="sslPolicyErrors">The SSL policy errors.</param>
    //    /// <returns></returns>
    //    public static bool CheckServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    //    {
    //        //TODO: impl.
    //        return true;
    //    }

    //}
}
