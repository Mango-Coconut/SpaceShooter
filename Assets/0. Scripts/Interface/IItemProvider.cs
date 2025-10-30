using System.Collections.Generic;

public interface IItemProvider
{
    // 한 번만 호출 (캐시 구성)
    void Initialize();

    // id로 조회
    bool TryGet(string id, out ItemData item);

    // 전체 나열(디버그/검증용)
    IEnumerable<string> GetAllIds();
}