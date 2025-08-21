using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstaclespawner : MonoBehaviour
{
    [Header("Prefab del obstáculo")]
    public GameObject obstaclePrefab;

    [Header("Tiempo entre spawns")]
    public float spawnInterval = 2f;

    [Header("Movimiento de obstáculos")]
    public float moveSpeed = 5f;
    public Vector3 startOffset = Vector3.zero;
    public float destroyXPosition = -15f;

    private float timer;
    private BoxCollider spawnArea3D;
    private BoxCollider2D spawnArea2D;

    void Awake()
    {
        // Intentar obtener BoxCollider 3D primero
        spawnArea3D = GetComponent<BoxCollider>();
        if (spawnArea3D != null)
        {
            spawnArea3D.isTrigger = true;
            return;
        }

        // Si no hay 3D, probar con 2D
        spawnArea2D = GetComponent<BoxCollider2D>();
        if (spawnArea2D != null)
        {
            spawnArea2D.isTrigger = true;
            return;
        }

        Debug.LogError("No se encontró BoxCollider ni BoxCollider2D en este GameObject. Añade uno para definir el área de spawn.");
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    void SpawnObstacle()
    {
        Vector3 spawnPos = Vector3.zero;

        if (spawnArea3D != null)
        {
            Vector3 size = spawnArea3D.size;
            Vector3 center = transform.position + spawnArea3D.center;

            float posX = transform.position.x; // Posición fija en X
            float posZ = transform.position.z; // Posición fija en Z
            float posY = Random.Range(center.y - size.y / 2, center.y + size.y / 2); // Posición aleatoria en Y

            spawnPos = new Vector3(posX, posY, posZ);
        }
        else if (spawnArea2D != null)
        {
            Vector2 size = spawnArea2D.size;
            Vector2 center = (Vector2)transform.position + spawnArea2D.offset;

            float posX = transform.position.x; // Posición fija en X
            float posZ = transform.position.z; // Posición fija en Z
            float posY = Random.Range(center.y - size.y / 2, center.y + size.y / 2); // Posición aleatoria en Y

            spawnPos = new Vector3(posX, posY, posZ);
        }
        else
        {
            return;
        }

        // Instanciar el obstáculo
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos + startOffset, Quaternion.identity);

        // Añadir script para moverlo hacia la izquierda
        ObstacleMover mover = obstacle.AddComponent<ObstacleMover>();
        mover.moveSpeed = moveSpeed;
        mover.destroyXPosition = destroyXPosition;
    }
}

public class ObstacleMover : MonoBehaviour
{
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public float destroyXPosition;

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);

        if (transform.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }
    }
}
