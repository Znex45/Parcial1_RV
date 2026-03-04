using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    [Header("Orden libre de roles para esta partida")]
    public List<PlayerRole> rolesDisponibles = new List<PlayerRole>();

    private int siguienteRolIndex = 0;

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerController controller = playerInput.GetComponent<PlayerController>();

        if (controller != null)
        {
            if (siguienteRolIndex < rolesDisponibles.Count)
            {
                controller.SetRole(rolesDisponibles[siguienteRolIndex]);
                siguienteRolIndex++;
            }
            else
            {
                Debug.LogWarning("No hay más roles disponibles.");
            }
        }
    }
}