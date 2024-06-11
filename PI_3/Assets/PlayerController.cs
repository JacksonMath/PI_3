using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void Start()
    {
        // Verifica se a posi��o de spawn no lobby foi salva
        if (PlayerPrefs.HasKey("LobbySpawnPosX"))
        {
            float x = PlayerPrefs.GetFloat("LobbySpawnPosX");
            float y = PlayerPrefs.GetFloat("LobbySpawnPosY");
            float z = PlayerPrefs.GetFloat("LobbySpawnPosZ");

            // Define a posi��o do jogador
            transform.position = new Vector3(x, y, z);

            // Limpa os PlayerPrefs para n�o afetar futuras entradas no lobby
            PlayerPrefs.DeleteKey("LobbySpawnPosX");
            PlayerPrefs.DeleteKey("LobbySpawnPosY");
            PlayerPrefs.DeleteKey("LobbySpawnPosZ");
        }
    }
}
