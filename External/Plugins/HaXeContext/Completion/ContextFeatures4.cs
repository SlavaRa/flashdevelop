using System.Collections.Generic;

namespace HaXeContext.Completion
{
    public class ContextFeatures4 : ContextFeatures
    {
        protected override void GetDeclarationKeywords(string foundMember, List<string> result)
        {
            // for example: constKey == finalKey
            result.Remove(constKey);
            base.GetDeclarationKeywords(foundMember, result);
        }
    }
}