using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class posicionPeaton
{
    public string id;
    public float[] position; // Utiliza un arreglo de float en lugar de Vector2.
}

[System.Serializable]
public class listaPeatones
{
    public posicionPeaton[] positions;
}

public class peatones : MonoBehaviour
{
    private Dictionary<string, GameObject> objetosP = new Dictionary<string, GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i <= 19; i++)
        {
            string idP = "peaton_" + i;
            GameObject peatonObject = GameObject.Find(idP);
            if (peatonObject != null)
            {
                objetosP[idP] = peatonObject;
            }
            StartCoroutine(mover_peatones());
        }
    }
    IEnumerator mover_peatones()
    {
        UnityWebRequest www = new UnityWebRequest();
        while (true)
        {
            www = UnityWebRequest.Get("http://127.0.0.1:5000/agent_postionsCompas");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonString = www.downloadHandler.text;

                listaPeatones posiciones = JsonUtility.FromJson<listaPeatones>("{\"positions\":" + jsonString + "}");
                foreach (posicionPeaton peatonPos in posiciones.positions)
                {
                    if (objetosP.TryGetValue(peatonPos.id, out GameObject peatonObject))
                    {

                        if (peatonObject == null)
                        {
                            Debug.LogError("El GameObject con ID " + peatonPos.id + " fue encontrado en el diccionario pero es null");
                            continue;
                        }
                        else
                        {
                            // Comprueba que haya exactamente dos elementos en el arreglo de posición.
                            if (peatonPos.position != null && peatonPos.position.Length == 3)
                            {
                                // Asigna las posiciones x y z al transform del objeto.
                                float prevX = peatonObject.transform.position.x;
                                float prevZ = peatonObject.transform.position.z;
                                float newX = peatonPos.position[0]*10+5;
                                float newZ = peatonPos.position[1]*10+5;
                                if (prevX > newX)
                                {
                                    peatonObject.transform.rotation = Quaternion.Euler(0, 270, 0);
                                }
                                else if (prevX < newX)
                                {
                                    peatonObject.transform.rotation = Quaternion.Euler(0, 90, 0);
                                }
                                else if (prevZ > newZ)
                                {
                                    peatonObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                                }
                                else if (prevZ < newZ)
                                {
                                    peatonObject.transform.rotation = Quaternion.Euler(0, 0, 0);
                                }
                                peatonObject.transform.position = new Vector3(newX, 2, newZ);
                            }
                        }
                    }
                }
            }
        }
    }
}