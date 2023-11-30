using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class posicion
{
    public string id;
    public float[] position; // Utiliza un arreglo de float en lugar de Vector2.
}

[System.Serializable]
public class listaPosiciones
{
    public posicion[] positions;
}

public class coches : MonoBehaviour
{
    private Dictionary<string, GameObject> objetos = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 20; i <= 30; i++)
        {
            string id = "car_" + i.ToString("00");
            GameObject carObject = GameObject.Find(id);
            if (carObject != null)
            {
                objetos[id] = carObject;
            }
            StartCoroutine(mover_coches());
        }
    }
    IEnumerator mover_coches()
    {
        UnityWebRequest www = new UnityWebRequest();
        while (true)
        {
            www = UnityWebRequest.Get("http://127.0.0.1:5000/agent");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonString = www.downloadHandler.text;

                listaPosiciones posiciones = JsonUtility.FromJson<listaPosiciones>("{\"positions\":" + jsonString + "}");
                foreach (posicion carPos in posiciones.positions)
                {
                    if (objetos.TryGetValue(carPos.id, out GameObject carObject))
                    {
                        if (carObject != null)
                        {
                            // Comprueba que haya exactamente dos elementos en el arreglo de posición.
                            if (carPos.position != null && carPos.position.Length == 2)
                            {
                                // Asigna las posiciones x y z al transform del objeto.
                                float prevX = carObject.transform.position.x;
                                float prevZ = carObject.transform.position.z;
                                float newX = carPos.position[0]*10+5;
                                float newZ = carPos.position[1]*10+5;
                                if (prevX > newX)
                                {
                                    carObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                                }
                                else if (prevX < newX)
                                {
                                    carObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                                }
                                else if (prevZ > newZ)
                                {
                                    carObject.transform.rotation = Quaternion.Euler(0, 90, 0);
                                }
                                else if (prevZ < newZ)
                                {
                                    carObject.transform.rotation = Quaternion.Euler(0, 270, 0);
                                }
                                carObject.transform.position = new Vector3(newX, 2, newZ);
                            }
                        }
                    }
                }
            }
        }
    }
}