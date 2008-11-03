﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.832
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 2.0.50727.832.
// 
#pragma warning disable 1591

namespace RssBandit.RssBandit.UpdateService {
    using System.Diagnostics;
    using System.Web.Services;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="UpdateServiceSoap", Namespace="urn:schemas-rssbandit-org:rssbandit:update-services")]
    public partial class UpdateService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback DownloadLinkOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public UpdateService() {
            this.Url = "http://localhost/RssBandit.UpdateService/UpdateService.asmx";
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event DownloadLinkCompletedEventHandler DownloadLinkCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:schemas-rssbandit-org:rssbandit:update-services/DownloadLink", RequestNamespace="urn:schemas-rssbandit-org:rssbandit:update-services", ResponseNamespace="urn:schemas-rssbandit-org:rssbandit:update-services", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string DownloadLink(string appID, string currentAppVersion, string appKey) {
            object[] results = this.Invoke("DownloadLink", new object[] {
                        appID,
                        currentAppVersion,
                        appKey});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public System.IAsyncResult BeginDownloadLink(string appID, string currentAppVersion, string appKey, System.AsyncCallback callback, object asyncState) {
            return this.BeginInvoke("DownloadLink", new object[] {
                        appID,
                        currentAppVersion,
                        appKey}, callback, asyncState);
        }
        
        /// <remarks/>
        public string EndDownloadLink(System.IAsyncResult asyncResult) {
            object[] results = this.EndInvoke(asyncResult);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void DownloadLinkAsync(string appID, string currentAppVersion, string appKey) {
            this.DownloadLinkAsync(appID, currentAppVersion, appKey, null);
        }
        
        /// <remarks/>
        public void DownloadLinkAsync(string appID, string currentAppVersion, string appKey, object userState) {
            if ((this.DownloadLinkOperationCompleted == null)) {
                this.DownloadLinkOperationCompleted = new System.Threading.SendOrPostCallback(this.OnDownloadLinkOperationCompleted);
            }
            this.InvokeAsync("DownloadLink", new object[] {
                        appID,
                        currentAppVersion,
                        appKey}, this.DownloadLinkOperationCompleted, userState);
        }
        
        private void OnDownloadLinkOperationCompleted(object arg) {
            if ((this.DownloadLinkCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.DownloadLinkCompleted(this, new DownloadLinkCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.42")]
    public delegate void DownloadLinkCompletedEventHandler(object sender, DownloadLinkCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.42")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class DownloadLinkCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal DownloadLinkCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591