using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProjectFirst.Data
{
    /// <summary>
    /// ?????以꾩쓽 ?곗씠?곕? ?섑??대뒗 吏곷젹??媛???대옒??
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
        public string dialogueId;

        public string groupId;

        public int orderInGroup;

        public string speakerName;

        public string text;

        public string backgroundKey;

        public string characterLKey;

        public string characterRKey;

        public string videoKey;

        public string choiceAText;

        public string choiceANext;

        public string choiceBText;

        public string choiceBNext;

        public string nextId;
    }

    /// <summary>
    /// ????곗씠???뚯씠釉?ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueTable", menuName = "MindArk/Data/DialogueTable")]
    public class DialogueTable : ScriptableObject
    {
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

