using System.Linq;

namespace StringExtensions
{
    public static class StringExt
    {
        /// <summary>
        /// 將每個單詞轉成最多前四個字母，例如
        /// "Job_Profile_Data.Qualification_Replacement_Data".ToAbbr('.')
        /// 會回傳
        /// Job_Prof_Data.Qual_Repl_Data
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="stringLimit">單個字段最多幾個字，例如給2的話Qualification_Replacement_Data這層會變成Qu_Re_Da</param>
        /// <returns></returns>
        public static string ToAbbr(this string str, char splitChar, int stringLimit)
        {
            #region 特例處理Specified
            var specified = "Specified";
            if (str.EndsWith(specified))
                str = str.Substring(0, str.Length - specified.Length) + "_Specified";
            #endregion
            return "@" + string.Join(splitChar.ToString(), str.Split(splitChar).Select(x => {
                return string.Join("_", x.Split('_').Select(y => {
                    return y.Substring(0, y.Length < stringLimit ? y.Length : stringLimit);
                }).ToArray());
            }).ToArray());
        }
        /// <summary>
        /// 將每個單詞轉成最多後四個字母，例如
        /// "Job_Profile_Data.Qualification_Replacement_Data".ToAbbr('.')
        /// 會回傳
        /// Job_file_Data.tion_ment_Data
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="stringLimit">單個字段最多幾個字，例如給2的話Qualification_Replacement_Data這層會變成on_nt_ta</param>
        /// <returns></returns>
        public static string ToAbbrFromEnd(this string str, char splitChar, int stringLimit)
        {
            #region 特例處理Specified
            var specified = "Specified";
            if (str.EndsWith(specified))
                str = str.Substring(0, str.Length - specified.Length) + "_Specified";
            #endregion
            return "@" + string.Join(splitChar.ToString(), str.Split(splitChar).Select(x => {
                return string.Join("_", x.Split('_').Select(y => {
                    return y.Substring(y.Length < stringLimit ? 0 : y.Length - stringLimit,
                        y.Length < stringLimit ? y.Length : stringLimit);
                }).ToArray());
            }).ToArray());
        }
    }
}
