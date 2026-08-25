using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using ClaimCar.Sdk;
namespace ClaimCar.Web.Infrastructure
{
    public class ClaimExtensionHost
    {
        private readonly IList<IClaimExtension> _extensions;
        public ClaimExtensionHost(){_extensions=Load();}
        public ExtensionValidationResult Validate(ExtensionContext c){foreach(var e in _extensions){var r=e.Validate(c);if(r!=null&&!r.IsValid)return r;}return ExtensionValidationResult.Ok();}
        public void BeforeSave(ExtensionContext c){foreach(var e in _extensions)e.BeforeSave(c);}
        public void AfterSave(ExtensionContext c){foreach(var e in _extensions)e.AfterSave(c);}
        public void BeforeDelete(ExtensionContext c){foreach(var e in _extensions)e.BeforeDelete(c);}
        public void AfterDelete(ExtensionContext c){foreach(var e in _extensions)e.AfterDelete(c);}
        private static IList<IClaimExtension> Load(){var list=new List<IClaimExtension>();try{var configured=ConfigurationManager.AppSettings["ClaimSdk.PluginFolder"]??"~/App_Data/ClaimExtensions";var path=HttpContext.Current.Server.MapPath(configured);if(!Directory.Exists(path))Directory.CreateDirectory(path);foreach(var f in Directory.GetFiles(path,"*.dll")){try{var a=Assembly.LoadFrom(f);foreach(var t in a.GetTypes().Where(x=>typeof(IClaimExtension).IsAssignableFrom(x)&&!x.IsAbstract&&!x.IsInterface))list.Add((IClaimExtension)Activator.CreateInstance(t));}catch{}}}catch{}return list.OrderBy(x=>x.Order).ToList();}
    }
}
