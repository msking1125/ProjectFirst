using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.OutGame.Data
{
    /// <summary>
    /// 湲덉튃??CSV ?곗씠?곕? 湲곕컲?쇰줈 ?댁떆?뗭쓣 援ъ꽦?섏뿬
    /// ?됰꽕????湲덉튃???ы븿 ?щ?瑜?鍮좊Ⅴ寃??먮퀎?⑸땲??
    /// </summary>
    public class BadWordData
    {
        private HashSet<string> badWordsSet;

        public BadWordData(TextAsset csvFile)
        {
            badWordsSet = new HashSet<string>();
            ParseCSV(csvFile);
        }

        private void ParseCSV(TextAsset csvFile)
        {
            if (csvFile == null) return;

            // ?쇱씤蹂?遺꾨━ (\r\n ?먮뒗 \n)
            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            // 泥?踰덉㎏ 以꾩씠 ?ㅻ뜑?쇰㈃ i=1遺???쒖옉 (?꾩옱???ㅻ뜑 ?녿떎怨?媛??
            for (int i = 0; i < lines.Length; i++)
            {
                string word = lines[i].Trim();
                if (!string.IsNullOrEmpty(word))
                {
                    badWordsSet.Add(word);
                }
            }
        }

        /// <summary>
        /// ?낅젰???띿뒪?몄뿉 湲덉튃?닿? ?ы븿?섏뼱 ?덈뒗吏 寃?ы빀?덈떎.
        /// (?꾩옱???뺥솗???쇱튂?? ?뱀? 遺遺??쇱튂???뺤옣 媛??
        /// </summary>
        public bool ContainsBadWord(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            
            // 遺遺??쇱튂瑜??먰븳?ㅻ㈃ 諛섎났臾몄쑝濡?Contains 寃??
            // ?뺥솗???⑥뼱 ?쇱튂?쇰㈃ Contains ???badWordsSet.Contains(text) ?ъ슜.
            // 蹂댄넻 ?됰꽕???꾪꽣??遺遺??ы븿??留됰뒗 寃쎌슦媛 留롮쑝誘濡?Contains 濡쒖쭅???ъ슜?⑸땲??
            foreach (var badWord in badWordsSet)
            {
                if (text.Contains(badWord))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
