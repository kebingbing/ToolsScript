using UnityEngine;
using System.Collections;
using System.Text;

public class MemoryManager : MonoBehaviour {

    /// <summary>
    /// 内存阈值
    /// </summary>
#if UNITY_EDITOR
    private int MemThreshold = 500;
#else
    private int MemThreshold = 250;
#endif
    /// <summary>
    /// 一次释放ab最大个数
    /// </summary>
    public int OneTimeFreeABNum = 5;

   public int OneTimeFreeAssetNum = 5;

    public uint OneMbtyes = 1024 * 1024;

    //一秒更新一次 一秒30帧
    private int OneScecondRate = 30;

    StringBuilder sb = new StringBuilder();

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if(OneScecondRate <= 0)
        {
            uint TotalAllocatedMemory = Profiler.GetTotalAllocatedMemory() / OneMbtyes;
            if (TotalAllocatedMemory > MemThreshold)
            {
                ResourceManager.GetInstance().ClearUnuseCache(OneTimeFreeABNum, OneTimeFreeAssetNum);
            }
            OneScecondRate = 30;
        }

        OneScecondRate--;


    }
#if UNITY_EDITOR
    //void OnGUI()
    //{
    //    sb.Append("MonoHeapSize:");
    //    sb.Append(Profiler.GetMonoHeapSize() / OneMbtyes);
    //    sb.Append("\n");

    //    sb.Append("MonoUsedSize:");
    //    sb.Append(Profiler.GetMonoUsedSize() / OneMbtyes);
    //    sb.Append("\n");

    //    sb.Append("TotalAllocatedMemory:");
    //    sb.Append(Profiler.GetTotalAllocatedMemory() / OneMbtyes);
    //    sb.Append("\n");

    //    sb.Append("TotalReservedMemory:");
    //    sb.Append(Profiler.GetTotalReservedMemory() / OneMbtyes);
    //    sb.Append("\n");

    //    sb.Append("TotalUnusedReservedMemory:");
    //    sb.Append(Profiler.GetTotalUnusedReservedMemory() / OneMbtyes);
    //    sb.Append("\n");
    //    GUI.skin.label.normal.textColor = Color.black;
    //    GUI.skin.label.fontSize = 20;
    //    GUILayout.Label(sb.ToString(),GUILayout.Width(300),GUILayout.Height(200));
    //    sb.Remove(0,sb.Length);
    //}
#endif
}
