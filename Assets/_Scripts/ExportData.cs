using System.IO;
using UnityEngine;

public class ExportData : MonoBehaviour
{
    public void ExportDataToJSON()
    {
        PlayerData data = new PlayerData();
        data.unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel");

        string json = JsonUtility.ToJson(data, true);

        // 3. Tạo đường dẫn file (Lưu vào thư mục AppData của Game)
        string path = Path.Combine(Application.persistentDataPath, "SaveData.json");

        // 4. Ghi file
        try
        {
            File.WriteAllText(path, json);
            Debug.Log("<color=green>Xuất JSON thành công tại: </color>" + path);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi xuất file JSON: " + e.Message);
        }
    }
}
