using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using System.Web.Http;

/// <summary>
/// Summary description for Documentation
/// </summary>
namespace GrowCreate.Plugins
{
    public class Docs
    {
        public int nodeId { get; set; }
        public string name { get; set; }
        public string alias { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public string type { get; set; }
        public int count { get; set; }
        public IEnumerable<Docs> properties { get; set; }
    }

    public class SiteDetails
    {
        public string SiteName { get; set; }
        public string SiteLogo { get; set; }
    }

    [PluginController("GrowCreate")]
    public class DocumentoController : UmbracoApiController
    {
        public IEnumerable<Docs> GetDocs()
        {
            var db = ApplicationContext.Current.DatabaseContext.Database;
            var types = db.Query<Docs>("select c.alias, c.nodeId, c.icon, c.description, n.text as name from cmsContentType c, umbracoNode n where c.nodeId = n.id and c.description <> ''").ToList().OrderBy(x => x.name);

            foreach (var type in types)
            {
                var _props = new List<Docs>();
                var props = db.Query<Docs>("select t.name, t.description, n.text as type from cmsPropertyType t, umbracoNode n where t.contentTypeId = @0 and t.dataTypeId = n.id", type.nodeId).ToList();
                foreach (var prop in props)
                {
                    _props.Add(new Docs()
                    {
                        name = prop.name,
                        type = prop.type,
                        description = prop.description
                    });
                }
                type.properties = _props;
                type.count = umbraco.uQuery.GetNodesByType(type.alias).Count();
            }
            return types;
        }

        public SiteDetails GetSiteDetails()
        {
            var helper = new Umbraco.Web.UmbracoHelper(UmbracoContext.Current);
            var root = helper.TypedContentAtRoot().FirstOrDefault();

            return new SiteDetails()
            {
                SiteName = root == null || !root.HasProperty("siteName") ? "" : root.GetProperty("siteName").Value.ToString(),
                SiteLogo = root == null || !root.HasProperty("siteLogo") ? "" : Umbraco.TypedMedia(root.GetProperty("siteLogo").Value.ToString()).Url
            };
        }
    }
}