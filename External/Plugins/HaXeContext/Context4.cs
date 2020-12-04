using System;
using System.Linq;
using ASCompletion.Completion;
using ASCompletion.Model;
using HaXeContext.Completion;
using PluginCore;
using ScintillaNet;
using ContextFeatures = ASCompletion.Completion.ContextFeatures;

namespace HaXeContext
{
    public class Context4 : Context
    {
        protected override ContextFeatures CreateContextFeatures() => new ContextFeatures4();

        public Context4(HaXeSettings initSettings, Func<InstalledSDK> getCurrentSdk, Func<string> getLanguageName) : base(initSettings, getCurrentSdk, getLanguageName)
        {
            features.constModifiers = Visibility.Public | Visibility.Private;
            features.hasConsts = true;
            features.constKey = "final";
            features.finalKey = "final";
            features.codeKeywords = features.codeKeywords.Concat(new[] {features.constKey}).ToArray();
            features.declKeywords = new[] {features.constKey}.Concat(features.declKeywords).ToArray();
            features.accessKeywords = features.accessKeywords.Concat(new[] {features.finalKey, "operator", "overload"}).ToArray();
        }

        internal override HaxeComplete GetHaxeComplete(ScintillaControl sci, ASExpr expression, bool autoHide, HaxeCompilerService compilerService)
        {
            var sdkVersion = GetCurrentSDKVersion();
            if (sdkVersion >= "4.0.0") return new HaxeComplete400(sci, expression, autoHide, completionModeHandler, compilerService, sdkVersion);
            return base.GetHaxeComplete(sci, expression, autoHide, compilerService);
        }
    }
}