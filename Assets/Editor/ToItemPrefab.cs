using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class ToItemPrefab : Editor
{
    [MenuItem("Tools/Setup/Attach Item Components")]
    private static void AttachItemComponents()
    {
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // 프리팹 인스턴스 수정 시작
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            // Items 추가
            if (instance.GetComponent<Items>() == null)
            {
                instance.AddComponent<Items>();
            }

            // Collider 추가 (기본 BoxCollider)
            if (instance.GetComponent<Collider>() == null)
            {
                instance.AddComponent<BoxCollider>();
            }

            // Rigidbody 추가
            if (instance.GetComponent<Rigidbody>() == null)
            {
                var rb = instance.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.mass = 1f;
            }

            // 태그 설정 (없으면 자동 생성 가능)
            string tagName = "Items";
            if (!IsTagExists(tagName))
                AddTag(tagName);
            instance.tag = tagName;

            // 레이어 설정
            int layerIndex = LayerMask.NameToLayer("Items");
            if (layerIndex == -1)
            {
                Debug.LogWarning("⚠️ 'Items' 레이어가 존재하지 않음. 수동으로 Project Settings → Tags and Layers에서 만들어야 함.");
            }
            else
            {
                instance.layer = layerIndex;
            }

            // 변경 저장
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            GameObject.DestroyImmediate(instance);

            Debug.Log($"✅ {prefab.name} 세팅 완료");
        }
    }

    private static bool IsTagExists(string tag)
    {
        foreach (var t in InternalEditorUtility.tags)
            if (t == tag) return true;
        return false;
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // 이미 있는지 확인
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue.Equals(tag))
                return;
        }

        // 추가
        tagsProp.InsertArrayElementAtIndex(0);
        tagsProp.GetArrayElementAtIndex(0).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
