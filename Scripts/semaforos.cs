using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class LightState
{
    public string id;
    public float[] state; // Utiliza un arreglo de float en lugar de Vector2.
}

[System.Serializable]
public class LightStateList
{
    public LightState[] states;
}

//------------------------------------------------------------------------------------------------

public class semaforos : MonoBehaviour
{
    private Dictionary<string, GameObject> semaforoObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        DontDestroyOnLoad(gameObject); // Agrega esta línea al principio del método Start
        for (int i = 185; i <= 188; i++)
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
                LightStateList lightStates = JsonUtility.FromJson<LightStateList>("{\"states\":" + jsonString + "}");
                foreach (LightState LS in lightStates.states)
                {
                    Debug.Log("Procesando: " + LS.id + " en estado: " + LS.state[0]);  // Esto imprimirá cada posición procesada

                    if (semaforoObjects.TryGetValue(LS.id, out GameObject semaforoObject))
                    {
                        if (semaforoObject == null)
                        {
                            Debug.LogError("El GameObject con ID " + LS.id + " fue encontrado en el diccionario pero es null");
                            continue; // Salta a la próxima iteración del bucle
                        }

                        // Comprueba que haya exactamente dos elementos en el arreglo de posición.
                        if (LS.state != null && LS.state.Length == 1)
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
