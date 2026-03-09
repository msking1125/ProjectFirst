using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

/// <summary>
/// 몬스터 데이터 테이블(인덱스 최적화 및 직관적인 구조)
/// </summary>
[CreateAssetMenu(menuName = "Game/Monster Table")]
public class MonsterTable : ScriptableObject
{
#if ODIN_INSPECTOR
    [TableList]
#endif
    public List<MonsterRow> rows = new();

    // ID, 이름 별로 MonsterRow를 빠르게 탐색하기 위한 인덱스
    private readonly Dictionary<int, MonsterRow> _idLookup = new();
    private readonly Dictionary<string, MonsterRow> _nameLookup = new();

    /// <summary>
    /// 입력 문자열을 인덱스 키로 정규화 (null/공백 -> null, 소문자)
    /// </summary>
    private static string NormalizeKey(string raw)
    {
        return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim().ToLowerInvariant();
    }

    private void OnEnable() => BuildIndex();
    private void OnValidate() => BuildIndex();

    /// <summary>
    /// 인덱스를 재구축. 모든 MonsterRow의 ID, 이름 별 참조를 구성.
    /// </summary>
    private void BuildIndex()
    {
        _idLookup.Clear();
        _nameLookup.Clear();
        if (rows == null) return;

        foreach (var row in rows)
        {
            if (row == null) continue;
            var nameKey = NormalizeKey(row.name);

            if (row.id > 0)
                _idLookup[row.id] = row;

            // 이름으로도 접근 가능하도록 인덱싱
            if (!string.IsNullOrEmpty(nameKey))
                _nameLookup[nameKey] = row;
        }
    }

    /// <summary>
    /// id로 몬스터 정보를 조회합니다. (Grade는 무시)
    /// </summary>
    public MonsterRow GetById(int id)
    {
        if (id <= 0)
            return null;

        // 인덱스가 최신인지 안전하게 한 번 더 BuildIndex 호출
        BuildIndex();

        return _idLookup.TryGetValue(id, out var row) ? row : null;
    }

    /// <summary>
    /// 이름으로 몬스터 정보를 조회합니다.
    /// </summary>
    public MonsterRow GetByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        BuildIndex();
        var key = NormalizeKey(name);
        return string.IsNullOrEmpty(key) ? null : _nameLookup.TryGetValue(key, out var row) ? row : null;
    }

    /// <summary>
    /// id와 grade가 모두 일치하는 몬스터를 반환합니다.
    /// 같은 id에 다른 grade가 있을 경우 정확히 일치하는 grade 우선, 없으면 baseRow 반환
    /// </summary>
    public MonsterRow GetByIdAndGrade(int id, MonsterGrade grade)
    {
        var baseRow = GetById(id);
        if (baseRow == null) return null;

        if (baseRow.grade == grade)
            return baseRow;

        if (rows == null) return baseRow;

        foreach (var row in rows)
        {
            if (row == null) continue;
            if (row.id == baseRow.id && row.grade == grade)
                return row;
        }
        return baseRow;
    }
}
