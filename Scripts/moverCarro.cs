using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class CarPosition
{
    public string id;
    public float[] position; // Utiliza un arreglo de float en lugar de Vector2.
}

[System.Serializable]
public class CarPositionList
{
    public CarPosition[] positions;
}

public class moverCarro : MonoBehaviour
{   
    private Dictionary<string, GameObject> carObjects = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject); // Agrega esta línea al principio del método Start
        for (int i = 0; i <= 5; i++)
        {
            string carId = "car_" + i;
            GameObject carObject = GameObject.Find(carId);
            if (carObject != null)
            {
                carObjects[carId] = carObject;
            }
        }

        StartCoroutine(GetAgentPositions());
    }
    IEnumerator GetAgentPositions()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:5000/agent");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                string jsonString = www.downloadHandler.text;

                CarPositionList carPositions = JsonUtility.FromJson<CarPositionList>("{\"positions\":" + jsonString + "}");
                foreach (CarPosition carPos in carPositions.positions)
                {

                    if (carObjects.TryGetValue(carPos.id, out GameObject carObject))
                    {
                        if (carObject == null)
                        {
                            Debug.LogError("El GameObject con ID " + carPos.id + " fue encontrado en el diccionario pero es null");
                            continue; // Salta a la próxima iteración del bucle
                        }

                        // Comprueba que haya exactamente dos elementos en el arreglo de posición.
                        if (carPos.position != null && carPos.position.Length == 2)
                        {
                            // Asigna las posiciones x y z al transform del objeto.
                            float prevX = carObject.transform.position.x;
                            float prevZ = carObject.transform.position.z;
                            float newX = carPos.position[1]*-10+85;
                            float newZ = carPos.position[0]*10-65;
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
                            carObject.transform.position = new Vector3(newX, 1.6f, newZ);
                        }
                        else
                        {
                            Debug.LogError("Array de posición incorrecto para " + carPos.id);
                        }
                    }
                    else
                    {
                        Debug.LogError("No se encontró el GameObject para " + carPos.id);
                    }
                }
            }

            
        }
    }
    // Update is called once per frame
    /*
    void Update()
    {
        if (directions[0] == "forward")
        {   
            transform.Translate(10, 0, 0);
        }
        else
        {
            float x = transform.position.x;
            float z = transform.position.z;
            int newX = (int) Math.Round(x / 5, 0);
            int newZ = (int) Math.Round(z / 5, 0);
            transform.position = new Vector3(newX * 5, transform.position.y, newZ * 5);
            if (directions[0] == "right")
            {
                transform.Rotate(0, 90, 0);
                transform.Translate(10, 0, 1.5f);
            }
            else if (directions[0] == "left")
            {
                transform.Rotate(0, -90, 0);
                transform.Translate(10, 0, -1.5f);
            }
        }
        directions = directions.Skip(1).ToArray();
        Thread.Sleep(300);
    }
    */
}
