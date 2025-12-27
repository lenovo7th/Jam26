using UnityEngine;
using UnityEditor;

public class EnemyPrefabCreator : EditorWindow
{
    [MenuItem("Tools/Create Enemy Prefab")]
    public static void CreateEnemyPrefab()
    {
        // Enemy GameObject oluştur
        GameObject enemy = new GameObject("Enemy");
        
        // Capsule mesh ekle (görsel)
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.SetParent(enemy.transform);
        capsule.transform.localPosition = Vector3.zero;
        capsule.name = "EnemyMesh";
        
        // Capsule'ın collider'ını kaldır (ana objeye ekleyeceğiz)
        DestroyImmediate(capsule.GetComponent<CapsuleCollider>());
        
        // Kırmızı materyal oluştur
        MeshRenderer renderer = capsule.GetComponent<MeshRenderer>();
        Material enemyMaterial = new Material(Shader.Find("Standard"));
        enemyMaterial.color = new Color(0.8f, 0.2f, 0.2f); // Kırmızı
        renderer.sharedMaterial = enemyMaterial;
        
        // Materyal'i kaydet
        string materialPath = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(materialPath))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        AssetDatabase.CreateAsset(enemyMaterial, materialPath + "/EnemyMaterial.mat");
        
        // Rigidbody ekle
        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 1f;
        
        // Capsule Collider ekle
        CapsuleCollider collider = enemy.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        collider.center = new Vector3(0, 1f, 0);
        
        // Enemy scriptleri ekle
        enemy.AddComponent<Enemy>();
        enemy.AddComponent<EnemyMovement>();
        enemy.AddComponent<EnemyAttack>();
        
        // Enemy tag oluştur ve ata
        enemy.tag = "Enemy";
        
        // Prefabs klasörü oluştur
        string prefabPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // Prefab olarak kaydet
        string localPath = prefabPath + "/Enemy.prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        
        PrefabUtility.SaveAsPrefabAsset(enemy, localPath);
        
        // Sahne objesini sil (prefab zaten kaydedildi)
        DestroyImmediate(enemy);
        
        // Asset database'i güncelle
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Enemy prefab oluşturuldu: {localPath}");
        EditorUtility.DisplayDialog("Başarılı!", $"Enemy prefab oluşturuldu:\n{localPath}", "Tamam");
        
        // Prefab'ı seç
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
    }
}
