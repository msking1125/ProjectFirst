using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace ProjectFirst.Data
{
    /// <summary>
    /// ?????以꾩쓽 ?곗씠?곕? ?섑??대뒗 吏곷젹??媛???대옒??
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
#if ODIN_INSPECTOR
        [BoxGroup("?앸퀎")]
        [LabelText("???ID")]
#endif
        public string dialogueId;

#if ODIN_INSPECTOR
        [BoxGroup("?앸퀎")]
        [LabelText("洹몃９ ID")]
#endif
        public string groupId;

#if ODIN_INSPECTOR
        [BoxGroup("?앸퀎")]
        [LabelText("洹몃９ ???쒖꽌")]
#endif
        public int orderInGroup;

#if ODIN_INSPECTOR
        [BoxGroup("???)]
        [LabelText("?붿옄 ?대쫫")]
        [InfoBox("鍮꾩뼱?덉쑝硫??섎젅?댁뀡?쇰줈 ?쒖떆?⑸땲??", InfoMessageType.Info)]
#endif
        public string speakerName;

#if ODIN_INSPECTOR
        [BoxGroup("???)]
        [LabelText("????띿뒪??)]
        [TextArea(2, 5)]
#endif
        public string text;

#if ODIN_INSPECTOR
        [BoxGroup("?곗텧")]
        [LabelText("諛곌꼍 ??)]
#endif
        public string backgroundKey;

#if ODIN_INSPECTOR
        [BoxGroup("?곗텧")]
        [LabelText("醫뚯륫 罹먮┃??)]
#endif
        public string characterLKey;

#if ODIN_INSPECTOR
        [BoxGroup("?곗텧")]
        [LabelText("?곗륫 罹먮┃??)]
#endif
        public string characterRKey;

#if ODIN_INSPECTOR
        [BoxGroup("?곗텧")]
        [LabelText("而룹뵮 ?곸긽 ??)]
#endif
        public string videoKey;

#if ODIN_INSPECTOR
        [BoxGroup("?좏깮吏")]
        [LabelText("?좏깮吏 A ?띿뒪??)]
#endif
        public string choiceAText;

#if ODIN_INSPECTOR
        [BoxGroup("?좏깮吏")]
        [LabelText("?좏깮吏 A ?ㅼ쓬 ID")]
#endif
        public string choiceANext;

#if ODIN_INSPECTOR
        [BoxGroup("?좏깮吏")]
        [LabelText("?좏깮吏 B ?띿뒪??)]
#endif
        public string choiceBText;

#if ODIN_INSPECTOR
        [BoxGroup("?좏깮吏")]
        [LabelText("?좏깮吏 B ?ㅼ쓬 ID")]
#endif
        public string choiceBNext;

#if ODIN_INSPECTOR
        [BoxGroup("?먮쫫")]
        [LabelText("?ㅼ쓬 ???ID")]
        [InfoBox("?좏깮吏媛 ?놁쓣 ???ъ슜?⑸땲??")]
#endif
        public string nextId;
    }

    /// <summary>
    /// ????곗씠???뚯씠釉?ScriptableObject
    /// </summary>
#if ODIN_INSPECTOR
    [CreateAssetMenu(fileName = "DialogueTable", menuName = "Soul Ark/Data/DialogueTable")]
#else
    [CreateAssetMenu(fileName = "DialogueTable", menuName = "MindArk/Data/DialogueTable")]
#endif
    public class DialogueTable : ScriptableObject
    {
#if ODIN_INSPECTOR
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [Searchable]
#endif
        [SerializeField] private List<DialogueLine> _lines = new List<DialogueLine>();

        public IReadOnlyList<DialogueLine> Lines => _lines;

        private readonly Dictionary<string, DialogueLine> _idIndex = new();
        private readonly Dictionary<string, List<DialogueLine>> _groupIndex = new();

        private void OnEnable() => RebuildIndex();
        private void OnValidate() => RebuildIndex();

        /// <summary>
        /// dialogueId濡??????以꾩쓣 寃?됲빀?덈떎.
        /// </summary>
        public DialogueLine GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_idIndex.Count == 0) RebuildIndex();
            return _idIndex.TryGetValue(id, out DialogueLine line) ? line : null;
        }

        /// <summary>
        /// groupId???대떦?섎뒗 ???紐⑸줉??orderInGroup ?쒖꽌濡?諛섑솚?⑸땲??
        /// </summary>
        public List<DialogueLine> GetGroup(string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) return new List<DialogueLine>();
            if (_groupIndex.Count == 0) RebuildIndex();
            return _groupIndex.TryGetValue(groupId, out List<DialogueLine> list)
                ? new List<DialogueLine>(list)
                : new List<DialogueLine>();
        }

#if ODIN_INSPECTOR
        [Button("?몃뜳???ш뎄異?, ButtonSizes.Medium)]
        [GUIColor(0.3f, 0.8f, 0.3f)]
#endif
        private void RebuildIndex()
        {
            _idIndex.Clear();
            _groupIndex.Clear();
            if (_lines == null) return;

            for (int i = 0; i < _lines.Count; i++)
            {
                DialogueLine line = _lines[i];
                if (line == null) continue;

                if (!string.IsNullOrEmpty(line.dialogueId))
                {
                    _idIndex[line.dialogueId] = line;
                }

                if (!string.IsNullOrEmpty(line.groupId))
                {
                    if (!_groupIndex.ContainsKey(line.groupId))
                    {
                        _groupIndex[line.groupId] = new List<DialogueLine>();
                    }
                    _groupIndex[line.groupId].Add(line);
                }
            }

            foreach (var kvp in _groupIndex)
            {
                kvp.Value.Sort((a, b) => a.orderInGroup.CompareTo(b.orderInGroup));
            }
        }
    }
}

