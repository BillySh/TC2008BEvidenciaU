using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Estado
{
    public string id;
    public float[] state; // Utiliza un arreglo de float en lugar de Vector2.
}

[System.Serializable]
public class ListaEstados
{
    public Estado[] states;
}

//------------------------------------------------------------------------------------------------

public class semaforos : MonoBehaviour
{
    private Dictionary<string, GameObject> semaforoObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        for (int i = 457; i <= 492; i++)
        {
            string semaforoId = "semaforo_" + i;
            GameObject semaforoObject = GameObject.Find(semaforoId);
            if (semaforoObject != null)
            {
                semaforoObjects[semaforoId] = semaforoObject;
            }
        }
        StartCoroutine(GetAgentStates());
    }

    IEnumerator GetAgentStates()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:5000/agentState");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonString = www.downloadHandler.text;
                ListaEstados lightStates = JsonUtility.FromJson<ListaEstados>("{\"states\":" + jsonString + "}");
                foreach (Estado LS in lightStates.states)
                {
                    if (semaforoObjects.TryGetValue(LS.id, out GameObject semaforoObject))
                    {
                        if (semaforoObject == null)
                        {
                            Debug.LogError("El GameObject con ID " + LS.id + " fue encontrado en el diccionario pero es null");
                            continue; // Salta a la próxima iteración del bucle
                        }

                        if (LS.state != null && LS.state.Length == 3)
                        {
                            Transform verde = semaforoObject.transform.Find("verde");
                            Transform amarillo = semaforoObject.transform.Find("amarillo");
                            Transform rojo = semaforoObject.transform.Find("rojo");
                            switch (LS.state[0])
                            {
                                case 0:
                                    verde.gameObject.SetActive(false);
                                    amarillo.gameObject.SetActive(false);
                                    rojo.gameObject.SetActive(true);
                                    break;
                                case 1:
                                    verde.gameObject.SetActive(true);
                                    amarillo.gameObject.SetActive(false);
                                    rojo.gameObject.SetActive(false);
                                    break;
                                case 2:
                                    verde.gameObject.SetActive(false);
                                    amarillo.gameObject.SetActive(true);
                                    rojo.gameObject.SetActive(false);
                                    break;
                            }
                        }
                        else
                        {
                            Debug.LogError("Array de posición incorrecto para " + LS.id);
                        }
                    }
                }
            }
        }
    }
}
