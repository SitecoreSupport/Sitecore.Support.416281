namespace Sitecore.Shell.Applications.ContentEditor
{
    using Sitecore;
    using Sitecore.Data;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.Links;
    using Sitecore.Text;
    using Sitecore.Web;
    using Sitecore.Web.UI.Sheer;
    using System;
    using System.Collections.Specialized;
    using System.Web.UI;

    public class Link : LinkBase
    {
        private bool linkBroken;

        public Link()
        {
            this.Class = "scContentControl";
            base.Activation = true;
        }

        private void ClearLink()
        {
            if (this.Value.Length > 0)
            {
                this.SetModified();
            }
            this.XmlValue = new Sitecore.Shell.Applications.ContentEditor.XmlValue(string.Empty, "link");
            this.Value = string.Empty;
            Sitecore.Context.ClientPage.ClientResponse.SetAttribute(this.ID, "value", string.Empty);
        }

        protected override void DoRender(HtmlTextWriter output)
        {
            base.DoRender(output);
            if (this.linkBroken)
            {
                output.Write("<div style=\"color:#999999;padding:2px 0px 0px 0px\">{0}</div>", Translate.Text("The item selected in the field does not exist, or you do not have read access to it."));
            }
        }

        private void Follow()
        {
            Sitecore.Shell.Applications.ContentEditor.XmlValue xmlValue = this.XmlValue;
            string attribute = xmlValue.GetAttribute("linktype");
            if (attribute != null)
            {
                if ((attribute != "internal") && (attribute != "media"))
                {
                    if ((attribute != "external") && (attribute != "mailto"))
                    {
                        if (attribute == "anchor")
                        {
                            SheerResponse.Alert(Translate.Text("You cannot follow an Anchor link."), new string[0]);
                            return;
                        }
                        if (attribute == "javascript")
                        {
                            SheerResponse.Alert(Translate.Text("You cannot follow a Javascript link."), new string[0]);
                        }
                        return;
                    }
                }
                else
                {
                    string str2 = xmlValue.GetAttribute("id");
                    if (!string.IsNullOrEmpty(str2))
                    {
                        Sitecore.Context.ClientPage.SendMessage(this, "item:load(id=" + str2 + ")");
                    }
                    return;
                }
                string str3 = xmlValue.GetAttribute("url");
                if (!string.IsNullOrEmpty(str3))
                {
                    SheerResponse.Eval("window.open('" + str3 + "', '_blank')");
                }
            }
        }

        private string GetLinkPath()
        {
            Sitecore.Shell.Applications.ContentEditor.XmlValue xmlValue = this.XmlValue;
            string attribute = xmlValue.GetAttribute("id");
            string str2 = string.Empty;
            Item item = null;
            if (!string.IsNullOrEmpty(attribute) && Sitecore.Data.ID.IsID(attribute))
            {
                item = Client.ContentDatabase.GetItem(new ID(attribute));
            }
            if (item != null)
            {
                if (this.Value.EndsWith("." + "aspx"))
                {
                    if (item.Paths.Path.StartsWith("/sitecore/content", StringComparison.InvariantCulture))
                    {
                        str2 = item.Paths.Path.Substring("/sitecore/content".Length);
                        if (LinkManager.AddAspxExtension)
                        {
                            str2 = str2 + ("." + "aspx");
                        }
                        return str2;
                    }
                    if (item.Paths.Path.StartsWith("/sitecore/media library", StringComparison.InvariantCulture))
                    {
                        str2 = item.Paths.Path + ("." + "aspx");
                    }
                    return str2;
                }
                if (item.Paths.Path.StartsWith("/sitecore/media library", StringComparison.InvariantCulture))
                {
                    str2 = item.Paths.Path.Substring("/sitecore/media library".Length);
                }
                return str2;
            }
            return xmlValue.GetAttribute("url");
        }

        public override string GetValue() =>
            this.XmlValue.ToString();

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            if (message["id"] == this.ID)
            {
                switch (message.Name)
                {
                    case "contentlink:internallink":
                        {
                            NameValueCollection additionalParameters = new NameValueCollection();
                            additionalParameters.Add("width", "685");
                            this.Insert("/sitecore/shell/Applications/Dialogs/Internal link.aspx", additionalParameters);
                            return;
                        }
                    case "contentlink:media":
                        {
                            NameValueCollection values3 = new NameValueCollection();
                            values3.Add("umwn", "1");
                            NameValueCollection values = values3;
                            this.Insert("/sitecore/shell/Applications/Dialogs/Media link.aspx", values);
                            return;
                        }
                    case "contentlink:externallink":
                        {
                            NameValueCollection values4 = new NameValueCollection();
                            values4.Add("height", "425");
                            this.Insert("/sitecore/shell/Applications/Dialogs/External link.aspx", values4);
                            return;
                        }
                    case "contentlink:anchorlink":
                        {
                            NameValueCollection values5 = new NameValueCollection();
                            values5.Add("height", "335");
                            this.Insert("/sitecore/shell/Applications/Dialogs/Anchor link.aspx", values5);
                            return;
                        }
                    case "contentlink:mailto":
                        {
                            NameValueCollection values6 = new NameValueCollection();
                            values6.Add("height", "335");
                            this.Insert("/sitecore/shell/Applications/Dialogs/Mail link.aspx", values6);
                            return;
                        }
                    case "contentlink:javascript":
                        {
                            NameValueCollection values7 = new NameValueCollection();
                            values7.Add("height", "418");
                            this.Insert("/sitecore/shell/Applications/Dialogs/Javascript link.aspx", values7);
                            return;
                        }
                    case "contentlink:follow":
                        this.Follow();
                        return;

                    case "contentlink:clear":
                        this.ClearLink();
                        return;
                }
            }
        }

        protected void Insert(string url)
        {
            Assert.ArgumentNotNull(url, "url");
            this.Insert(url, new NameValueCollection());
        }

        protected void Insert(string url, NameValueCollection additionalParameters)
        {
            Assert.ArgumentNotNull(url, "url");
            Assert.ArgumentNotNull(additionalParameters, "additionalParameters");
            NameValueCollection values2 = new NameValueCollection();
            values2.Add("url", url);
            values2.Add(additionalParameters);
            NameValueCollection parameters = values2;
            Sitecore.Context.ClientPage.Start(this, "InsertLink", parameters);
        }

        protected void InsertLink(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.IsPostBack)
            {
                if (!string.IsNullOrEmpty(args.Result) && (args.Result != "undefined"))
                {
                    this.XmlValue = new Sitecore.Shell.Applications.ContentEditor.XmlValue(args.Result, "link");
                    this.SetValue();
                    this.SetModified();
                    Sitecore.Context.ClientPage.ClientResponse.SetAttribute(this.ID, "value", this.Value);
                    SheerResponse.Eval("scContent.startValidators()");
                }
            }
            else
            {
                UrlString urlString = new UrlString(args.Parameters["url"]);
                string width = args.Parameters["width"];
                string height = args.Parameters["height"];
                this.GetHandle().Add(urlString);
                urlString.Append("ro", this.Source);
                urlString.Add("la", this.ItemLanguage);
                urlString.Append("sc_content", WebUtil.GetQueryString("sc_content"));
                Sitecore.Context.ClientPage.ClientResponse.ShowModalDialog(urlString.ToString(), width, height, string.Empty, true);
                args.WaitForPostBack();
            }
        }

        protected override bool LoadPostData(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            bool flag = base.LoadPostData(value);
            if (flag)
            {
                if (string.IsNullOrEmpty(this.Value))
                {
                    this.ClearLink();
                    return true;
                }
                Sitecore.Shell.Applications.ContentEditor.XmlValue xmlValue = this.XmlValue;
                if (this.GetLinkPath() == this.Value)
                {
                    return flag;
                }
                if (xmlValue.GetAttribute("linktype").Length == 0)
                {
                    xmlValue.SetAttribute("linktype", (this.Value.IndexOf("://", StringComparison.InvariantCulture) >= 0) ? "external" : "internal");
                }
                string str2 = string.Empty;
                if (xmlValue.GetAttribute("linktype") == "internal")
                {
                    string path = string.Empty;
                    if (this.Value.EndsWith("." + "aspx"))
                    {
                        if (this.Value.StartsWith("/sitecore/media library"))
                        {
                            path = this.Value.Remove(this.Value.Length - ("." + "aspx").Length);
                        }
                        else
                        {
                            path = "/sitecore/content" + this.Value.Remove(this.Value.Length - ("." + "aspx").Length);
                        }
                    }
                    Item item = Client.ContentDatabase.GetItem(path);
                    if (item != null)
                    {
                        str2 = item.ID.ToString();
                    }
                }
                else if (xmlValue.GetAttribute("linktype") == "media")
                {
                    string str4 = "/sitecore/media library" + this.Value;
                    Item item2 = Client.ContentDatabase.GetItem(str4);
                    if (item2 != null)
                    {
                        str2 = item2.ID.ToString();
                    }
                }
                else
                {
                    xmlValue.SetAttribute("url", this.Value);
                }
                if (!string.IsNullOrEmpty(str2))
                {
                    xmlValue.SetAttribute("id", str2);
                }
                this.XmlValue = xmlValue;
            }
            return flag;
        }

        protected override void OnPreRender(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            base.OnPreRender(e);
            base.ServerProperties["Value"] = base.ServerProperties["Value"];
            base.ServerProperties["XmlValue"] = base.ServerProperties["XmlValue"];
            base.ServerProperties["Source"] = base.ServerProperties["Source"];
        }

        protected override void SetModified()
        {
            base.SetModified();
            if (base.TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        private void SetValue()
        {
            string str = "";
            this.linkBroken = false;
            string attribute = this.XmlValue.GetAttribute("linktype");
            if (!string.IsNullOrEmpty(attribute))
            {
                string str3 = string.Empty;
                string str6 = attribute;
                if (str6 != null)
                {
                    if (str6 == "internal")
                    {
                        str3 = this.XmlValue.GetAttribute("id");
                        if (!string.IsNullOrEmpty(str3) && Sitecore.Data.ID.IsID(str3))
                        {
                            Item item = Client.ContentDatabase.GetItem(new ID(str3));
                            if (item == null)
                            {
                                this.linkBroken = true;
                                str = str3;
                            }
                            else
                            {
                                string path = item.Paths.Path;
                                if (path.StartsWith("/sitecore/content", StringComparison.InvariantCulture))
                                {
                                    path = path.Substring("/sitecore/content".Length);
                                }
                                if (LinkManager.AddAspxExtension)
                                {
                                    path = path + ("." + "aspx");
                                }
                                str = path;
                            }
                        }
                    }
                    else if (str6 == "media")
                    {
                        str3 = this.XmlValue.GetAttribute("id");
                        if (!string.IsNullOrEmpty(str3) && Sitecore.Data.ID.IsID(str3))
                        {
                            Item item2 = Client.ContentDatabase.GetItem(new ID(str3));
                            if (item2 == null)
                            {
                                this.linkBroken = true;
                                str = str3;
                            }
                            else
                            {
                                string str5 = item2.Paths.Path;
                                if (str5.StartsWith("/sitecore/media library", StringComparison.InvariantCulture))
                                {
                                    str5 = str5.Substring("/sitecore/media library".Length);
                                }
                                str = str5;
                            }
                        }
                    }
                }
            }
            if (str != "")
            {
                this.Value = str;
            }
            else
            {
                this.Value = this.XmlValue.GetAttribute("url");
            }
        }

        public override void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, "value");
            this.XmlValue = new Sitecore.Shell.Applications.ContentEditor.XmlValue(value, "link");
            this.SetValue();
        }

        private Sitecore.Shell.Applications.ContentEditor.XmlValue XmlValue
        {
            get
            {
                return
               new Sitecore.Shell.Applications.ContentEditor.XmlValue(base.GetViewStateString("XmlValue"), "link");
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                base.SetViewStateString("XmlValue", value.ToString());
            }
        }
    }
}
