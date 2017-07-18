using System.Collections.Generic;
using Newtonsoft.Json;
using SiteServer.Plugin;
using SiteServer.Plugin.Hooks;

namespace SiteServer.Backup
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Config
    {
        public bool IsEnabled { get; set; }
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string BucketName { get; set; }
        public string BucketEndPoint { get; set; }
        public string BucketPath { get; set; }
        public bool IsInitSyncAll { get; set; }
    }

    public class Main : PluginBase, IRestful, IMenu
    {
        private IPublicApi _api;
        private readonly Dictionary<int, Config> _dict = new Dictionary<int, Config>();

        public override void Uninstall(PluginContext context)
        {
            var siteIds = _api.GetSiteIds();
            foreach (var siteId in siteIds)
            {
                _api.RemoveConfig(siteId, nameof(Config));
            }
        }

        public override void Active(PluginContext context)
        {
            _api = context.Api;

            var siteIds = _api.GetSiteIds();
            foreach (var siteId in siteIds)
            {
                var config = _api.GetConfig<Config>(siteId, nameof(Config));
                if (config == null) continue;

                _dict.Add(siteId, config);
            }
        }

        public override object Get(IRequestContext context, string name, int id)
        {
            return !_dict.ContainsKey(context.SiteId) ? null : _dict[context.SiteId];
        }

        public override object Post(IRequestContext context, string name, int id)
        {
            var config = new Config
            {
                IsEnabled = context.GetPostBool("isEnabled"),
                AccessKeyId = context.GetPostString("accessKeyId"),
                AccessKeySecret = context.GetPostString("accessKeySecret"),
                BucketName = context.GetPostString("bucketName"),
                BucketEndPoint = context.GetPostString("bucketEndPoint"),
                BucketPath = context.GetPostString("bucketPath"),
                IsInitSyncAll = context.GetPostBool("isInitSyncAll")
            };

            _api.SetConfig(context.SiteId, nameof(Config), config);
            _dict[context.SiteId] = config;

            return null;
        }

        public override PluginMenu GetSiteMenu(int siteId)
        {
            return new PluginMenu
            {
                Text = "站点备份",
                Menus = new List<PluginMenu>
                {
                    new PluginMenu
                    {
                        Text = "数据备份",
                        Href = "@/plugins/pageBackup.aspx"
                    },
                    new PluginMenu
                    {
                        Text = "数据恢复",
                        Href = "@/plugins/pageBackupRecovery.aspx"
                    }
                }
            };
        }
    }
}