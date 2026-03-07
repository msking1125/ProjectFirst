using System.Collections.Generic;
using UnityEngine;

namespace ProjectFirst.OutGame.Data
{
    /// <summary>
    /// 금칙어 CSV 데이터를 기반으로 해시셋을 구성하여
    /// 닉네임 내 금칙어 포함 여부를 빠르게 판별합니다.
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

            // 라인별 분리 (\r\n 또는 \n)
            string[] lines = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            // 첫 번째 줄이 헤더라면 i=1부터 시작 (현재는 헤더 없다고 가정)
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
        /// 입력된 텍스트에 금칙어가 포함되어 있는지 검사합니다.
        /// (현재는 정확히 일치형, 혹은 부분 일치형 확장 가능)
        /// </summary>
        public bool ContainsBadWord(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            
            // 부분 일치를 원한다면 반복문으로 Contains 검사.
            // 정확한 단어 일치라면 Contains 대신 badWordsSet.Contains(text) 사용.
            // 보통 닉네임 필터는 부분 포함도 막는 경우가 많으므로 Contains 로직을 사용합니다.
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
