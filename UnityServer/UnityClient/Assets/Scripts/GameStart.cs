using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Instance.Connect("127.0.0.1", 8888);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Instance.Update();
    }

    private void OnApplicationQuit()
    {
        NetManager.Instance.Close();
    }
}
