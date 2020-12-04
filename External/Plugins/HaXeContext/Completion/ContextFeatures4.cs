using System.Collections.Generic;

namespace HaXeContext.Completion
{
    public class ContextFeatures4 : ContextFeatures
    {
        protected override List<string> GetDeclarationKeywords(bool insideClass, string foundMember, List<string> result)
        {
            // for example: constKey == finalKey
            result.Remove(constKey);
            return base.GetDeclarationKeywords(insideClass,foundMember, result);
        }
    }
}