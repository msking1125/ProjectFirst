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
    /// 대화 한 줄의 데이터를 나타내는 직렬화 가능 클래스
    /// </summary>
    [Serializable]
    public class DialogueLine
    {
#if ODIN_INSPECTOR
        [BoxGroup("식별")]
        [LabelText("대화 ID")]
#endif
        public string dialogueId;

#if ODIN_INSPECTOR
        [BoxGroup("식별")]
        [LabelText("그룹 ID")]
#endif
        public string groupId;

#if ODIN_INSPECTOR
        [BoxGroup("식별")]
        [LabelText("그룹 내 순서")]
#endif
        public int orderInGroup;

#if ODIN_INSPECTOR
        [BoxGroup("대사")]
        [LabelText("화자 이름")]
        [InfoBox("비어있으면 나레이션으로 표시됩니다.", InfoMessageType.Info)]
#endif
        public string speakerName;

#if ODIN_INSPECTOR
        [BoxGroup("대사")]
        [LabelText("대사 텍스트")]
        [TextArea(2, 5)]
#endif
        [TextArea(3, 10)]
        public string text;

#if ODIN_INSPECTOR
        [BoxGroup("연출")]
        [LabelText("배경 키")]
#endif
        public string backgroundKey;

#if ODIN_INSPECTOR
        [BoxGroup("연출")]
        [LabelText("좌측 캐릭터")]
#endif
        public string characterLKey;

#if ODIN_INSPECTOR
        [BoxGroup("연출")]
        [LabelText("우측 캐릭터")]
#endif
        public string characterRKey;

#if ODIN_INSPECTOR
        [BoxGroup("연출")]
        [LabelText("컷씬 영상 키")]
#endif
        public string videoKey;

#if ODIN_INSPECTOR
        [BoxGroup("선택지")]
        [LabelText("선택지 A 텍스트")]
#endif
        public string choiceAText;

#if ODIN_INSPECTOR
        [BoxGroup("선택지")]
        [LabelText("선택지 A 다음 ID")]
#endif
        public string choiceANext;

#if ODIN_INSPECTOR
        [BoxGroup("선택지")]
        [LabelText("선택지 B 텍스트")]
#endif
        public string choiceBText;

#if ODIN_INSPECTOR
        [BoxGroup("선택지")]
        [LabelText("선택지 B 다음 ID")]
#endif
        public string choiceBNext;

#if ODIN_INSPECTOR
        [BoxGroup("흐름")]
        [LabelText("다음 대화 ID")]
        [InfoBox("선택지가 없을 때 사용됩니다.")]
#endif
        public string nextId;
    }

    /// <summary>
    /// 대화 데이터 테이블 ScriptableObject
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
        /// dialogueId로 대화 한 줄을 검색합니다.
        /// </summary>
        public DialogueLine GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_idIndex.Count == 0) RebuildIndex();
            return _idIndex.TryGetValue(id, out DialogueLine line) ? line : null;
        }

        /// <summary>
        /// groupId에 해당하는 대화 목록을 orderInGroup 순서로 반환합니다.
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
        [Button("인덱스 재구축", ButtonSizes.Medium)]
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
