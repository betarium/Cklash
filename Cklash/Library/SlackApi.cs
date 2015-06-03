using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

/// zlib License
/// Copyright (c) 2015 Betarium
namespace Library
{
    /// <summary>
    /// https://api.slack.com/methods
    /// </summary>
    public class SlackApi
    {
        public class ChannelInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<string> Members { get; set; }
        }

        public class ChannelsHistoryResult
        {
            public bool Ok { get; set; }
            public string Latest { get; set; }
            public List<MessageInfo> Messages { get; set; }
            public bool HasMore { get; set; }
        }

        public class MessageInfo
        {
            public string Type { get; set; }
            public string Subtype { get; set; }
            public string User { get; set; }
            public string Username { get; set; }
            public string BotId { get; set; }
            public string Text { get; set; }
            public string Ts { get; set; }
            public string IconsImage48 { get; set; }
            public MessageIcon Icons { get; set; }
            public List<MessageAttachmentObject> Attachments { get; set; }
            public DateTime TsTimestamp { get; set; }

            public virtual void CopyFrom(MessageInfo source)
            {
                Type = source.Type;
                User = source.User;
                BotId = source.BotId;
                Username = source.Username;
                Text = source.Text;
                Ts = source.Ts;
                TsTimestamp = source.TsTimestamp;
                IconsImage48 = source.IconsImage48;
                Icons = source.Icons;
                Attachments = source.Attachments;
            }
        }

        public class MessageIcon
        {
            public string Image48 { get; set; }
            public string Image64 { get; set; }
        }

        public class MessageAttachmentObject
        {
            public string Title { get; set; }
            public string Text { get; set; }
        }

        public class IMObject
        {
            public string Id { get; set; }
            public string User { get; set; }
        }

        public class UserObject
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public UserProfileObject Profile { get; set; }
        }

        public class UserProfileObject
        {
            public string RealName { get; set; }
            public string Image24 { get; set; }
        }

        const string BASE_URL = "https://slack.com";

        public string ClientId { get; set; }
        public string ClientSecret { protected get; set; }
        public string AccessToken { get; set; }
        public string UserId { get; protected set; }
        public string Team { get; protected set; }

        public bool IsLogin { get { return !string.IsNullOrEmpty(UserId); } }

        public SlackApi()
        {
            InitApi();
        }

        public string GetAuthorizeUrl()
        {
            string authUrl = string.Format(BASE_URL + "/oauth/authorize?client_id={0}&scope=client&redirect_uri={1}", ClientId, "https://slack.com");
            return authUrl;
        }

        public void OpenAuthorizeUrl()
        {
            Process.Start(GetAuthorizeUrl());
        }

        public bool OauthAccess(string code)
        {
            Dictionary<string, string> paramMap = new Dictionary<string, string>();
            paramMap.Add("client_id", ClientId);
            paramMap.Add("client_secret", ClientSecret);
            paramMap.Add("code", code);

            string result = CallApi("/api/oauth.access", paramMap);
            JsonParser.JsonEntity json = JsonParser.Parse(result);
            if (!json.Entities.ContainsKey("access_token"))
            {
                return false;
            }

            AccessToken = json.Entities["access_token"].Value;

            if (!Auth.Test())
            {
                return false;
            }
            return true;
        }

        public AuthApi Auth { get; protected set; }
        public ChannelsApi Channels { get; protected set; }
        public ChatApi Chat { get; protected set; }
        public UsersApi Users { get; protected set; }
        public ImApi Im { get; protected set; }

        protected void InitApi()
        {
            Auth = new AuthApi(this);
            Channels = new ChannelsApi(this);
            Chat = new ChatApi(this);
            Users = new UsersApi(this);
            Im = new ImApi(this);
        }

        public class BaseApi
        {
            protected SlackApi Slack;

            public BaseApi(SlackApi slack)
            {
                Slack = slack;
            }
        }

        public class AuthApi : BaseApi
        {
            public AuthApi(SlackApi slack)
                : base(slack)
            {
            }

            public bool Test()
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);

                string result = Slack.CallApi("/api/auth.test", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(result);
                if (!json.Entities.ContainsKey("user_id"))
                {
                    return false;
                }
                Slack.UserId = json.Entities["user_id"].Value;
                Slack.Team = json.Entities["team"].Value;
                return true;
            }

        }

        public class ChannelsApi : BaseApi
        {
            public ChannelsApi(SlackApi slack)
                : base(slack)
            {
            }

            public List<ChannelInfo> List()
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);

                List<ChannelInfo> channelListTemp = new List<ChannelInfo>();
                string result = Slack.CallApi("/api/channels.list", paramMap);

                JsonParser.JsonEntity json = JsonParser.Parse(result);
                foreach (var item in json.Entities["channels"].Items)
                {
                    ChannelInfo info = new ChannelInfo();
                    info.Id = item.Entities["id"].Value;
                    info.Name = item.Entities["name"].Value;
                    info.Members = new List<string>();
                    foreach (var member in item.Entities["members"].Items)
                    {
                        info.Members.Add(member.Value);
                    }
                    channelListTemp.Add(info);
                }

                return channelListTemp;
            }

            public ChannelsHistoryResult History(string channel, string oldest = null)
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);
                paramMap.Add("channel", channel);
                if (oldest != null)
                {
                    paramMap.Add("oldest", channel);
                }

                string resultData = Slack.CallApi("/api/channels.history", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(resultData);

                ChannelsHistoryResult resultObj = new ChannelsHistoryResult();
                resultObj.Ok = (json.GetChildText("ok") == "true");
                resultObj.Latest = json.GetChildText("latest");
                resultObj.HasMore = (json.GetChildText("has_more") == "true");

                if (!resultObj.Ok)
                {
                    return resultObj;
                }

                List<MessageInfo> tempList = new List<MessageInfo>();
                long unixTimeTick = new DateTime(1970, 1, 1).Ticks;

                foreach (var item in json.Entities["messages"].Items)
                {
                    if (item.Entities.ContainsKey("subtype") && item.Entities["subtype"].Value == "channel_join")
                    {
                        continue;
                    }

                    string ts = item.Entities["ts"].Value;
                    string user = item.Entities.ContainsKey("user") ? item.Entities["user"].Value : null;
                    string username = item.Entities.ContainsKey("username") ? item.Entities["username"].Value : null;
                    string botId = item.Entities.ContainsKey("bot_id") ? item.Entities["bot_id"].Value : null;
                    string text = item.Entities.ContainsKey("text") ? item.Entities["text"].Value : null;
                    string image_48 = null;
                    MessageIcon iconInfo = null;
                    if (item.Entities.ContainsKey("icons"))
                    {
                        iconInfo = new MessageIcon();
                        var iconsItem = item.Entities["icons"];
                        if (iconsItem.Entities.ContainsKey("image_48"))
                        {
                            image_48 = iconsItem.Entities["image_48"].Value;
                            iconInfo.Image48 = image_48;
                        }
                        if (iconsItem.Entities.ContainsKey("image_64"))
                        {
                            iconInfo.Image64 = iconsItem.Entities["image_64"].Value;
                        }
                    }

                    List<MessageAttachmentObject> attachments = null;
                    if (item.Entities.ContainsKey("attachments"))
                    {
                        attachments = new List<MessageAttachmentObject>();
                        foreach (var item2 in item.Entities["attachments"].Items)
                        {
                            MessageAttachmentObject attatch = new MessageAttachmentObject();
                            if (item2.Entities.ContainsKey("title"))
                            {
                                attatch.Title = item2.Entities["title"].Value;
                            }
                            if (item2.Entities.ContainsKey("text"))
                            {
                                attatch.Text = item2.Entities["text"].Value;
                            }
                            attachments.Add(attatch);
                        }
                    }

                    SlackApi.MessageInfo messageInfo = new SlackApi.MessageInfo();
                    messageInfo.User = user;
                    messageInfo.Username = username;
                    messageInfo.BotId = botId;
                    messageInfo.Text = text;
                    messageInfo.Ts = ts;
                    messageInfo.IconsImage48 = image_48;
                    messageInfo.Icons = iconInfo;
                    long tsticks = unixTimeTick + long.Parse(ts.Split('.')[0]) * 10000000;
                    messageInfo.TsTimestamp = new DateTime(tsticks);
                    messageInfo.TsTimestamp += System.TimeZoneInfo.Local.BaseUtcOffset;
                    messageInfo.Attachments = attachments;

                    tempList.Add(messageInfo);
                }

                resultObj.Messages = tempList;
                return resultObj;
            }

        }

        public class ChatApi : BaseApi
        {
            public ChatApi(SlackApi slack)
                : base(slack)
            {
            }

            public bool PostMessage(string channel, string text, bool asUser = true)
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);
                paramMap.Add("channel", channel);
                paramMap.Add("text", text);
                paramMap.Add("as_user", asUser.ToString().ToLower());
                paramMap.Add("link_names", "1");

                string result = Slack.CallApi("/api/chat.postMessage", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(result);
                if (!json.Entities.ContainsKey("ok") || json.Entities["ok"].Value != "true")
                {
                    return false;
                }

                return true;
            }

        }

        public class UsersApi : BaseApi
        {
            public UsersApi(SlackApi slack)
                : base(slack)
            {
            }

            public UserObject Info(string id)
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);
                paramMap.Add("user", id);

                string result = Slack.CallApi("/api/users.info", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(result);

                if (json.GetChildText("ok") != "true")
                {
                    return null;
                }
                //UserObject info = new UserObject();

                //var item = json.Entities["user"];
                //info.Id = item.Entities["id"].Value;
                //info.Name = item.Entities["name"].Value;
                //info.RealName = item.Entities["profile"].Entities["real_name"].Value;
                //info.Image24 = item.Entities["profile"].GetChildText("image_24");

                var item = json.Entities["user"];
                UserObject info = ParseUser(item);

                return info;
            }

            public List<UserObject> List()
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);

                string result = Slack.CallApi("/api/users.list", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(result);

                var tempList = new List<UserObject>();
                foreach (var item in json.Entities["members"].Items)
                {
                    //UserObject info = new UserObject();

                    //info.Id = item.Entities["id"].Value;
                    //info.Name = item.Entities["name"].Value;
                    //info.RealName = item.Entities["profile"].Entities["real_name"].Value;
                    //info.Image24 = item.Entities["profile"].GetChildText("image_24");

                    UserObject info = ParseUser(item);
                    tempList.Add(info);
                }

                return tempList;
            }

            protected UserObject ParseUser(JsonParser.JsonEntity item)
            {
                UserObject info = new UserObject();

                info.Id = item.Entities["id"].Value;
                info.Name = item.Entities["name"].Value;
                info.Profile = new UserProfileObject();

                info.Profile.RealName = item.Entities["profile"].Entities["real_name"].Value;
                info.Profile.Image24 = item.Entities["profile"].GetChildText("image_24");

                return info;
            }
        }

        public class ImApi : BaseApi
        {
            public ImApi(SlackApi slack)
                : base(slack)
            {
            }

            public List<IMObject> List()
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);

                string result = Slack.CallApi("/api/im.list", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(result);

                long unixTimeTick = new DateTime(1970, 1, 1).Ticks;

                List<IMObject> tempList = new List<IMObject>();
                foreach (var item in json.Entities["ims"].Items)
                {
                    string id = item.Entities["id"].Value;
                    string user = item.Entities["user"].Value;

                    IMObject messageInfo = new IMObject();
                    messageInfo.Id = id;
                    messageInfo.User = user;

                    tempList.Add(messageInfo);
                }

                return tempList;
            }

            public List<MessageInfo> History(string channel)
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);
                paramMap.Add("channel", channel);

                string result = Slack.CallApi("/api/im.history", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(result);

                long unixTimeTick = new DateTime(1970, 1, 1).Ticks;

                List<MessageInfo> tempList = new List<MessageInfo>();
                foreach (var item in json.Entities["messages"].Items)
                {
                    string type = item.Entities["type"].Value;
                    string ts = item.Entities["ts"].Value;
                    string user = item.Entities.ContainsKey("user") ? item.Entities["user"].Value : null;
                    string text = item.Entities.ContainsKey("text") ? item.Entities["text"].Value : null;

                    MessageInfo messageInfo = new MessageInfo();
                    messageInfo.Type = type;
                    messageInfo.Ts = ts;
                    messageInfo.User = user;
                    messageInfo.Text = text;
                    long tsticks = unixTimeTick + long.Parse(ts.Split('.')[0]) * 10000000;
                    messageInfo.TsTimestamp = new DateTime(tsticks);
                    messageInfo.TsTimestamp += System.TimeZoneInfo.Local.BaseUtcOffset;

                    tempList.Add(messageInfo);
                }

                return tempList;
            }

            public bool Close(string channel)
            {
                Dictionary<string, string> paramMap = new Dictionary<string, string>();
                paramMap.Add("token", Slack.AccessToken);
                paramMap.Add("channel", channel);

                string result = Slack.CallApi("/api/im.close", paramMap);
                JsonParser.JsonEntity json = JsonParser.Parse(result);
                if (json.GetChildText("ok") == "true")
                {
                    return true;
                }
                return false;
            }

        }

        public string CallApi(string apiUrl, Dictionary<string, string> paramMap, bool getRequestFlag = true)
        {
            List<string> paramList = new List<string>();
            foreach (var param in paramMap)
            {
                paramList.Add(param.Key + "=" + Uri.EscapeUriString(param.Value));
            }
            string allParam = string.Join("&", paramList);
            Uri requestUri = new Uri(string.Format("{0}{1}?{2}", BASE_URL, apiUrl, allParam));
            if (getRequestFlag)
            {
                System.Diagnostics.Debug.WriteLine("slack request:" + requestUri);
            }
            else
            {
                requestUri = new Uri(string.Format("{0}{1}", BASE_URL, apiUrl));
                System.Diagnostics.Debug.WriteLine("slack request:" + requestUri + "?" + allParam);
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            if (!getRequestFlag)
            {
                request.Method = "POST";
                byte[] body = Encoding.UTF8.GetBytes(allParam);
                request.ContentLength = body.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(body, 0, body.Length);
                    stream.Flush();
                }
            }

            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseData = reader.ReadToEnd();
                    System.Diagnostics.Debug.WriteLine("slack response:" + responseData);
                    return responseData;
                }
            }
        }
    }
}
