using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Library;
using System.Drawing;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Cklash
{
    public class SlackApiCache : SlackApi
    {
        public Image DefaultUserIcon { get; set; }
        public ImageList UserIconList { get; set; }

        public string IconCacheDir { get; set; }

        private Dictionary<string, SlackApiCache.MessageCacheInfo> MessageCache = new Dictionary<string, SlackApiCache.MessageCacheInfo>();

        private Dictionary<string, UserCacheObject> userMap = new Dictionary<string, UserCacheObject>();

        private Dictionary<string, SlackApi.MessageInfo> directMessageMap = new Dictionary<string, MessageInfo>();
        private List<SlackApi.ChannelInfo> ChannelList = new List<SlackApi.ChannelInfo>();

        public SlackApiCache()
        {
            UserIconList = new ImageList();
            DefaultUserIcon = Properties.Resources.NoUserIcon;
            UserIconList.Images.Add(DefaultUserIcon);
        }

        public class MessageCacheInfo : MessageInfo
        {
            public UserCacheObject UserDetail { get; set; }
            public ChannelInfo ChannelDetail { get; set; }
            public string DisplayText { get; set; }
            public string DisplayRealName { get; set; }
            public string IconKey { get; set; }

            public MessageCacheInfo()
            {
            }

            public MessageCacheInfo(MessageInfo source)
            {
                //Type = info.Type;
                //User = info.User;
                //BotId = info.BotId;
                //Username = info.Username;
                //Text = info.Text;
                //Ts = info.Ts;
                //TsTimestamp = info.TsTimestamp;
                //IconsImage48 = info.IconsImage48;
                //Icons = info.Icons;
                //Attachments = info.Attachments;

                //DisplayText = info.Text;
                //DisplayRealName = info.Username;
                CopyFrom(source);
            }

            public override void CopyFrom(MessageInfo source)
            {
                base.CopyFrom(source);
                Username = source.Username;
                DisplayText = source.Text ?? "";
                DisplayRealName = source.Username;
            }
        }

        public class UserCacheObject : UserObject
        {
            public UserCacheObject()
            {
            }

            public UserCacheObject(UserObject info)
            {
                Id = info.Id;
                Name = info.Name;
                Profile = info.Profile;
            }

            public Image Icon { get; set; }
            public int IconIndex { get; set; }
            public string IconKey { get; set; }
        }

        protected static string MakeHash(string key)
        {
            byte[] sourceBytes = System.Text.Encoding.UTF8.GetBytes(key);
            sourceBytes = System.Security.Cryptography.MD5CryptoServiceProvider.Create().ComputeHash(sourceBytes);
            System.Text.StringBuilder hashBuf = new System.Text.StringBuilder();
            for (int i = 0; i < sourceBytes.Length; i++)
            {
                hashBuf.AppendFormat("{0:X2}", sourceBytes[i]);
            }
            return hashBuf.ToString();
        }

        public void LoadUsers()
        {
            List<SlackApi.UserObject> userList = Users.List();
            foreach (var item in userList)
            {
                System.Diagnostics.Debug.WriteLine("user: id=" + item.Id + " name=" + item.Name);
                userMap.Add(item.Id, new UserCacheObject(item));
            }
            foreach (var item in userMap.Values)
            {
                item.Icon = DefaultUserIcon;
                if (string.IsNullOrEmpty(item.Profile.Image24))
                {
                    continue;
                }

                Image image = LoadIcon(item.Profile.Image24, IconCacheDir);
                if (image == null)
                {
                    continue;
                }

                UserIconList.Images.Add(item.Profile.Image24, image);
                item.IconIndex = UserIconList.Images.Count - 1;
                item.IconKey = item.Profile.Image24;
                item.Icon = image;
            }
        }

        public static Image LoadIcon(string url, string IconCacheDir)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            string iconHash = MakeHash(url);
            string iconHashPath = null;
            if (!string.IsNullOrEmpty(IconCacheDir))
            {
                iconHashPath = Path.Combine(IconCacheDir, iconHash);
                if (File.Exists(iconHashPath))
                {
                    try
                    {
                        Image image = Image.FromFile(iconHashPath);
                        return image;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                }
            }

            try
            {
                WebRequest req = WebRequest.Create(url);
                using (WebResponse res = req.GetResponse())
                {
                    Stream st = res.GetResponseStream();
                    byte[] buf = new byte[res.ContentLength];
                    int index = 0;
                    while (index < buf.Length)
                    {
                        int read = st.Read(buf, index, buf.Length - index);
                        if (read < 0)
                        {
                            break;
                        }
                        index += read;
                    }
                    if (!string.IsNullOrEmpty(IconCacheDir))
                    {
                        File.WriteAllBytes(iconHashPath, buf);
                    }
                    MemoryStream stream2 = new MemoryStream();
                    stream2.Write(buf, 0, buf.Length);
                    stream2.Seek(0, SeekOrigin.Begin);
                    Image image = Image.FromStream(stream2);
                    return image;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            return null;
        }

        public UserCacheObject GetUser(string id)
        {
            if (id == null)
            {
                return null;
            }
            if (userMap.ContainsKey(id))
            {
                return userMap[id];
            }

            SlackApi.UserObject info = Users.Info(id);
            if (info == null)
            {
                return null;
            }

            var info2 = new UserCacheObject(info);
            info2.Icon = DefaultUserIcon;
            userMap.Add(info.Id, info2);
            return info2;
        }

        public int LoadDirectMessage()
        {
            List<SlackApi.IMObject> imlist = Im.List();
            List<SlackApi.MessageInfo> dmlist = new List<SlackApi.MessageInfo>();
            foreach (var imobj in imlist)
            {
                List<SlackApi.MessageInfo> list = Im.History(imobj.Id);
                dmlist.AddRange(list);
            }

            int newCount = 0;
            foreach (var item in dmlist)
            {
                if (directMessageMap.ContainsKey(item.Ts))
                {
                    continue;
                }
                directMessageMap.Add(item.Ts, item);
                newCount++;
            }
            return newCount;
        }

        public List<SlackApi.ChannelInfo> ReloadChannel()
        {
            if (!IsLogin)
            {
                return null;
            }

            string userId = UserId;

            List<SlackApi.ChannelInfo> channelList1 = Channels.List();
            List<SlackApi.ChannelInfo> channelListTemp = new List<SlackApi.ChannelInfo>();

            foreach (var info in channelList1)
            {
                if (!info.Members.Contains(userId))
                {
                    continue;
                }
                channelListTemp.Add(info);
                System.Diagnostics.Debug.WriteLine("channels:" + info.Id + "=" + info.Name);
            }

            ChannelList.Clear();
            ChannelList.AddRange(channelListTemp);

            return channelListTemp;
        }

        public List<SlackApiCache.MessageCacheInfo> LoadChannelHistory()
        {
            List<SlackApiCache.MessageCacheInfo> tempList = new List<SlackApiCache.MessageCacheInfo>();

            foreach (var channel in ChannelList)
            {
                if (channel.Id == null)
                {
                    continue;
                }
                SlackApi.ChannelsHistoryResult resultObj = Channels.History(channel.Id);
                if (!resultObj.Ok)
                {
                    continue;
                }

                List<SlackApi.MessageInfo> tempList2 = resultObj.Messages;

                foreach (var item in tempList2)
                {
                    string ts = item.Ts;
                    if (MessageCache.ContainsKey(ts))
                    {
                        continue;
                    }

                    SlackApiCache.UserCacheObject user = null;
                    if (item.User != null)
                    {
                        user = GetUser(item.User);
                    }

                    SlackApiCache.MessageCacheInfo item2 = new SlackApiCache.MessageCacheInfo(item);
                    item2.UserDetail = user;
                    item2.ChannelDetail = channel;
                    MessageCache.Add(item.Ts, item2);

                    tempList.Add(item2);
                }
            }

            Regex regex = new Regex("<@([a-z0-9]+)([^>]*)>", RegexOptions.IgnoreCase);
            foreach (var item in tempList)
            {
                string text = item.DisplayText;
                if (text == null && item.Attachments != null)
                {
                    item.DisplayText = item.Attachments[0].Title + " " + item.Attachments[0].Text;
                }
                if (item.User != null)
                {
                    var user = GetUser(item.User);
                    if (user != null)
                    {
                        item.DisplayRealName = user.Profile.RealName;
                    }
                }
                if (item.UserDetail != null && item.UserDetail.Icon != null)
                {
                    item.IconKey = item.UserDetail.IconKey;
                }
                else if (item.UserDetail == null && item.Icons != null)
                {
                    string iconUrl = item.Icons.Image48;
                    if (string.IsNullOrEmpty(iconUrl))
                    {
                        iconUrl = item.Icons.Image64;
                    }

                    if (!string.IsNullOrEmpty(iconUrl))
                    {
                        if (UserIconList.Images.ContainsKey(iconUrl))
                        {
                            item.IconKey = iconUrl;
                        }
                        else
                        {
                            Image icon = LoadIcon(iconUrl, IconCacheDir);
                            if (icon != null)
                            {
                                UserIconList.Images.Add(iconUrl, icon);
                                item.IconKey = iconUrl;
                            }
                        }
                    }

                    //if (UserIconList.Images.ContainsKey(item.IconsImage48))
                    //{
                    //    item.IconKey = item.IconsImage48;
                    //}
                    //else
                    //{
                    //    Image icon = LoadIcon(item.IconsImage48, IconCacheDir);
                    //    if (icon != null)
                    //    {
                    //        UserIconList.Images.Add(item.IconsImage48, icon);
                    //        item.IconKey = item.IconsImage48;
                    //    }
                    //}
                }

                if (item.DisplayText == null)
                {
                    continue;
                }

                MatchCollection matches = regex.Matches(item.DisplayText);
                if (matches != null && matches.Count > 0)
                {
                    string displayText2 = "";
                    int index = 0;
                    foreach (Match match in matches)
                    {
                        string userId = match.Groups[1].Value;
                        string userName = null;
                        var user = GetUser(userId);
                        if (user != null)
                        {
                            userName = user.Profile.RealName;
                        }
                        displayText2 += item.DisplayText.Substring(index, match.Groups[0].Index - index);
                        displayText2 += "@" + userName;
                        index = match.Groups[0].Index + match.Groups[0].Length;
                        //= item.DisplayText.Substring(0, match.Groups[0].Index) + "@" + userName + item.DisplayText.Substring(match.Groups[0].Index + match.Groups[0].Length);                        
                    }
                    displayText2 += item.DisplayText.Substring(index);
                    item.DisplayText = displayText2;
                }
            }
            return tempList;
        }

        public void SaveMessageCache()
        {
            JsonParser.JsonEntity list = new JsonParser.JsonEntity(JsonParser.JsonEntity.JsonType.Array);
            foreach (var message in MessageCache.Values)
            {
                JsonParser.JsonEntity entity = new JsonParser.JsonEntity(JsonParser.JsonEntity.JsonType.Pear);
                var field = new JsonParser.JsonEntity(JsonParser.JsonEntity.JsonType.Integer);
                field.Value = message.Ts;
                entity.Entities.Add("ts", field);
                list.Items.Add(entity);
            }

            StringBuilder buffer = new StringBuilder();
            buffer.Append("[");
            int cnt = 0;
            foreach (var entity in list.Items)
            {
                buffer.Append("{");
                foreach (var field in entity.Entities)
                {
                    buffer.Append("\"" + field.Key + "\"");
                    buffer.Append(":");
                    buffer.Append(field.Value.Value);
                }
                buffer.Append("}");
                if (++cnt < list.Items.Count)
                {
                    buffer.Append(",");
                }
            }
            buffer.Append("]");

            //string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //dir = Path.Combine(dir, Application.ProductName);
            //if (!Directory.Exists(dir))
            //{
            //    Directory.CreateDirectory(dir);
            //}
            string dir = Application.StartupPath;
            string path = Path.Combine(dir, "message.json");
            File.WriteAllText(path, buffer.ToString(), Encoding.Default);
        }
    }
}
