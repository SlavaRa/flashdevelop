using System.Collections.Generic;

namespace ASCompletion.Model
{
    public static class MemberListExtensions
    {
        /// <summary>
        /// Return all MemberModel instance matches in the MemberList
        /// </summary>
        /// <param name="this">Target collection</param>
        /// <param name="name">Member name to match</param>
        /// <returns>All matches</returns>
        public static IEnumerable<MemberModel> Where(this MemberList @this, string name)
        {
            var count = @this.Count;
            for (var i = 0; i < count; i++)
            {
                var model = @this[i];
                if (model.Name == name) yield return model;
            }
        }

        /// <summary>
        /// Return all MemberModel instance matches in the MemberList
        /// </summary>
        /// <param name="this">Target collection</param>
        /// <param name="mask">Flags mask to match</param>
        /// <returns>All matches</returns>
        public static IEnumerable<MemberModel> Where(this MemberList @this, FlagType mask)
        {
            var count = @this.Count;
            for (var i = 0; i < count; i++)
            {
                var model = @this[i];
                if ((model.Flags & mask) == mask) yield return model;
            }
        }

        /// <summary>
        /// Return all MemberModel instance matches in the MemberList
        /// </summary>
        /// <param name="this">Target collection</param>
        /// <param name="name">Member name to match</param>
        /// <param name="mask">Flags mask to match</param>
        /// <returns>All matches</returns>
        public static IEnumerable<MemberModel> Where(this MemberList @this, string name, FlagType mask)
        {
            var count = @this.Count;
            for (var i = 0; i < count; i++)
            {
                var model = @this[i];
                if ((model.Flags & mask) == mask && model.Name == name) yield return model;
            }
        }

        /// <summary>
        /// Return all MemberModel instance matches in the MemberList
        /// </summary>
        /// <param name="this">Target collection</param>
        /// <param name="name">Member name to match</param>
        /// <param name="mask">Flags mask to match</param>
        /// <param name="access">Visibility mask to match</param>
        /// <returns>All matches</returns>
        public static IEnumerable<MemberModel> Where(this MemberList @this, string name, FlagType mask, Visibility access)
        {
            var count = @this.Count;
            for (var i = 0; i < count; i++)
            {
                var model = @this[i];
                if ((model.Flags & mask) == mask
                    && (access == 0 || (model.Access & access) > 0)
                    && model.Name == name) yield return model;
            }
        }
    }
}