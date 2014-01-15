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
using System.Net;

namespace NewsComponents.Net
{
    /// <summary>
    /// First, we enabled default credentials to be used as proxy credentials
    /// in our app.config. For the CLR 3.x there is one more requirement to fullfill:
    /// see http://www.codeproject.com/KB/miscctrl/WPF_proxy_authentication.aspx
    /// </summary>
    class ProxyCredentialsPolicy : ICredentialPolicy
    {
        bool ICredentialPolicy.ShouldSendCredential(Uri challengeUri, WebRequest request, NetworkCredential credential, IAuthenticationModule authenticationModule)
        {
            if (request != null && request.Proxy != null)
                return true;
            // only send, if web server explicitely request that:
            return false;
        }
    }
}