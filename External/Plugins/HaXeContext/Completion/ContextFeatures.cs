using System.Collections.Generic;

namespace HaXeContext.Completion
{
    public class ContextFeatures : ASCompletion.Completion.ContextFeatures
    {
        public string AbstractKey;
        public string MacroKey;

        protected override List<string> GetDeclarationKeywords(bool insideClass, string foundMember, List<string> result)
        {
            base.GetDeclarationKeywords(insideClass,foundMember, result);
            if (foundMember == "abstract")
            {
                result.Add("to");
                result.Add("from");
            }
            return result;
        }
    }
}