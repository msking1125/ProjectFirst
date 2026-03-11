using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 챕터 데이터 테이블. CSV 임포트를 통해 데이터가 채워집니다.
/// </summary>
[CreateAssetMenu(menuName = "Game/Chapter Table")]
public class ChapterTable : ScriptableObject
{
    public List<ChapterRow> rows = new List<ChapterRow>();

    private Dictionary<int, ChapterRow> _index;

    private void OnEnable() => BuildIndex();
    private void OnValidate() => BuildIndex();

    private void BuildIndex()
    {
        _index = new Dictionary<int, ChapterRow>();
        if (rows == null) return;

        foreach (var row in rows)
        {
            if (row != null && row.id > 0)
                _index[row.id] = row;
        }
    }

    /// <summary>
    /// ID로 챕터를 조회합니다.
    /// </summary>
    public ChapterRow GetById(int id)
    {
        BuildIndex();
        return _index.TryGetValue(id, out var row) ? row : null;
    }

    /// <summary>
    /// 전체 챕터 목록을 ID 오름차순으로 반환합니다.
    /// </summary>
    public List<ChapterRow> GetAll() =>
        rows?.OrderBy(r => r.id).ToList() ?? new List<ChapterRow>();
}
