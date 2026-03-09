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
            new Entry("Agent Table", AgentTableImporter.Import),
            new Entry("Monster Table", MonsterTableImporter.Import),
            new Entry("Skill Table", SkillTableImporter.Import),
            new Entry("Wave Table", WaveTableImporter.Import),
            new Entry("Dialogue Table", DialogueTableImporter.Import),
            new Entry("Badword Table", BadwordTableImporter.Import),
        };
    }
}
#endif
