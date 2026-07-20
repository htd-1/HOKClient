using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using GameLogic;

public static class FixPlayUIBindings
{
    [MenuItem("Tools/Fix PlayUI Bindings")]
    public static void Run()
    {
        string path = "Assets/AssetRaw/UI/Prefabs/PlayUI.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        UIBindComponent uiBind = prefab.GetComponent<UIBindComponent>();
        SerializedObject ser = new SerializedObject(uiBind);
        SerializedProperty prop = ser.FindProperty("m_components");

        // Build name → GameObject map
        var childMap = new System.Collections.Generic.Dictionary<string, GameObject>();
        foreach (Transform c in prefab.GetComponentsInChildren<Transform>(true))
            if (c != prefab.transform) childMap[c.name] = c.gameObject;

        // Fix indices 21/22/24: these should bind to Image, not RectTransform
        string[] names = { "m_img_skill1", "m_img_skill2", "m_img_skill3" };
        int[] idxs = { 21, 22, 24 };
        for (int i = 0; i < 3; i++)
        {
            if (childMap.ContainsKey(names[i]))
            {
                Image img = childMap[names[i]].GetComponent<Image>();
                if (img != null)
                    prop.GetArrayElementAtIndex(idxs[i]).objectReferenceValue = img;
            }
        }

        ser.ApplyModifiedProperties();
        PrefabUtility.SavePrefabAsset(prefab);
        AssetDatabase.Refresh();

        int ok = 0, nulls = 0;
        for (int i = 0; i < prop.arraySize; i++)
            if (prop.GetArrayElementAtIndex(i).objectReferenceValue != null) ok++; else nulls++;
        Debug.Log($"[Fix] PlayUI bindings: {ok}/{prop.arraySize} OK, {nulls} null");

        Debug.Log("[Fix] Complete! Now enter PlayMode and test F2 offline battle.");
    }

    [MenuItem("Tools/Verify All UIBindings")]
    public static void VerifyAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/AssetRaw/UI/Prefabs" });
        int totalNull = 0;
        foreach (string guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            UIBindComponent bind = go.GetComponent<UIBindComponent>();
            if (bind == null) continue;
            SerializedObject s = new SerializedObject(bind);
            SerializedProperty prop = s.FindProperty("m_components");
            int nulls = 0;
            for (int i = 0; i < prop.arraySize; i++)
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == null) nulls++;
            if (nulls > 0)
                Debug.LogWarning($"[Verify] {p}: {nulls} null bindings");
            totalNull += nulls;
        }
        if (totalNull == 0)
            Debug.Log("[Verify] All UI Prefab bindings OK! ✓");
    }
}
