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
            string path = AssetDatabase.GetAssetPath(obj);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // Prefab 인스턴스 생성 (수정용)
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            // Items 추가
            if (instance.GetComponent<Items>() == null)
            {
                instance.AddComponent<Items>();
            }

            // Collider 추가 (없으면 BoxCollider 기본)
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
            string tagName = "Item";
            if (!IsTagExists(tagName))
                AddTag(tagName);
            instance.tag = tagName;

            // 레이어 설정
            int layerIndex = LayerMask.NameToLayer("Item");
            if (layerIndex == -1)
            {
                Debug.LogWarning("⚠️ 'Item' 레이어가 존재하지 않음. 수동으로 Project Settings → Tags and Layers에서 만들어야 함.");
            }
            else
            {
                instance.layer = layerIndex;
            }

            // Prefab 저장
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            GameObject.DestroyImmediate(instance);

            Debug.Log($"✅ {prefab.name} 세팅 완료 (Rigidbody, Collider, ItemPickup, Tag=Item, Layer=Item)");
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
