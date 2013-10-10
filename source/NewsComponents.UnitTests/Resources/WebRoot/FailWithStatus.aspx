<%@ Page Language="C#" EnableSessionState="False" EnableViewState="False" %>
<%@ Import Namespace="System.Web" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.Clear();
        Response.StatusCode = GetStatusCode();
        Response.End();
    }
    int GetStatusCode()
    {
        return Convert.ToInt32(this.Request.QueryString.Get("code"));
    }
</script>
Failure