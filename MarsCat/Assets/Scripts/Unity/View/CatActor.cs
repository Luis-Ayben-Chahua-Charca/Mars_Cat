using UnityEngine;

namespace MarsCat.Unity.View
{
    /// <summary>
    /// Marcador de "esto es un gato" para la detección por trigger (por ejemplo, <see cref="AutoDoor"/>).
    /// Va en el prefab del gato junto a un Collider y un Rigidbody kinematic (los triggers solo
    /// se disparan si uno de los dos objetos tiene Rigidbody). No tiene lógica: sustituye a los tags.
    /// </summary>
    [DisallowMultipleComponent]
    public class CatActor : MonoBehaviour
    {
    }
}
