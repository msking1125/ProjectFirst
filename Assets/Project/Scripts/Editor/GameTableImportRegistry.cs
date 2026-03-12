#if UNITY_EDITOR
using System;
using System.Collections.Generic;

internal static class GameTableImportRegistry
{
    internal readonly struct Entry
    {
        public readonly string Label;
        public readonly Action ImportAction;

        public Entry(string label, Action importAction)
        {
            Label = label;
            ImportAction = importAction;
        }
    }

    public static IReadOnlyList<Entry> GetEntries()
    {
        return new[]
        {
            new Entry("에이전트 테이블", AgentTableImporter.Import),
            new Entry("몬스터 테이블", MonsterTableImporter.Import),
            new Entry("스킬 테이블", SkillTableImporter.Import),
            new Entry("웨이브 테이블", WaveTableImporter.Import),
            new Entry("챕터 테이블", ChapterTableImporter.Import),
            new Entry("스테이지 테이블", StageTableImporter.Import),
            new Entry("대사 테이블", DialogueTableImporter.Import),
            new Entry("금칙어 테이블", BadwordTableImporter.Import),
        };
    }
}
#endif

