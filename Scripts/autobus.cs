using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class posicionBus
{
    public string id;
    public float[] position; // Utiliza un arreglo de float en lugar de Vector2.
}

[System.Serializable]
public class listaPosicionesBus
{
    public posicionBus[] positions;
}

public class autobus : MonoBehaviour
{
    private Dictionary<string, GameObject> objetos = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        GameObject autobus = GameObject.Find("car_30");
        objetos["car_30"] = autobus;
        StartCoroutine(mover_autobus());
    }
    IEnumerator mover_autobus()
    {
        UnityWebRequest www = new UnityWebRequest();
        while (true)
        {
            www = UnityWebRequest.Get("http://127.0.0.1:5000/agent_postionsHelado");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonString = www.downloadHandler.text;

                listaPosicionesBus posiciones = JsonUtility.FromJson<listaPosicionesBus>("{\"positions\":" + jsonString + "}");
                foreach (posicionBus busPos in posiciones.positions)
                {
                    if (objetos.TryGetValue(busPos.id, out GameObject bus))
                    {
                        if (bus != null)
                        {
                            // Comprueba que haya exactamente dos elementos en el arreglo de posición.
                            if (busPos.position != null && busPos.position.Length == 2)
                            {
                                // Asigna las posiciones x y z al transform del objeto.
                                float prevX = bus.transform.position.x;
                                float prevZ = bus.transform.position.z;
                                if (prevX % 5 != 0)
                                {
                                    prevX = prevX - 3.5f;
                                    if (prevX % 5 != 0)
                                    {
                                        prevX = prevX + 7;
                                    }
                                }
                                if (prevZ % 5 != 0)
                                {
                                    prevZ = prevZ - 3.5f;
                                    if (prevZ % 5 != 0)
                                    {
                                        prevZ = prevZ + 7;
                                    }
                                }
                                float newX = busPos.position[0]*10+5;
                                float newZ = busPos.position[1]*10+5;
                                if (prevX > newX)
                                {
                                    bus.transform.rotation = Quaternion.Euler(0, 180, 0);
                                    bus.transform.position = new Vector3(newX + 3.5f, 3.4f, newZ);
                                }
                                else if (prevX < newX)
                                {
                                    bus.transform.rotation = Quaternion.Euler(0, 0, 0);
                                    bus.transform.position = new Vector3(newX - 3.5f, 3.4f, newZ);
                                }
                                else if (prevZ > newZ)
                                {
                                    bus.transform.rotation = Quaternion.Euler(0, 90, 0);
                                    bus.transform.position = new Vector3(newX, 3.4f, newZ + 3.5f);
                                }
                                else if (prevZ < newZ)
                                {
                                    bus.transform.rotation = Quaternion.Euler(0, 270, 0);
                                    bus.transform.position = new Vector3(newX, 3.4f, newZ - 3.5f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}